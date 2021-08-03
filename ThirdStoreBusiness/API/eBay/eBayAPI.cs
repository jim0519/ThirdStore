using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThirdStoreCommon;

namespace ThirdStoreBusiness.API.eBay
{

    public class ShipmentDetail
    {
        public string OrderLineItemID { get; set; }
        public string ShipmentTrackingNumber { get; set; }
        public string ShippingCarrierUsed { get; set; }
    }

    public interface IeBayApiContextProvider
    {
        ApiContext GetAPIContext();
    }

    public interface IeBayAPICallManager
    {
        //Download New Catch Orders by period
        OrderType[] GeteBayOrdersByOrderLineItemID(IList<string> orderLineItemIDs);

        //ItemType[] GetSoldItemSnapshot(IList<string> orderLineItemIDs);

        ItemType GetSoldItemSnapshot(string orderLineItemID);

        void UpdateeBayShipment(IList<ShipmentDetail> shipmentDetails);
    }


    public class eBayAPICredentialProvider : IeBayApiContextProvider
    {
        public ApiContext GetAPIContext()
        {
            var lsteBayAPIContextConfig = ThirdStoreConfig.Instance.eBayAPIConfigList;
            var lstApiContext = new List<ApiContext>();
            foreach (var config in lsteBayAPIContextConfig)
            {
                //var apiContext = new ApiContext();
                //var apiCredential = new ApiCredential();
                //apiCredential.eBayToken = config.eBayToken;
                //apiCredential.eBayAccount.UserName = config.SellerID;
                //apiContext.SoapApiServerUrl = config.ServiceURL;


                //apiContext.ApiCredential = apiCredential;
                //apiContext.Site = (SiteCodeType)Enum.Parse(typeof(SiteCodeType), config.eBaySiteID);

                lstApiContext.Add(GetAPIContextByConfig(config));

            }
            return lstApiContext.FirstOrDefault();
        }

        private ApiContext GetAPIContextByConfig(eBayAPIContextConfig config)
        {
            var apiContext = new ApiContext();
            var apiCredential = new ApiCredential();
            apiCredential.eBayToken = config.eBayToken;
            apiCredential.eBayAccount.UserName = config.SellerID;
            apiCredential.ApiAccount.Application = config.AppID;
            apiCredential.ApiAccount.Developer = config.DevID;
            apiCredential.ApiAccount.Certificate = config.CertID;
            
            apiContext.SoapApiServerUrl = config.ServiceURL;
            //apiContext.Site = (SiteCodeType)Enum.Parse(typeof(SiteCodeType), config.eBaySiteID);
            apiContext.Site = SiteCodeType.Australia;

            apiContext.ApiCredential = apiCredential;
            
            return apiContext;
        }
    }

    public class eBayAPICallManager : IeBayAPICallManager
    {

        private readonly IeBayApiContextProvider _eBayApiContextProvider;
        public eBayAPICallManager(IeBayApiContextProvider eBayApiContextProvider)
        {
            _eBayApiContextProvider = eBayApiContextProvider;
        }

        public OrderType[] GeteBayOrdersByOrderLineItemID(IList<string> orderLineItemIDs)
        {
            try
            {
                if (orderLineItemIDs == null || orderLineItemIDs.Count == 0)
                    return default(OrderType[]);

                GetOrderTransactionsCall getOrderTransactionCall;
                GetOrderTransactionsRequestType getOrderTransactionRequest;

                DetailLevelCodeTypeCollection detailLevelColl = new DetailLevelCodeTypeCollection();
                detailLevelColl.Add(DetailLevelCodeType.ItemReturnDescription);

                var apiContext = _eBayApiContextProvider.GetAPIContext();

                List<OrderType> returnOrders = new List<OrderType>();

                getOrderTransactionCall = new GetOrderTransactionsCall(apiContext);
                getOrderTransactionRequest = new GetOrderTransactionsRequestType();
                getOrderTransactionRequest.DetailLevel = detailLevelColl;

                int pageSize = 20;
                int totalPage = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(orderLineItemIDs.Count()) / Convert.ToDecimal(pageSize)));
                int pageNumber = 1;

                do
                {
                    getOrderTransactionRequest.ItemTransactionIDArray = new ItemTransactionIDTypeCollection();
                    var lstTransactionIDs = orderLineItemIDs.Select(olid => new ItemTransactionIDType() { OrderLineItemID = olid }).Skip((pageNumber - 1) * pageSize).Take(pageSize);
                    getOrderTransactionRequest.ItemTransactionIDArray.AddRange(lstTransactionIDs.ToArray());

                    var getOrderTransactionResponse = getOrderTransactionCall.ExecuteRequest(getOrderTransactionRequest) as GetOrderTransactionsResponseType;
                    if (getOrderTransactionResponse.OrderArray != null && getOrderTransactionResponse.OrderArray.Count > 0)
                        returnOrders.AddRange(getOrderTransactionResponse.OrderArray.ToArray());

                    pageNumber++;
                } while (pageNumber <= totalPage);

                return returnOrders.ToArray();
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }


