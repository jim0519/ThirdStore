using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using ThirdStoreCommon.Models.Order;
using ThirdStoreData;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreBusiness.API.Neto;
using ThirdStoreBusiness.API.eBay;
using System.Web;
using HtmlAgilityPack;
using ThirdStoreBusiness.JobItem;
using ThirdStoreBusiness.Item;
using ThirdStoreCommon.Models.Item;
using LINQtoCSV;

namespace ThirdStoreBusiness.Order
{
    public class OrderService:IOrderService
    {
        private readonly IRepository<D_Order_Header> _orderRepository;
        private readonly INetoAPICallManager _netoAPICallManager;
        private readonly IeBayAPICallManager _eBayAPICallManager;
        private readonly IWorkContext _workContext;
        private readonly IJobItemService _jobItemService;
        private readonly IItemService _itemService;
        private readonly CsvContext _csvContext;
        private readonly CsvFileDescription _csvFileDesc;

        public OrderService(IRepository<D_Order_Header> orderRepository,
            INetoAPICallManager netoAPICallManager,
            IeBayAPICallManager eBayAPICallManager,
            IWorkContext workContext,
            IJobItemService jobItemService,
            IItemService itemService,
            CsvContext csvContext,
            CsvFileDescription csvFileDesc)
        {
            _orderRepository = orderRepository;
            _netoAPICallManager = netoAPICallManager;
            _eBayAPICallManager = eBayAPICallManager;
            _workContext = workContext;
            _jobItemService = jobItemService;
            _itemService = itemService;
            _csvContext = csvContext;
            _csvFileDesc = csvFileDesc;
        }

        public void DeleteOrder(D_Order_Header order)
        {
            throw new NotImplementedException();
        }

        protected IList<D_Order_Header> DownloadOrders(DateTime? fromTime, DateTime? toTime)
        {
            try
            {
                var retOrders = new List<D_Order_Header>();
                var netoOrders = _netoAPICallManager.DownloadNewNetoOrder(fromTime, toTime);
                if(netoOrders!=null&&netoOrders.Count()>0)
                {
                    foreach(var netoOrder in netoOrders)
                    {
                        var existingLocalOrder = GetOrderByChannelOrderID(netoOrder.OrderID, 1);
                        if (existingLocalOrder!=null)
                        {
                            //continue;
                            retOrders.Add(UpdateLocalOrder(netoOrder, existingLocalOrder));
                        }
                        else
                        {
                            retOrders.Add(CreateNewLocalOrder(netoOrder));
                        }

                    }
                }
                return retOrders;
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        private D_Order_Header UpdateLocalOrder(GetOrderResponseOrder netoOrder,D_Order_Header existingOrder=null)
        {
            D_Order_Header updateOrder = null;
            if (existingOrder == null)
                updateOrder = GetOrderByChannelOrderID(netoOrder.OrderID, 1);
            else
                updateOrder = existingOrder;

            updateOrder.Ref1 = netoOrder.OrderStatus;
            updateOrder.SubTotal = Convert.ToDecimal(netoOrder.GrandTotal);
            updateOrder.TotalAmount = Convert.ToDecimal(netoOrder.GrandTotal);
            updateOrder.Postage = netoOrder.ShippingTotal;
            if (netoOrder.OrderPayment != null && netoOrder.OrderPayment.Count() > 0)
            {
                var orderPayment = netoOrder.OrderPayment.FirstOrDefault();
                updateOrder.PaymentMethod = orderPayment.PaymentType;
                if(netoOrder.DatePaid.HasValue)
                    updateOrder.PaidTime =  netoOrder.DatePaid.Value.ToLocalTime();
            }

            //Shipping Address
            updateOrder.ConsigneeName = $"{netoOrder.ShipFirstName} {netoOrder.ShipLastName}";
            updateOrder.ShippingAddress1 = netoOrder.ShipStreetLine1;
            updateOrder.ShippingAddress2 = netoOrder.ShipStreetLine2;
            updateOrder.ShippingSuburb = netoOrder.ShipCity;
            updateOrder.ShippingPostcode = netoOrder.ShipPostCode;
            updateOrder.ShippingState = netoOrder.ShipState;
            updateOrder.ShippingCountry = netoOrder.ShipCountry;
            updateOrder.ConsigneeEmail = netoOrder.Email;
            updateOrder.ConsigneePhoneNo = netoOrder.ShipPhone;

            //Billing Address
            updateOrder.BillingName = $"{netoOrder.BillFirstName} {netoOrder.BillLastName}";
            updateOrder.BillingAddress1 = netoOrder.BillStreetLine1;
            updateOrder.BillingAddress2 = netoOrder.BillStreetLine2;
            updateOrder.BillingSuburb = netoOrder.BillCity;
            updateOrder.BillingPostcode = netoOrder.BillPostCode;
            updateOrder.BillingState = netoOrder.BillState;
            updateOrder.BillingCountry = netoOrder.BillCountry;
            updateOrder.BillingEmail = netoOrder.Email;
            updateOrder.BillingPhoneNo = netoOrder.BillPhone;

            updateOrder.BuyerNote = netoOrder.DeliveryInstruction;
            foreach (var line in netoOrder.OrderLine)
            {
                var updateOrderLine = updateOrder.OrderLines.Where(ol => ol.Ref1.Equals(line.OrderLineID)).FirstOrDefault();
                if(updateOrderLine==null)
                {
                    updateOrderLine = new D_Order_Line();
                    updateOrder.OrderLines.Add(updateOrderLine);
                }

                updateOrderLine.ItemPrice = line.UnitPrice;
                updateOrderLine.Qty = Convert.ToInt32(line.Quantity);
                updateOrderLine.SKU = line.SKU;
                updateOrderLine.SubTotal = updateOrderLine.ItemPrice * updateOrderLine.Qty;
                updateOrderLine.Ref1 = line.OrderLineID;
                if (line.eBay != null)
                {
                    updateOrderLine.Ref2 = $"{line.eBay.eBayAuctionID}-{line.eBay.eBayTransactionID}";
                }
                updateOrderLine.Ref3 = line.ShippingMethod;
                updateOrderLine.Ref4 = line.ShippingTracking;
            }

            return this.UpdateOrder(updateOrder);
        }

        private D_Order_Header CreateNewLocalOrder(GetOrderResponseOrder netoOrder)
        {
            var newOrder = new D_Order_Header();

            newOrder.TypeID = 1;//TODO
            newOrder.StatusID = 1;//TODO
            newOrder.ChannelOrderID = netoOrder.OrderID;
            newOrder.Ref1 = netoOrder.OrderStatus;
            newOrder.SubTotal = Convert.ToDecimal(netoOrder.GrandTotal);
            newOrder.TotalAmount = Convert.ToDecimal(netoOrder.GrandTotal);
            newOrder.Postage = netoOrder.ShippingTotal;
            newOrder.OrderTime = netoOrder.DatePlaced.HasValue? netoOrder.DatePlaced.Value.ToLocalTime():DateTime.Now;
            newOrder.CustomerID = netoOrder.Username;
            if (netoOrder.OrderPayment != null&&netoOrder.OrderPayment.Count()>0)
            {
                var orderPayment = netoOrder.OrderPayment.FirstOrDefault();
                newOrder.PaymentMethod = orderPayment.PaymentType;
                if (netoOrder.DatePaid.HasValue)
                    newOrder.PaidTime = netoOrder.DatePaid.Value.ToLocalTime();
                //newOrder.PaidTime = netoOrder.DatePaid.ToLocalTime();
            }

            //Shipping Address
            newOrder.ConsigneeName = $"{netoOrder.ShipFirstName} {netoOrder.ShipLastName}";
            newOrder.ShippingAddress1 = netoOrder.ShipStreetLine1;
            newOrder.ShippingAddress2 = netoOrder.ShipStreetLine2;
            newOrder.ShippingSuburb = netoOrder.ShipCity;
            newOrder.ShippingPostcode = netoOrder.ShipPostCode;
            newOrder.ShippingState = netoOrder.ShipState;
            newOrder.ShippingCountry = netoOrder.ShipCountry;
            newOrder.ConsigneeEmail = netoOrder.Email;
            newOrder.ConsigneePhoneNo = netoOrder.ShipPhone;

            //Billing Address
            newOrder.BillingName = $"{netoOrder.BillFirstName} {netoOrder.BillLastName}";
            newOrder.BillingAddress1 = netoOrder.BillStreetLine1;
            newOrder.BillingAddress2 = netoOrder.BillStreetLine2;
            newOrder.BillingSuburb = netoOrder.BillCity;
            newOrder.BillingPostcode = netoOrder.BillPostCode;
            newOrder.BillingState = netoOrder.BillState;
            newOrder.BillingCountry = netoOrder.BillCountry;
            newOrder.BillingEmail = netoOrder.Email;
            newOrder.BillingPhoneNo = netoOrder.BillPhone;

            newOrder.BuyerNote = netoOrder.DeliveryInstruction;


            foreach (var line in netoOrder.OrderLine)
            {
                var newOrderLine = new D_Order_Line();
                newOrderLine.ItemPrice = line.UnitPrice;
                newOrderLine.Qty = Convert.ToInt32(line.Quantity);
                newOrderLine.SKU = line.SKU;
                newOrderLine.SubTotal = newOrderLine.ItemPrice * newOrderLine.Qty;
                newOrderLine.Ref1 = line.OrderLineID;
                if (line.eBay != null)
                {
                    newOrderLine.Ref2 = $"{line.eBay.eBayAuctionID}-{line.eBay.eBayTransactionID}";
                }
                newOrderLine.Ref3 = line.ShippingMethod;
                newOrderLine.Ref4 = line.ShippingTracking;

                newOrder.OrderLines.Add(newOrderLine);
            }
            return this.InsertOrder(newOrder);
        }

        public D_Order_Header GetOrderByID(int id)
        {
            return _orderRepository.GetById(id);
        }

        public D_Order_Header InsertOrder(D_Order_Header order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
            {
                currentUser = _workContext.CurrentUser.Email;
            }

            order.FillOutNull();
            order.CreateBy = currentUser;
            order.CreateTime = currentTime;
            order.EditBy = currentUser;
            order.EditTime = currentTime;

            foreach (var line in order.OrderLines)
            {
                line.FillOutNull();
                line.CreateBy = currentUser;
                line.CreateTime = currentTime;
                line.EditBy = currentUser;
                line.EditTime = currentTime;
            }

            _orderRepository.Insert(order);

            return order;
        }

        public D_Order_Header UpdateOrder(D_Order_Header order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
            {
                currentUser = _workContext.CurrentUser.Email;
            }

            order.FillOutNull();
            order.EditBy = currentUser;
            order.EditTime = currentTime;

            foreach (var line in order.OrderLines)
            {
                line.FillOutNull();
                if (line.ID > 0)
                {
                    line.EditBy = currentUser;
                    line.EditTime = currentTime;
                }
                else
                {
                    line.CreateBy = currentUser;
                    line.CreateTime = currentTime;
                    line.EditBy = currentUser;
                    line.EditTime = currentTime;
                }
            }

            _orderRepository.Update(order);

            return order;
        }

        //protected bool IsOrderDuplicated(string channelOrderID)
        //{
        //    //return _salesOrderService.GetSalesOrderByCustomIDOrderType(customID, orderType) != null;
        //    //var order = _orderRepository.Table.Where(o => o.ChannelOrderID.Equals(channelOrderID) && o.TypeID.Equals(1)).FirstOrDefault();
        //    return GetOrderByChannelOrderID(channelOrderID,1) != null;
        //}

        public void UpdateOrderDeliveryInstruction(DateTime? fromTime, DateTime? toTime)
        {
            try
            {
                var downloadedOrders = DownloadOrders(fromTime, toTime);
                //var eBayOrderLineItemIDs = from o in downloadedOrders
                //                          from line in o.OrderLines
                //                          select line.Ref2;
                var notInStatus = new string[] { GetOrderFilterOrderStatus.Dispatched.ToName(), GetOrderFilterOrderStatus.Cancelled.ToName() };
                downloadedOrders = downloadedOrders.Where(o => !IsAlreadyUpdateCustomInstructions(o.BuyerNote) && o.PaidTime != null&&!notInStatus.Contains( o.Ref1)).ToList();

                this.UpdateOrderDeliveryInstruction(downloadedOrders);
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.ToString());
                throw ex;
            }
        }

        private bool IsAlreadyUpdateCustomInstructions(string buyerNote)
        {
            //if(!string.IsNullOrEmpty( buyerNote))
            //{
            //    return true;
            //}
            //else 
            if(buyerNote.Contains(Constants.FirstInvJobItemsRef) ||buyerNote.Contains(Constants.JobItemInvList))
            {
                return true;
            }

            return false;
        }

        public void UpdateOrderDeliveryInstruction(IList<D_Order_Header> orders)
        {
            try
            {
                var updateOrderObj = new UpdateOrder();
                var updateOrderOrders = new List<UpdateOrderOrder>();
                var lstTotalAllocatedJobItemID = new List<string>();
                var lstOrderAffectedSKU = new List<string>();
                foreach (var order in orders)
                {
                    //var eBayOrderLineItemIDs = order.OrderLines.Select(l => new {LineID=l.ID,OrderLineItemID=l.Ref2 });
                    var lstCustomInstruction = new List<string>();
                    var lstAllocateInvJobItemIDs = new List<string>();
                    foreach (var orderLine in order.OrderLines)
                    {
                        if(!lstOrderAffectedSKU.Contains(orderLine.SKU))
                            lstOrderAffectedSKU.Add(orderLine.SKU);
                        var soldItemSnapshot = _eBayAPICallManager.GetSoldItemSnapshot(orderLine.Ref2);
                        if (soldItemSnapshot != null)
                        {
                            var description = HttpUtility.HtmlDecode(soldItemSnapshot.Description);
                            var htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(description);
                            var nodeJID = htmlDoc.GetElementbyId(Constants.FirstInvJobItems);
                            var nodeRef = htmlDoc.GetElementbyId(Constants.FirstInvJobItemsRef);
                            var nodeJobItemInvIDs = htmlDoc.GetElementbyId(Constants.JobItemInvIDs);
                            var orderCustomInstruction = "";
                            if (nodeJID != null&& nodeRef!=null)
                            {
                                var allocateInvJobItemIDs = nodeJID.Attributes["value"].Value;
                                var allocateInvJobItemRefs = nodeRef.InnerText;

                                lstAllocateInvJobItemIDs.Add(allocateInvJobItemIDs);

                                //if (orderLine.Qty > 1)
                                //{
                                    if (orderLine.Qty > 1&&nodeJobItemInvIDs != null)
                                    {
                                        var arrJobItemInvIDs = nodeJobItemInvIDs.Attributes["value"].Value.Split(';');
                                        //var arrJobItemInvRefs = _jobItemService.ConvertToJobItemReference(arrJobItemInvIDs);
                                        int i = 2;
                                        foreach (var jobItemInvIDs in arrJobItemInvIDs)
                                        {
                                            if (!_jobItemService.CanConsistInv(jobItemInvIDs))
                                                continue;

                                            if (i > orderLine.Qty)
                                                break;

                                            lstAllocateInvJobItemIDs.Add(jobItemInvIDs);
                                            i++;
                                        }
                                    }
                                //}

                                var allocateInvJobItems = _jobItemService.GetJobItemsByIDs(lstAllocateInvJobItemIDs.SelectMany(jiids=>jiids.Split(',')).Select(s=>Convert.ToInt32(s)).ToList());
                                orderLine.Ref5 = _jobItemService.ConvertToJobItemReference(lstAllocateInvJobItemIDs).Aggregate((current, next) => current + "," + next);
                                

                                var jobItemRefs = Constants.FirstInvJobItemsRef+": ";
                                foreach(var ji in allocateInvJobItems)
                                {
                                    var refStr = (!string.IsNullOrEmpty(ji.DesignatedSKU) ? ji.DesignatedSKU : ji.JobItemLines.FirstOrDefault().SKU) + " "+(!string.IsNullOrEmpty(ji.Location)?ji.Location+" ":string.Empty) + _jobItemService.GetJobItemReference(ji);
                                    jobItemRefs += refStr+",";
                                    //lstCustomInstruction.Add($"{Constants.FirstInvJobItemsRef}:{firstInvJobItemRefs} ");
                                }
                                jobItemRefs = jobItemRefs.TrimEnd(',');
                                //lstCustomInstruction.Add(jobItemRefs);
                                orderCustomInstruction += jobItemRefs;
                            }



                            //var nodeJL = htmlDoc.GetElementbyId(Constants.JobItemInvList);
                            if (nodeJobItemInvIDs != null)
                            {
                                var arrJobItemInvIDs = nodeJobItemInvIDs.Attributes["value"].Value.Split(';');
                                var remainingJobItemInvList = arrJobItemInvIDs.Where(jiid=> !lstAllocateInvJobItemIDs.Contains(jiid));
                                if(remainingJobItemInvList.Count()>0)
                                    orderCustomInstruction +=" "+ Constants.JobItemInvList+": "+_jobItemService.ConvertToJobItemReference(remainingJobItemInvList.ToList()).Aggregate((current, next) => current + ";" + next);
                            }

                            if(!string.IsNullOrEmpty(orderCustomInstruction))
                            {
                                lstCustomInstruction.Add(orderCustomInstruction);
                            }
                        }
                    }

                    //add allocated job item to list
                    lstTotalAllocatedJobItemID.AddRange(lstAllocateInvJobItemIDs.SelectMany(jiid => jiid.Split(',')));
                    
                    var customInstruction = (lstCustomInstruction.Count>0?"Front Door. "+lstCustomInstruction.Aggregate((current, next) => current + ";" + next):string.Empty);
                    if(!string.IsNullOrEmpty(customInstruction))
                    {
                        updateOrderOrders.Add(new UpdateOrderOrder() { OrderID = order.ChannelOrderID, ShipInstructions = order.BuyerNote+"\r\n" + customInstruction });
                        order.BuyerNote = order.BuyerNote + "\r\n" + customInstruction;
                    }
                }

                //Update Neto Custom Instruction
                updateOrderObj.Order = updateOrderOrders.ToArray();

                _netoAPICallManager.UpdateOrderDeliveryInstruction(updateOrderObj);
                //Update local orders
                _orderRepository.Update(orders);

                //Update job items status
                var jobItems= _jobItemService.GetJobItemsByIDs(lstTotalAllocatedJobItemID.Select(id=>Convert.ToInt32( id)).ToList());
                foreach(var jobItem in jobItems)
                {
                    if (jobItem.StatusID != ThirdStoreJobItemStatus.SHIPPED.ToValue()&&jobItem.StatusID!= ThirdStoreJobItemStatus.BOOKED.ToValue())
                    {
                        jobItem.StatusID = ThirdStoreJobItemStatus.ALLOCATED.ToValue();
                        _jobItemService.UpdateJobItem(jobItem);//TODO: Jobitem Edit time should not be changed if it is from logic action
                    }
                }

                var orderAffectedItems = _itemService.GetItemsBySKUs(lstOrderAffectedSKU);
                var affectedItemIDs = (from jiItemID in _jobItemService.GetAffectedItemIDsByJobItemIDs(jobItems.Select(ji => ji.ID).ToList())
                                       select jiItemID)
                                       .Union(from item in orderAffectedItems ?? new List<D_Item>()
                                              select item.ID
                    );
                if(affectedItemIDs.Count()>0)
                    _jobItemService.SyncInventory(affectedItemIDs.ToList());
                if (affectedItemIDs.Count() > 0)
                    LogManager.Instance.Info("Sync affected item ids for download order: "+affectedItemIDs.Select(id=>id.ToString()).Aggregate((current, next) => current + "," + next));
                
                
                //_jobItemService.SyncInventory(jobItems.Select(ji => ji.ID).ToArray());

                //var eBayOrderLineItemIDs = from o in orders
                //                           from line in o.OrderLines
                //                           select new KeyValuePair<int, string>(line.ID, line.Ref2);

                ////var soldItemSnapshots = _eBayAPICallManager.GetSoldItemSnapshot(eBayOrderLineItemIDs.ToList());
                //if (eBayOrderLineItemIDs != null && eBayOrderLineItemIDs.Count() > 0)
                //{
                //    foreach (var eBayOrderLineItemID in eBayOrderLineItemIDs)
                //    {
                //        var soldItemSnapshot= _eBayAPICallManager.GetSoldItemSnapshot(eBayOrderLineItemID.Value);
                //        //var orderLine = from o in orders
                //        //                from l in o.OrderLines
                //        //                where l.ID.Equals(eBayOrderLineItemID.Key)
                //        //                select l;
                //        var orderLine = orders.SelectMany(o => o.OrderLines).FirstOrDefault(l=>l.ID.Equals(eBayOrderLineItemID.Key));
                //        var description = HttpUtility.HtmlDecode(soldItemSnapshot.Description);
                //        var htmlDoc = new HtmlDocument();
                //        htmlDoc.LoadHtml(description);
                //        var node=htmlDoc.GetElementbyId("FirstInvJobItemIDs");
                //        if(node!=null)
                //        { 
                //            var firstInvJobItemIDs=node.Attributes["value"].Value;

                //            orderLine.Ref5 = firstInvJobItemIDs;
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.ToString());
                throw ex;
            }
        }