        public ItemType GetSoldItemSnapshot(string orderLineItemID)
        {
            try
            {
                var apiContext = _eBayApiContextProvider.GetAPIContext();
                var getItemCall = new GetItemCall(apiContext);
                ItemType retItem = null;
                var orderLineItemIDSpilt = orderLineItemID.Split('-');
                if (orderLineItemIDSpilt.Count() == 2)
                {
                    var detailLevel = new DetailLevelCodeTypeCollection();
                    detailLevel.Add(DetailLevelCodeType.ItemReturnDescription);
                    var getItemRequest = new GetItemRequestType() { ItemID = orderLineItemIDSpilt[0], TransactionID = orderLineItemIDSpilt[1], DetailLevel= detailLevel };
                    int failTimes = 0;
                    bool isSuccess = false;
                    do
                    {
                        try
                        {
                            var response = getItemCall.ExecuteRequest(getItemRequest) as GetItemResponseType;
                            //lstSoldItem.Add(response.Item);
                            isSuccess = true;
                            retItem = response.Item;
                        }
                        catch (Exception ex)
                        {
                            failTimes++;
                            LogManager.Instance.Error($"Get item itemID:{getItemRequest.ItemID}, transactionID:{getItemRequest.TransactionID} failed. {ex.ToString()}");
                        }
                    }
                    while (failTimes < 5&&!isSuccess);
                    //if (!isSuccess)
                    //    throw new Exception($"Get item itemID:{getItemRequest.ItemID}, transactionID:{getItemRequest.TransactionID} failed for {failTimes} times.");

                }
                return retItem;
                //return lstSoldItem.ToArray();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.ToString());
                throw ex;
            }
        }

        public void UpdateeBayShipment(IList<ShipmentDetail> shipmentDetails)
        {
            try
            {
                CompleteSaleRequestType completeSaleRequest;

                var apiContext = _eBayApiContextProvider.GetAPIContext();

                var completeSaleCall = new CompleteSaleCall(apiContext);
                foreach (var shipmentDetail in shipmentDetails)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(shipmentDetail.OrderLineItemID))
                            continue;

                        completeSaleRequest = new CompleteSaleRequestType();
                        completeSaleRequest.OrderLineItemID = shipmentDetail.OrderLineItemID;

                        if (!string.IsNullOrEmpty(shipmentDetail.ShipmentTrackingNumber) && !string.IsNullOrEmpty(shipmentDetail.ShippingCarrierUsed))
                        {
                            completeSaleRequest.Shipment = new ShipmentType();
                            completeSaleRequest.Shipment.ShipmentTrackingNumber = shipmentDetail.ShipmentTrackingNumber;
                            completeSaleRequest.Shipment.ShippingCarrierUsed = shipmentDetail.ShippingCarrierUsed;
                        }

                        completeSaleRequest.Shipped = true;

                        var completeSaleResponse = completeSaleCall.ExecuteRequest(completeSaleRequest) as CompleteSaleResponseType;
                        if (completeSaleResponse.Ack == AckCodeType.Failure || completeSaleResponse.Ack == AckCodeType.PartialFailure)
                        {
                            LogManager.Instance.Error("Update eBay Shipment Failed for Order " + shipmentDetail.OrderLineItemID + Environment.NewLine);
                            if (completeSaleResponse.Errors != null && completeSaleResponse.Errors.Count > 0)
                            {
                                LogManager.Instance.Error("Error Detail:" + shipmentDetail.OrderLineItemID + " " + completeSaleResponse.Errors[0].LongMessage + Environment.NewLine);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error("Update eBay Shipment Failed for Order " + shipmentDetail.OrderLineItemID + Environment.NewLine + ex.Message);
                        //throw ex;
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }
        }

        //public ItemType[] GetSoldItemSnapshot(IList<string> orderLineItemIDs)
        //{
        //    try
        //    {
        //        var apiContext = _eBayApiContextProvider.GetAPIContext();
        //        var getItemCall = new GetItemCall(apiContext);
        //        var lstSoldItem = new List<ItemType>();
        //        foreach (var orderLineItemID in orderLineItemIDs)
        //        {
        //            var orderLineItemIDSpilt = orderLineItemID.Split('-');
        //            if (orderLineItemIDSpilt.Count() == 3)
        //            {
        //                var getItemRequest = new GetItemRequestType() { ItemID= orderLineItemIDSpilt[0], TransactionID = orderLineItemIDSpilt[1] };
        //                int failTimes = 0;
        //                bool isSuccess = false;
        //                do
        //                {
        //                    try
        //                    {
        //                        var response = getItemCall.ExecuteRequest(getItemRequest) as GetItemResponseType;
        //                        lstSoldItem.Add(response.Item);
        //                        isSuccess = true;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        failTimes++;
        //                        LogManager.Instance.Error($"Get item itemID:{getItemRequest.ItemID}, transactionID:{getItemRequest.TransactionID} failed. {ex.ToString()}");
        //                    }
        //                }
        //                while (failTimes < 5 && !isSuccess);
        //                if (!isSuccess)
        //                    throw new Exception($"Get item itemID:{getItemRequest.ItemID}, transactionID:{getItemRequest.TransactionID} failed for {failTimes} times.");
        //            }
        //        }

        //        return lstSoldItem.ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Instance.Error(ex.ToString());
        //        throw ex;
        //    }
        //}
    }
}