        public D_Order_Header GetOrderByChannelOrderID(string channelOrderID,int channelID)
        {
            var order = _orderRepository.Table.Where(o => o.ChannelOrderID.Equals(channelOrderID) && o.TypeID.Equals(channelID)).FirstOrDefault();
            return order;
        }

        public IPagedList<D_Order_Header> SearchOrders(
            DateTime? orderTimeFrom = null, 
            DateTime? orderTimeTo = null, 
            string channelOrderID = null, 
            string jobItemID = null, 
            string customerID=null,
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            var query = _orderRepository.Table;

            if (orderTimeFrom != null)
                query = query.Where(o => o.OrderTime >= orderTimeFrom);

            if (orderTimeTo != null)
            {
                orderTimeTo = orderTimeTo.Value.AddDays(1);
                query = query.Where(o => o.OrderTime < orderTimeTo);
            }

            if (channelOrderID != null)
                query = query.Where(o => o.ChannelOrderID.Contains(channelOrderID));

            if (customerID != null)
                query = query.Where(o => o.CustomerID.Contains(customerID));

            if(jobItemID!=null)
                query=query.Where(o => o.BuyerNote.Contains(jobItemID)||o.OrderLines.Any(l=>l.Ref5.Contains(jobItemID)));

            query = query.OrderByDescending(o => o.OrderTime);

            return new PagedList<D_Order_Header>(query, pageIndex, pageSize);
        }

        public string GetOrderScreenshot(string orderTran)
        {
            var item = _eBayAPICallManager.GetSoldItemSnapshot(orderTran);
            return item?.Description;
        }

        public Stream ExportDSZImportFile(IList<int> orderids)
        {
            Stream retStream = new MemoryStream();
            var listDSZImportLine = new List<DSZImportLine>();

            var orders = GetOrdersByIDs(orderids);
            foreach(var o in orders)
            {
                foreach(var line in o.OrderLines)
                {
                    if (string.IsNullOrWhiteSpace(line.Ref5))//item has been allocated and not local stock item
                    {
                        var importLine = new DSZImportLine();
                        importLine.serial_number = line.Ref1;
                        var firstName = o.ConsigneeName.Substring(0, o.ConsigneeName.IndexOf(" "));
                        var lastName = o.ConsigneeName.Substring(o.ConsigneeName.IndexOf(" ") + 1);
                        importLine.first_name = firstName;
                        importLine.last_name = lastName;

                        importLine.address1 = o.ShippingAddress1;
                        importLine.address2 = o.ShippingAddress2;
                        importLine.suburb = o.ShippingSuburb;
                        importLine.state = o.ShippingState;
                        importLine.country = o.ShippingCountry;
                        importLine.postcode = o.ShippingPostcode;
                        importLine.telephone = o.ConsigneePhoneNo;
                        importLine.sku = line.SKU;
                        importLine.price = 10;
                        importLine.postage = 0;
                        importLine.qty = line.Qty;
                        importLine.comment = o.BuyerNote;

                        listDSZImportLine.Add(importLine);
                    }
                }
            }

            var stWriter = new StreamWriter(retStream);
            _csvContext.Write(listDSZImportLine, stWriter, _csvFileDesc);
            stWriter.Flush();
            retStream.Position = 0;

            return retStream;
        }

        public IList<D_Order_Header> GetOrdersByIDs(IList<int> orderids)
        {
            var query = _orderRepository.Table.Where(o => orderids.Contains(o.ID));
            return query.ToList();
        }
    }
}
