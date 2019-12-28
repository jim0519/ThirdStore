using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon.Models.JobItem;
using ThirdStoreData;
using ThirdStoreCommon;
using ThirdStoreBusiness.API.Neto;
using ThirdStoreCommon.Models.Item;
using ThirdStoreBusiness.Item;
using ThirdStoreCommon.Models.Image;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreBusiness.Order;
using ThirdStoreBusiness.Image;
using System.Data.SqlClient;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using ThirdStoreBusiness.ReportPrint;
using HtmlAgilityPack;
using LINQtoCSV;
using System.IO;

namespace ThirdStoreBusiness.JobItem
{
    public class JobItemService : IJobItemService
    {
        private readonly IRepository<D_JobItem> _jobItemRepository;
        private readonly IRepository<D_JobItemLine> _jobItemLineRepository;
        private readonly IRepository<D_Item> _itemRepository;
        private readonly IRepository<V_ItemRelationship> _itemRelationship;
        private readonly IDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly INetoAPICallManager _netoAPIManager;
        private readonly IImageService _imageService;
        private readonly IReportPrintService _reportPrintService;
        private readonly CsvContext _csvContext;
        private readonly CsvFileDescription _csvFileDescription;

        public JobItemService(IRepository<D_JobItem> jobItemRepository,
            IRepository<D_JobItemLine> jobItemLineRepository,
            IRepository<D_Item> itemRepository,
            IRepository<V_ItemRelationship> itemRelationship,
            IDbContext dbContext,
            IWorkContext workContext,
            INetoAPICallManager netoAPIManager,
            IImageService imageService,
            IReportPrintService reportPrintService)
        {
            _jobItemRepository = jobItemRepository;
            _jobItemLineRepository = jobItemLineRepository;
            _itemRepository = itemRepository;
            _itemRelationship = itemRelationship;
            _dbContext = dbContext;
            _workContext = workContext;
            _netoAPIManager = netoAPIManager;
            _imageService = imageService;
            _reportPrintService = reportPrintService;
            _csvContext = new CsvContext();
            _csvFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true, TextEncoding = Encoding.Default };
        }

        public IList<D_JobItem> GetAllJobItems()
        {
            var items = _jobItemRepository.Table.ToList();
            return items;
        }


        public IPagedList<D_JobItem> SearchJobItems(
            string id = null,
            string reference = null,
            string sku = null,
            DateTime? jobItemCreateTimeFrom = null,
            DateTime? jobItemCreateTimeTo = null,
            ThirdStoreJobItemType? jobItemType = null,
            ThirdStoreJobItemStatus? jobItemStatus = null,
            ThirdStoreJobItemCondition? jobItemCondition = null,
            ThirdStoreSupplier? jobItemSupplier = null,
            string location = null,
             List<string> inspector = null,
            string trackingNumber = null,
            int hasStocktakeTime = -1,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _jobItemRepository.Table;

            if (id != null)
            {
                int idint = 0;
                int.TryParse(id, out idint);
                query = query.Where(o => o.ID.Equals(idint));
            }

            if (jobItemCreateTimeFrom != null)
                query = query.Where(o => o.JobItemCreateTime >= jobItemCreateTimeFrom);

            if (jobItemCreateTimeTo != null)
            {
                jobItemCreateTimeTo = jobItemCreateTimeTo.Value.AddDays(1);
                query = query.Where(o => o.JobItemCreateTime < jobItemCreateTimeTo);
            }

            if (jobItemType.HasValue)
            {
                var itemTypeID = jobItemType.Value.ToValue();
                query = query.Where(i => i.Type.Equals(itemTypeID));
            }

            if (jobItemStatus.HasValue)
            {
                var itemStatusID = jobItemStatus.Value.ToValue();
                query = query.Where(i => i.StatusID.Equals(itemStatusID));
            }

            if (jobItemCondition.HasValue)
            {
                var itemConditionID = jobItemCondition.Value.ToValue();
                query = query.Where(i => i.ConditionID.Equals(itemConditionID));
            }

            if (jobItemSupplier.HasValue)
            {
                var itemSupplierID = jobItemSupplier.Value.ToValue();
                query = query.Where(i => i.JobItemLines.Any(l=>l.Item.SupplierID.Equals(itemSupplierID)));
            }

            if (location != null)
            {
                query = query.Where(i => i.Location.Contains(location));
            }

            if(inspector!=null)
            {
                //query = query.Where(i => i.Ref2.Contains(inspector));
                //var sysHoldReasonsStr = inspector.Select(r => r.ToEnumName<SystemHoldReason>());
                //query = query.Where(o => inspector.Any(s => o.Ref2.Contains(s)));
                foreach(var sp in inspector)
                {
                    query = query.Where(i => i.Ref2.Contains(sp));
                }
            }

            if (trackingNumber != null)
                query = query.Where(i => i.TrackingNumber.Contains(trackingNumber.ToLower()));

            if(hasStocktakeTime!=-1)
            {
                var blHasStocktakeTime = Convert.ToBoolean(hasStocktakeTime);
                if(blHasStocktakeTime)
                {
                    query = query.Where(i => i.StocktakeTime.HasValue);
                }
                else
                {
                    query = query.Where(i => !i.StocktakeTime.HasValue);
                }
            }

            if(reference!=null&&reference.Length>4)
            {
                var datePart = reference.Substring(0, 4);
                var numPart = reference.Substring(datePart.Length, reference.Length - datePart.Length);
                query = query.Where(ji => (DbFunctions.Right("000" + SqlFunctions.DatePart("dd", ji.CreateTime).Value, 2) + DbFunctions.Right("000" + SqlFunctions.DatePart("mm", ji.CreateTime).Value, 2)).Equals(datePart) && ji.Ref1.Equals(numPart));
            }

            if (sku != null)
            {
                query = query.Where(i => i.JobItemLines.Any(l=>l.SKU.ToLower().Contains(sku.ToLower())) || i.DesignatedSKU.ToLower().Contains(sku.ToLower()));
            }

            query = query.OrderByDescending(i => i.JobItemCreateTime);

            return new PagedList<D_JobItem>(query, pageIndex, pageSize);
        }


        public IPagedList<D_JobItem> SearchJobItems(
            int jobItemLineID = 0, 
            string jobItemReference = null, 
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            var query = _jobItemRepository.Table;

            if (jobItemLineID != 0)
            {
                query = from ji in query
                        from line in ji.JobItemLines
                        where line.ID.Equals(jobItemLineID)
                        select ji;
            }

            if(!string.IsNullOrWhiteSpace(jobItemReference))
            {
                query = GetJobItemByReference(jobItemReference, query);
            }

            query = query.OrderByDescending(i => i.JobItemCreateTime);

            return new PagedList<D_JobItem>(query, pageIndex, pageSize);

        }


        public D_JobItem GetJobItemByID(int id)
        {
            var item = _jobItemRepository.GetById(id);
            return item;
        }


        public void InsertJobItem(D_JobItem jobItem)
        {
            if (jobItem == null)
                throw new ArgumentNullException("Job item null");
            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
                currentUser = _workContext.CurrentUser.Email;

            if (string.IsNullOrEmpty( jobItem.Ref1))
                jobItem.Ref1 = GetJobItemSequenceNumber();
            jobItem.FillOutNull();
            jobItem.CreateBy = currentUser;
            if (jobItem.JobItemCreateTime.Equals(DateTime.MinValue))
                jobItem.JobItemCreateTime = currentTime;
            if (jobItem.CreateTime.Equals(DateTime.MinValue))
                jobItem.CreateTime =currentTime;
            jobItem.EditBy = currentUser;
            if (jobItem.EditTime.Equals(DateTime.MinValue))
                jobItem.EditTime = currentTime;

            foreach(var line in jobItem.JobItemLines)
            {
                line.FillOutNull();
                line.CreateBy = currentUser;
                if (line.CreateTime.Equals(DateTime.MinValue))
                    line.CreateTime = currentTime;
                line.EditBy = currentUser;
                if (line.EditTime.Equals(DateTime.MinValue))
                    line.EditTime = currentTime;
            }

            foreach(var img in jobItem.JobItemImages)
            {
                img.FillOutNull();
                img.CreateBy = currentUser;
                if (img.CreateTime.Equals(DateTime.MinValue))
                    img.CreateTime = currentTime;
                img.EditBy = currentUser;
                if (img.EditTime.Equals(DateTime.MinValue))
                    img.EditTime = currentTime;
            }

            _jobItemRepository.Insert(jobItem);
        }

        public void UpdateJobItem(D_JobItem jobItem)
        {
            if (jobItem == null)
                throw new ArgumentNullException("Job item null");

            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser!=null)
                currentUser = _workContext.CurrentUser.Email;

            //auto update shiptime when status change to shipped
            if (jobItem.StatusID == ThirdStoreJobItemStatus.SHIPPED.ToValue() && jobItem.ShipTime == null)
                jobItem.ShipTime = currentTime;

            jobItem.FillOutNull();
            jobItem.EditBy = currentUser;
            jobItem.EditTime = currentTime;


            foreach (var line in jobItem.JobItemLines)
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

            foreach (var img in jobItem.JobItemImages)
            {
                img.FillOutNull();
                if (img.ID > 0)
                {
                    img.EditBy = currentUser;
                    img.EditTime = currentTime;
                }
                else
                {
                    img.CreateBy = currentUser;
                    img.CreateTime = currentTime;
                    img.EditBy = currentUser;
                    img.EditTime = currentTime;
                }
            }

            _jobItemRepository.Update(jobItem);
        }

        public void DeleteJobItem(D_JobItem jobItem)
        {
            throw new NotImplementedException();
        }

        public void GetNetoProducts()
        {
            _netoAPIManager.GetNetoProducts();
        }

        public ThirdStoreReturnMessage SyncInventory(IList<int> itemids=null)
        {
            try
            {
                #region Calculate Product Inventory
                //var itemRelationship = _itemService.GetAllItemsWithRelationship();
                var notInItemType = new int[] { ThirdStoreItemType.PART.ToValue() };
                //var listingItems = from item in _itemRepository.Table
                //                   where item.IsActive && !item.IgnoreListing && !notInItemType.Contains( item.Type)
                //                   select item;
                var qlistingItems = _itemRepository.Table;
                qlistingItems = qlistingItems.Where(itm => itm.IsActive && itm.IsReadyForList && !notInItemType.Contains(itm.Type));
                if (itemids != null && itemids.Count > 0)
                    qlistingItems = qlistingItems.Where(itm => itemids.Contains(itm.ID));

                var listingItems = qlistingItems.ToList();

                //var notInStatus = new int[] { ThirdStoreJobItemStatus.PENDING.ToValue(), ThirdStoreJobItemStatus.SHIPPED.ToValue() };
                var inStatus = new int[] { ThirdStoreJobItemStatus.READY.ToValue(), ThirdStoreJobItemStatus.ALLOCATED.ToValue() };
                var inType = new int[] { ThirdStoreJobItemType.SELFSTORED.ToValue() };
                var inventory = from jobItem in _jobItemRepository.Table
                                from jobItemLine in jobItem.JobItemLines
                                where inStatus.Contains(jobItem.StatusID) && inType.Contains(jobItem.Type)
                                orderby jobItem.JobItemCreateTime
                                select new
                                {
                                    JobItemID = jobItem.ID,
                                    jobItemLine.ItemID,
                                    jobItemLine.SKU,
                                    jobItemLine.Qty,
                                    jobItemLine.Weight,
                                    jobItemLine.Length,
                                    jobItemLine.Width,
                                    jobItemLine.Height,
                                    jobItemLine.CubicWeight,
                                    jobItem.ConditionID,
                                    jobItem.JobItemCreateTime,
                                    jobItem.StatusID,
                                    jobItem.ItemName,
                                    jobItem.Note,
                                    jobItem.ItemPrice,
                                    jobItem.DesignatedSKU,
                                    jobItem.CreateTime,
                                    JobItemReference= jobItem.Ref1
                                };

                var queryItem = (from item in listingItems
                                 join itemR in _itemRelationship.Table on item.ID equals itemR.ItemID
                                group itemR by new { itemR.ItemID } into grpItemR
                                select grpItemR).ToList();

                var queryInventory = (from unit in inventory
                                     join itemR in _itemRelationship.Table on unit.ItemID equals itemR.ItemID
                                     group new { unit, itemR } by new { unit.JobItemID,unit.JobItemCreateTime,unit.StatusID, unit.DesignatedSKU, unit.ConditionID, unit.CreateTime, unit.JobItemReference,itemR.ItemID,itemR.Qty, itemR.BottomItemID } into grpInventory
                                     orderby grpInventory.Key.JobItemCreateTime
                                     select new JobItemInv
                                     {
                                         JobItemID=grpInventory.Key.JobItemID,
                                         CreateTime=grpInventory.Key.CreateTime,
                                         StatusID=grpInventory.Key.StatusID,
                                         DesignatedSKU=grpInventory.Key.DesignatedSKU,
                                         ConditionID=grpInventory.Key.ConditionID,
                                         JobItemReference=grpInventory.Key.JobItemReference,
                                         ItemID=grpInventory.Key.ItemID,
                                         RelationQty=grpInventory.Key.Qty,
                                         BottomItemID=grpInventory.Key.BottomItemID,
                                         SumQty = grpInventory.Sum(l => l.unit.Qty * l.itemR.Qty)

                                     }).ToList();


                var lstExportProductListing = new List<ExportProductListing>();
                foreach (var parentItem in queryItem)
                {
                    var conditionList = new int[] { ThirdStoreJobItemCondition.NEW.ToValue(), ThirdStoreJobItemCondition.USED.ToValue() };
                    var itemBottomQty = from itemRLine in parentItem
                                        from conditionID in conditionList
                                        join inv in queryInventory on new { itemRLine.BottomItemID, ConditionID = conditionID } equals new { inv.BottomItemID, inv.ConditionID } into leftJoinItemInv
                                        from itmRInv in leftJoinItemInv.DefaultIfEmpty()
                                        select new
                                        {
                                            ItmSection = itemRLine,
                                            InvSection = (itmRInv == null||(!string.IsNullOrEmpty( itmRInv.DesignatedSKU)&&itmRInv.DesignatedSKU!=itemRLine.SKU) ? new JobItemInv { JobItemID = 0,CreateTime=DateTime.MinValue, StatusID=ThirdStoreJobItemStatus.PENDING.ToValue(), DesignatedSKU = string.Empty, ConditionID = conditionID, JobItemReference=string.Empty, ItemID=itemRLine.ItemID, RelationQty=itemRLine.Qty, BottomItemID=itemRLine.BottomItemID, SumQty = 0 } : itmRInv)
                                        };

                    var itemSumBottomQty = from ibq in itemBottomQty
                                           group new { ibq } by new { ibq.ItmSection.ItemID, ibq.ItmSection.Qty, ibq.InvSection.BottomItemID, ibq.InvSection.ConditionID } into grpBottomItemWInv
                                           select new
                                           {
                                               grpBottomItemWInv.Key.ItemID,
                                               RelationQty = grpBottomItemWInv.Key.Qty,
                                               grpBottomItemWInv.Key.BottomItemID,
                                               grpBottomItemWInv.Key.ConditionID,
                                               SumQty = grpBottomItemWInv.Sum(l => (l.ibq.ItmSection.Qty % l.ibq.InvSection.RelationQty == 0 ? l.ibq.InvSection.SumQty : 0))
                                               //SumQty = grpBottomItemWInv.Sum(l => (l.ibq.ItmSection.Qty % l.ibq.InvSection.RelationQty == 0&&IsItemAEqualToOrContainItemB(l.ibq.InvSection.ItemID, l.ibq.ItmSection.ItemID, queryItem.SelectMany(grp => grp)) ? l.ibq.InvSection.SumQty : 0))
                                           };

                    var totalItemQtyWCondition = from isbq in itemSumBottomQty
                                                 group isbq by new { isbq.ItemID, isbq.ConditionID } into grpISBQ
                                                 select new
                                                 {
                                                     grpISBQ.Key.ItemID,
                                                     grpISBQ.Key.ConditionID,
                                                     TotalQty = grpISBQ.Min(l => (l.SumQty / l.RelationQty))
                                                 };

                    foreach (var itmInv in totalItemQtyWCondition)
                    {
                        var strSKUSuffix = parentItem.FirstOrDefault().SKU;
                        var itm = listingItems.FirstOrDefault(i => i.ID.Equals(parentItem.FirstOrDefault().ItemID));
                        if (itmInv.ConditionID.Equals(ThirdStoreJobItemCondition.USED.ToValue()) )
                        {
                            if ((strSKUSuffix + Constants.UsedSKUSuffix).Length <= 25)
                            {
                                strSKUSuffix += Constants.UsedSKUSuffix;
                            }
                            else if ((strSKUSuffix + Constants.UsedSKUSuffix).Length > 25 && !string.IsNullOrWhiteSpace(itm.Ref2))
                            {
                                strSKUSuffix = itm.Ref2.Trim() + Constants.UsedSKUSuffix;
                            }
                            else
                            {
                                LogManager.Instance.Error($"SKU {strSKUSuffix + Constants.UsedSKUSuffix} length is longer than 25");
                                continue;
                            }
                        }
                        else
                        {
                            if(strSKUSuffix.Length>25  )
                            {
                                if (!string.IsNullOrWhiteSpace(itm.Ref2))
                                {
                                    strSKUSuffix = itm.Ref2.Trim();
                                }
                                else
                                {
                                    LogManager.Instance.Error($"SKU {strSKUSuffix} length is longer than 25");
                                    continue;
                                }
                            }
                        }

                        var addProductListing=new ExportProductListing() {ItemID=parentItem.FirstOrDefault().ItemID, SKU = parentItem.FirstOrDefault().SKU, Condition = itmInv.ConditionID.ToEnumName<ThirdStoreJobItemCondition>(), Qty = itmInv.TotalQty, SKUWSuffix = strSKUSuffix };
                        if (itmInv.TotalQty>0)
                        {
                            #region New Calculate Inv Job Items
                            var relatedInv = from jobItmInv in queryInventory
                                             join itemR in parentItem on jobItmInv.BottomItemID equals itemR.BottomItemID
                                             where jobItmInv.ConditionID.Equals(itmInv.ConditionID)
                                             && (string.IsNullOrEmpty(jobItmInv.DesignatedSKU) || jobItmInv.DesignatedSKU.ToLower().Equals(itemR.SKU.ToLower()))
                                                        && itemR.Qty % jobItmInv.RelationQty == 0
                                                        && jobItmInv.StatusID != ThirdStoreJobItemStatus.ALLOCATED.ToValue()
                                             select jobItmInv;

                            var invJobItemIDs = SortInvJobItemIDs(relatedInv.ToList(), parentItem.Select(i => i).ToList());
                            //var invJobItemReferences = ConvertToJobItemReference(invJobItemIDs);

                            //addProductListing.FirstInvJobItemIDs= (CanConsistInv(invJobItemIDs.FirstOrDefault())? invJobItemIDs.FirstOrDefault():string.Empty);
                            addProductListing.FirstInvJobItemIDs = invJobItemIDs.FirstOrDefault(jids => CanConsistInv(jids))??string.Empty;
                            if (string.IsNullOrEmpty(addProductListing.FirstInvJobItemIDs)&& invJobItemIDs.Count>0)
                            {
                                addProductListing.JobItemInvIDs = invJobItemIDs.Aggregate((current, next) => current + ";" + next);
                            }
                            else
                            {
                                if (invJobItemIDs.Where(i => !i.Equals(addProductListing.FirstInvJobItemIDs)).Count() > 0)
                                    addProductListing.JobItemInvIDs = invJobItemIDs.Where(i => !i.Equals(addProductListing.FirstInvJobItemIDs)).Aggregate((current, next) => current + ";" + next);
                            }
                            if (!string.IsNullOrEmpty(addProductListing.JobItemInvIDs))
                                addProductListing.JobItemInvList = ConvertToJobItemReference(addProductListing.JobItemInvIDs.Split(';')).Aggregate((current, next) => current + ";" + next);

                            #endregion

                            //#region Old Calculate Inv Job Items
                            ////var dicStockList = new Dictionary<int, int[]>();

                            ////for (int i = 1; i <= itmInv.TotalQty; i++)
                            ////{
                            //var firstItemJobItemIDs = new List<int>();
                            //foreach (var itemR in parentItem)
                            //{
                            //    var itemInvWCond = from jobItmInv in queryInventory
                            //                       where jobItmInv.ConditionID.Equals(itmInv.ConditionID)
                            //                       && jobItmInv.BottomItemID.Equals(itemR.BottomItemID)
                            //                       && (string.IsNullOrEmpty(jobItmInv.DesignatedSKU) || jobItmInv.DesignatedSKU.ToLower().Equals(itemR.SKU.ToLower()))
                            //                       && itemR.Qty % jobItmInv.RelationQty == 0
                            //                       && jobItmInv.StatusID != 3//not in Allocated
                            //                                                 //&& !dicStockList.SelectMany(sl=>sl.Value).Contains( jobItmInv.JobItemID)
                            //                       select jobItmInv;
                            //    //||aggrQty<=line.SumQty
                            //    var jobItemIDs = itemInvWCond.TakeWhileAggregate(0, (aggrQty, line) => aggrQty + line.SumQty, (aggrQty, line) => aggrQty < itemR.Qty, (aggrQty, line) => aggrQty > itemR.Qty).Select(line => line.JobItemID);//TODO: Sum Qty for the first item need to be exact itemR.Qty
                            //    if (jobItemIDs.Count() == 0)
                            //    {
                            //        firstItemJobItemIDs.Clear();
                            //        break;
                            //    }
                            //    foreach (var jiID in jobItemIDs)
                            //        if (!firstItemJobItemIDs.Contains(jiID))
                            //            firstItemJobItemIDs.Add(jiID);
                            //}
                            ////}
                            ////.Add(new ExportProductListing() { SKU = parentItem.FirstOrDefault().SKU, Condition = itmInv.ConditionID.ToEnumName<ThirdStoreJobItemCondition>(), Qty = itmInv.TotalQty, FirstInvJobItemIDs = string.Empty });
                            //var arrFirstItemJobItemIDs = firstItemJobItemIDs.ToArray();
                            ////var ss = (from jobItmInv in queryInventory
                            ////          join itemR in parentItem.AsQueryable() on jobItmInv.BottomItemID equals itemR.BottomItemID
                            ////          select new { jobItmInv.JobItemID }).ToList();
                            //var continueJobItemIDs = (from jobItmInv in queryInventory
                            //                          join itemR in parentItem on jobItmInv.BottomItemID equals itemR.BottomItemID
                            //                          where jobItmInv.ConditionID.Equals(itmInv.ConditionID) && !arrFirstItemJobItemIDs.Contains(jobItmInv.JobItemID)
                            //                          && (string.IsNullOrEmpty(jobItmInv.DesignatedSKU) || jobItmInv.DesignatedSKU.ToLower().Equals(itemR.SKU.ToLower()))
                            //                          && itemR.Qty % jobItmInv.RelationQty == 0
                            //                          && jobItmInv.StatusID != 3//not in Allocated
                            //                          select GetJobItemReference(jobItmInv.CreateTime, jobItmInv.JobItemReference)).Distinct().ToList();
                            //addProductListing.FirstInvJobItemIDs = (firstItemJobItemIDs.Count > 0 ? firstItemJobItemIDs.Select(id => id.ToString()).Aggregate((current, next) => current + "," + next) : string.Empty);
                            //addProductListing.JobItemInvList = (continueJobItemIDs.Count() > 0 ? continueJobItemIDs.Aggregate((current, next) => current + ";" + next) : string.Empty);

                            //#endregion
                        }
                        else
                        {
                            addProductListing.FirstInvJobItemIDs = string.Empty;
                            addProductListing.JobItemInvList = string.Empty;
                        }
                        lstExportProductListing.Add(addProductListing);

                    }

                   
                }

                //lstExportProductListing = lstExportProductListing.Where(l => (l.SKU == "TestAPIAddSKU-5" || l.SKU == "TestAPIAddSKU") && l.Condition == ThirdStoreJobItemCondition.NEW.ToName()).ToList();

                #endregion

                #region Get Online Products from Neto
                var syncSKUs = lstExportProductListing.Select(epl => epl.SKUWSuffix).ToArray();
                //var onlineListings = _netoAPIManager.GetNetoProducts(true);
                var onlineListings = _netoAPIManager.GetNetoProductBySKUs(syncSKUs,true);
                //var onlineListings = _netoAPIManager.GetNetoProductBySKUs(new string[] 
                //{
                //    "TestAPIAddSKU",
                //    "TestAPIAddSKU-DEF",
                //    "TestAPIAddSKU-5",
                //    "TestAPIAddSKU-5-DEF",
                //    "TestAPISKU-NewImage",
                //    "TestAPISKU-NewImage-DEF"
                //});
                #endregion

                #region Finalize ExportProductListing (decide what qty need to be sync)

                var di = new DirectoryInfo(ThirdStoreConfig.Instance.ThirdStoreDSZData);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles().ToArray();
                    if (files.Count() > 0)
                    {
                        var dszDataFile = files.OrderByDescending(fi => fi.CreationTime).FirstOrDefault();
                        var dszData = _csvContext.Read<DSZSKUModel>(dszDataFile.FullName, _csvFileDescription);
                        var tmpExportProductListing = new List<ExportProductListing>();
                        foreach (var epl in lstExportProductListing)
                        {
                            if (epl.Condition.Equals(ThirdStoreJobItemCondition.NEW.ToName())
                                && epl.Qty == 0)
                            {
                                var dszSKU = dszData.FirstOrDefault(d => d.SKU.ToLower().Equals(epl.SKU.ToLower()));
                                var dsInventoryThredshold = Convert.ToInt32(ThirdStoreConfig.Instance.DSInventoryThreshold);
                                if (dszSKU != null
                                    && dszSKU.InventoryQty >= dsInventoryThredshold
                                    && dszSKU.Price<=Convert.ToDecimal( ThirdStoreConfig.Instance.SyncDSPriceBelow))
                                {
                                    epl.Qty = dsInventoryThredshold;
                                }
                            }
                            tmpExportProductListing.Add(epl);
                        }
                        lstExportProductListing = tmpExportProductListing;
                    }
                }


                #endregion

                #region Generate Product Inventory Feed
                //var addProductFeed=new product

                #region Update Listings

                var queryBase = (from localListing in lstExportProductListing
                                from firstJobItmID in localListing.FirstInvJobItemIDs.Split(',').Select(id => Convert.ToInt32((string.IsNullOrEmpty( id)?"0":id)))
                                 join item in listingItems on localListing.ItemID equals item.ID
                                 join jobItem in _jobItemRepository.Table on firstJobItmID equals jobItem.ID into leftJoinJobItem
                                 from ljJobItem in leftJoinJobItem.DefaultIfEmpty()
                                
                                select new { localListing, jobItem=ljJobItem, item }).ToList();

                var updates = (from record in queryBase
                              //where onlineListings.Any(ol => ol.SKU.ToLower().Equals(record.localListing.SKUWSuffix.ToLower()))
                              join ol in onlineListings on record.localListing.SKUWSuffix.ToLower() equals ol.SKU.ToLower()
                               group new { record.localListing, record.jobItem, record.item,onlineListing=ol } by new { record.localListing.SKUWSuffix } into grpUpdates
                               //where grpUpdates.FirstOrDefault().localListing.Condition.Equals(ThirdStoreJobItemCondition.NEW.ToName())
                               //|| grpUpdates.FirstOrDefault().jobItem != null//no need to sync if condition is used and do not have first job item inv
                               select new
                              {
                                  SKU = grpUpdates.Key.SKUWSuffix,
                                  Name = (grpUpdates.FirstOrDefault().localListing.Condition.Equals(ThirdStoreJobItemCondition.NEW.ToName()) ? string.Empty : Constants.UsedNameSuffix)+(grpUpdates.FirstOrDefault().jobItem!=null&& !string.IsNullOrWhiteSpace( grpUpdates.FirstOrDefault().jobItem.ItemName)? grpUpdates.FirstOrDefault().jobItem.ItemName: grpUpdates.FirstOrDefault().item.Name),
                                  DefaultPrice = (grpUpdates.FirstOrDefault().jobItem!=null? grpUpdates.Sum(ji => ji.jobItem.ItemPrice*(ji.jobItem.PricePercentage>0?ji.jobItem.PricePercentage:1)):grpUpdates.FirstOrDefault().item.Price),
                                  Description = GenerateProductDesc(grpUpdates.FirstOrDefault().localListing, (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.Select(grp => grp.jobItem) : null), grpUpdates.FirstOrDefault().item, grpUpdates.FirstOrDefault().onlineListing),//TODO: Add first quantity job item ids and rest ids
                                  Images = GenerateUpdateImages(grpUpdates.FirstOrDefault().localListing, (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.Select(grp => grp.jobItem) : null), grpUpdates.FirstOrDefault().item, grpUpdates.FirstOrDefault().onlineListing),
                                  ShippingHeight = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.SelectMany(ji=>ji.jobItem.JobItemLines).Max(jil=>jil.Height) : grpUpdates.FirstOrDefault().item.Height),//TODO: Determine which height should be used
                                  ShippingLength = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.SelectMany(ji => ji.jobItem.JobItemLines).Max(jil => jil.Length) : grpUpdates.FirstOrDefault().item.Length),
                                  ShippingWidth = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.SelectMany(ji => ji.jobItem.JobItemLines).Max(jil => jil.Width) : grpUpdates.FirstOrDefault().item.Width),
                                  ShippingWeight = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.SelectMany(ji => ji.jobItem.JobItemLines).Max(jil => jil.Weight) : grpUpdates.FirstOrDefault().item.GrossWeight),
                                  //PrimarySupplier = grpUpdates.FirstOrDefault().item.SupplierID.ToEnumName<ThirdStoreSupplier>(),
                                  //ListingTemplateID=(grpUpdates.FirstOrDefault().jobItem.ConditionID==ThirdStoreJobItemCondition.NEW.ToValue()?1:8),
                                  Quantity=grpUpdates.FirstOrDefault().localListing.Qty,
                                  UPC= grpUpdates.FirstOrDefault().item.Ref3

                               })
                              //.Where(u=>u.SKU.Contains("PIPE-CHAIR-511-BKX2"))
                              .ToList();

                //var updates = from localListing in lstExportProductListing
                //              from firstJobItmID in localListing.FirstInvJobItemIDs.Split(',').Select(id => Convert.ToInt32(id))
                //              join onlineListing in onlineListings on new { SKU =localListing.SKUWSuffix.ToLower() } equals new { SKU = onlineListing.SKU.ToLower() }
                //              join jobItem in _jobItemRepository.Table on firstJobItmID equals jobItem.ID
                //              join item in _itemRepository.Table on localListing.ItemID equals item.ID
                //              group new { localListing, jobItem, item } by new { localListing.SKUWSuffix } into grpUpdates
                //              select new
                //              {
                //                  SKU = grpUpdates.Key.SKUWSuffix,
                //                  Name = grpUpdates.FirstOrDefault().item.Name + (grpUpdates.FirstOrDefault().localListing.Condition.Equals(ThirdStoreJobItemCondition.NEW.ToName()) ? string.Empty : "-USED/DEFECT"),
                //                  DefaultPrice = grpUpdates.Sum(ji => ji.jobItem.ItemPrice)
                //              };

                #endregion

                #region Delete Listings
                //TODO
                #endregion

                #region Add Listings
                var adds = (from record in queryBase
                            where !onlineListings.Any(ol => ol.SKU.ToLower().Equals(record.localListing.SKUWSuffix.ToLower()))
                            group new { record.localListing, record.jobItem, record.item } by new { record.localListing.SKUWSuffix } into grpUpdates
                            select new
                            {
                                SKU = grpUpdates.Key.SKUWSuffix,
                                Name = (grpUpdates.FirstOrDefault().localListing.Condition.Equals(ThirdStoreJobItemCondition.NEW.ToName()) ? string.Empty : Constants.UsedNameSuffix) + (grpUpdates.FirstOrDefault().jobItem != null && !string.IsNullOrWhiteSpace(grpUpdates.FirstOrDefault().jobItem.ItemName) ? grpUpdates.FirstOrDefault().jobItem.ItemName : grpUpdates.FirstOrDefault().item.Name),
                                DefaultPrice = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.Sum(ji => ji.jobItem.ItemPrice * (ji.jobItem.PricePercentage > 0 ? ji.jobItem.PricePercentage : 1)) : grpUpdates.FirstOrDefault().item.Price),
                                Description = GenerateProductDesc(grpUpdates.FirstOrDefault().localListing, (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.Select(grp => grp.jobItem) : null), grpUpdates.FirstOrDefault().item),//TODO: Add first quantity job item ids and rest ids
                                Images = GenerateAddImages(GenerateUpdateImages(grpUpdates.FirstOrDefault().localListing, (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.Select(grp => grp.jobItem):null), grpUpdates.FirstOrDefault().item)),
                                ShippingHeight = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.SelectMany(ji => ji.jobItem.JobItemLines).Max(jil => jil.Height) : grpUpdates.FirstOrDefault().item.Height),//TODO: Determine which height should be used
                                ShippingLength = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.SelectMany(ji => ji.jobItem.JobItemLines).Max(jil => jil.Length) : grpUpdates.FirstOrDefault().item.Length),
                                ShippingWidth = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.SelectMany(ji => ji.jobItem.JobItemLines).Max(jil => jil.Width) : grpUpdates.FirstOrDefault().item.Width),
                                ShippingWeight = (grpUpdates.FirstOrDefault().jobItem != null ? grpUpdates.SelectMany(ji => ji.jobItem.JobItemLines).Max(jil => jil.Weight) : grpUpdates.FirstOrDefault().item.GrossWeight),
                                PrimarySupplier = grpUpdates.FirstOrDefault().item.SupplierID.ToEnumName<ThirdStoreSupplier>(),
                                ListingTemplateID =(grpUpdates.FirstOrDefault().localListing.Condition== ThirdStoreJobItemCondition.NEW.ToName()?5:2),
                                Quantity = grpUpdates.FirstOrDefault().localListing.Qty,
                                UPC = grpUpdates.FirstOrDefault().item.Ref3
                            }).ToList() ;
                           #endregion

                //var localCrossNeto = from localListing in lstExportProductListing
                //                     join onlineListing in onlineListings on new { SKU = (localListing.Condition.Equals(ThirdStoreJobItemCondition.NEW.ToName()) ? localListing.SKU.ToLower() : (localListing.SKU + Constants.UsedSKUSuffix).ToLower()) } equals new { SKU = onlineListing.SKU.ToLower() } into leftJoinOnlineListing
                //                     from resultListing in leftJoinOnlineListing.DefaultIfEmpty()
                //                     select new
                //                     {
                //                         SKU = (localListing.Condition.Equals(ThirdStoreJobItemCondition.NEW.ToName()) ? localListing.SKU.ToUpper() : (localListing.SKU + Constants.UsedSKUSuffix).ToUpper()),
                //                         localListing.Condition,
                //                         localListing.Qty,
                //                         localListing.FirstInvJobItemIDs,
                //                         localListing.JobItemInvList,
                //                         OnlineListing = resultListing
                //                     };

                //var addListings=from lcn in localCrossNeto
                //                from firstJobItmID in lcn.FirstInvJobItemIDs.Split(',')
                //                join jit in _jobItemRepository.Table on firstJobItmID equals jit.ID

                var updateItem = new UpdateItem();
                var updateItems = new List<UpdateItemItem>();
                
                //foreach(var updateListing in lstExportProductListing)
                //{
                //    var warehouseQty = new UpdateItemItemWarehouseQuantity[] { new UpdateItemItemWarehouseQuantity() { WarehouseID = 1.ToString(), Quantity = updateListing.Qty.ToString(), Action = UpdateItemItemWarehouseQuantityAction.set } };
                    
                //    var itm = new UpdateItemItem() { SKU = updateListing.SKU, WarehouseQuantity = warehouseQty };
                //    //var images = new UpdateItemItemImage[] { new UpdateItemItemImage() { Name = "Alt 1", URL = "https://i.ebayimg.com/images/g/NDIAAOSwVExaohG~/s-l300.jpg",Delete=true } };
                //    //var imageObj = new UpdateItemItemImages();
                //    //imageObj.Image = images;
                //    //itm.Images = imageObj;
                //    updateItems.Add(itm);
                //}

                foreach(var updateObj in updates)
                {
                    var itm = new UpdateItemItem();
                    itm.SKU = updateObj.SKU;
                    itm.Name = updateObj.Name;
                    itm.DefaultPrice = updateObj.DefaultPrice;
                    itm.IsActive = true;
                    itm.Description = updateObj.Description;
                    //if(updateObj.Images!=null)
                        itm.Images = updateObj.Images;
                    itm.ShippingHeight = updateObj.ShippingHeight;
                    itm.ShippingLength = updateObj.ShippingLength;
                    itm.ShippingWidth = updateObj.ShippingWidth;
                    itm.ShippingWeight = updateObj.ShippingWeight;
                    var warehouseQty = new UpdateItemItemWarehouseQuantity[] { new UpdateItemItemWarehouseQuantity() { WarehouseID = 1.ToString(), Quantity = updateObj.Quantity.ToString(), Action = UpdateItemItemWarehouseQuantityAction.set } };
                    itm.WarehouseQuantity = warehouseQty;
                    itm.UPC = updateObj.UPC;

                    updateItems.Add(itm);
                }

                updateItem.Item = updateItems
                    //.Where(ai => ai.SKU.Contains("BA-I-3933-BK"))
                    .ToArray();

                var updateReturnMessage = _netoAPIManager.UpdateProducts(updateItem);


                var addItem = new AddItem();
                var addItems = new List<AddItemItem>();
                foreach(var addObj in adds)
                {
                    var itm = new AddItemItem();
                    itm.SKU = addObj.SKU;
                    itm.Name = addObj.Name;
                    itm.DefaultPrice = addObj.DefaultPrice;
                    itm.IsActive = true;
                    itm.Description = addObj.Description;
                    //if (addObj.Images != null)
                        itm.Images = addObj.Images;
                    itm.ShippingHeight = addObj.ShippingHeight;
                    itm.ShippingLength = addObj.ShippingLength;
                    itm.ShippingWidth = addObj.ShippingWidth;
                    itm.ShippingWeight = addObj.ShippingWeight;
                    itm.PrimarySupplier = addObj.PrimarySupplier;
                    var ebayItems = new AddItemItemEBayItems();
                    ebayItems.eBayItem = new AddItemItemEBayItem[] { new AddItemItemEBayItem() { ListingTemplateID=addObj.ListingTemplateID.ToString() } };
                    itm.eBayItems = ebayItems;

                    var warehouseQty = new AddItemItemWarehouseQuantity[] { new AddItemItemWarehouseQuantity() { WarehouseID = 1.ToString(), Quantity = addObj.Quantity.ToString(), Action = UpdateItemItemWarehouseQuantityAction.set } };
                    itm.WarehouseQuantity = warehouseQty;

                    itm.UPC = addObj.UPC;

                    addItems.Add(itm);
                }
                addItem.Item = addItems
                    //.Where(ai=>ai.SKU== "BFRAME-E-TINO-Q-CHAR-AB")
                    .ToArray();

                var addItemReturnMessage = _netoAPIManager.AddProducts(addItem);

                #endregion

                //return addItemReturnMessage;
                return new ThirdStoreReturnMessage() { IsSuccess = true};
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return new ThirdStoreReturnMessage() { IsSuccess = false,Mesage=ex.Message};
            }
        }

        public IList<string> ConvertToJobItemReference(IList<string> invJobItemIDs)
        {
            var retList = new List<string>();
            if (invJobItemIDs == null || invJobItemIDs.Count == 0)
                return retList;

            foreach(var jobItemIDs in invJobItemIDs)
            {
                //if(jobItemIDs.IndexOf(',')!=-1)
                //{
                //    retList.Add(jobItemIDs.Split(',').Select(id => GetJobItemReference(Convert.ToInt32(id))).Aggregate((current, next) => current + "," + next));
                //}
                //else if(jobItemIDs.IndexOf('-') != -1)
                //{
                //    retList.Add(jobItemIDs.Split('-').Select(id => GetJobItemReference(Convert.ToInt32(id))).Aggregate((current, next) => current + "-" + next));
                //}
                //else
                //{
                //    retList.Add(jobItemIDs);
                //}
                retList.Add(jobItemIDs.Split(',').Select(id => (id.IndexOfAny(new char[] { '[', ']' }) != -1 ? "[" + GetJobItemReference(Convert.ToInt32(id.Replace("[", "").Replace("]", ""))) + "]" : GetJobItemReference(Convert.ToInt32(id)))).Aggregate((current, next) => current + "," + next));

            }
            return retList;
        }

        public bool CanConsistInv(string jobItemIDs)
        {
            return jobItemIDs.IndexOfAny(new char[] { '[', ']' }) == -1;
        }

        private IList<string> SortInvJobItemIDs(IList<JobItemInv> relatedInv,IList<V_ItemRelationship> itemStructure)
        {
            var retList = new List<string>();
            if (relatedInv == null || relatedInv.Count == 0)
                return retList;
            var grpRelatedInv = relatedInv.OrderBy(ri => ri.CreateTime).GroupBy(ri => ri.JobItemID);
            //var grpRelatedInv = relatedInv.OrderBy(ri => ri.CreateTime).GroupBy(ri => ri.JobItemID).OrderByDescending(g => g.Count()).ThenBy(g => g.k);
            var remainingJobItemInvs = new List<JobItemInv>();
            //var waitForProcess = new List<JobItemInv>();
            //var inProcess = new List<JobItemInv>();
            var grpJobItemInvs = grpRelatedInv.Where(grp => (grp.Count() > 1)).SelectMany(grp => grp);
            var notDesignatedJobItemInvs = grpRelatedInv.SelectMany(grp => grp).Where(ji=> !grpJobItemInvs.Contains(ji));
            if (grpRelatedInv.FirstOrDefault().Count()>1)
            {
                //grpRelatedInv.Where(grp => !grp.Equals(grp.FirstOrDefault()));
                var firstGrpJobItems = grpRelatedInv.FirstOrDefault().Select(grp => grp);
                retList.Add(grpRelatedInv.FirstOrDefault().Key.ToString());
                remainingJobItemInvs = grpRelatedInv.SelectMany(grp => grp).Where(ji => !firstGrpJobItems.Contains(ji)).ToList();
            }
            else
            {
                //var notDesignatedJobItemInvs = grpNotDesignatedJobItemInvs.SelectMany(grp => grp);
                var firstItemJobItemIDs = new List<int>();
                //var notSufficient = false;
                foreach (var itemR in itemStructure)
                {
                    //||aggrQty<=line.SumQty
                    var itemInvs = from jobItmInv in notDesignatedJobItemInvs
                                   where jobItmInv.BottomItemID.Equals(itemR.BottomItemID)
                                  select jobItmInv;
                    var jobItemIDs = GetFulfillJobItemIDs(itemInvs,itemR.Qty).Select(ji => ji.JobItemID);
                    if (jobItemIDs.Count() == 0)
                    {
                        firstItemJobItemIDs.Clear();
                        break;
                    }
                    foreach (var jiID in jobItemIDs)
                        if (!firstItemJobItemIDs.Contains(jiID))
                            firstItemJobItemIDs.Add(jiID);
                }
                if(firstItemJobItemIDs.Count>0)
                {
                    retList.Add(firstItemJobItemIDs.Select(id => id.ToString()).Aggregate((current, next) => current + "," + next));
                    remainingJobItemInvs = notDesignatedJobItemInvs.Where(ji=>!firstItemJobItemIDs.Contains( ji.JobItemID)).ToList();
                }
                else
                {
                    //retList.AddRange();
                    retList.Add(notDesignatedJobItemInvs.Select(ji => "["+ji.JobItemID.ToString() + "]").Aggregate((current, next) => current+ "," + next));
                }

                if(grpJobItemInvs.Count()>0)
                    remainingJobItemInvs.AddRange(grpJobItemInvs);
               
            }

            if(remainingJobItemInvs.Count>0)
                retList.AddRange(SortInvJobItemIDs(remainingJobItemInvs, itemStructure));
            //foreach(var grp in grpRelatedInv)
            //{
            //    grp.
            //}
            return retList;
        }

        private IEnumerable<JobItemInv> GetFulfillJobItemIDs(IEnumerable<JobItemInv> itemInvs, int fulfillQty)
        {
            var retList = new List<JobItemInv>();
            int accumulator = 0;
            foreach (var item in itemInvs)
            {
                accumulator +=item.SumQty;
                retList.Add(item);
                if (accumulator>= fulfillQty)
                    break;
            }

            if (accumulator < fulfillQty)
                retList.Clear();

            return retList;
        }

        //private IEnumerable<> 

        private AddItemItemImages GenerateAddImages(UpdateItemItemImages updateItemItemImages)
        {
            if (updateItemItemImages == null)
                return default(AddItemItemImages);

            var retObj = new AddItemItemImages();
            var lstImgs = new List<AddItemItemImage>();
            foreach(var upImg in updateItemItemImages.Image)
            {
                lstImgs.Add(new AddItemItemImage() { Name = upImg.Name, URL = upImg.URL, Delete = upImg.Delete });
            }
            retObj.Image = lstImgs.ToArray();
            return retObj;
        }

        private UpdateItemItemImages GenerateUpdateImages(ExportProductListing localListing, IEnumerable<D_JobItem> firstInvJobItems, D_Item item, GetItemResponseItem onlineListing=null)
        {
            try
            {
                

                var retImageObj = new UpdateItemItemImages();
                int i = 0;
                var lstItemImages = new List<UpdateItemItemImage>();

                if ((firstInvJobItems == null || firstInvJobItems.Count() == 0)
                    && localListing.Condition.Equals(ThirdStoreJobItemCondition.USED.ToName())
                    //&& localListing.Qty == 0 //commented because qty may be 1 and there is allocated job item
                    && onlineListing != null)
                {
                    return default(UpdateItemItemImages);
                }

                if (item.ItemImages.Count > 0)
                {
                    foreach (var itmImg in item.ItemImages.OrderBy(img=>img.DisplayOrder))
                    {
                        if (i <6)
                        {
                            lstItemImages.Add(new UpdateItemItemImage() { Name = (i == 0 ? "Main" : "Alt " + i), URL = _imageService.GetImageURL(itmImg.ImageID), Delete = false });
                            i++;
                        }
                    }
                }

                if (firstInvJobItems != null && firstInvJobItems.Count() > 0)
                {
                    foreach (var jobItem in firstInvJobItems)
                    {
                        foreach (var jobItmImg in jobItem.JobItemImages.Where(img=>!img.StatusID.Equals(0)).OrderBy(img => img.DisplayOrder))
                        {
                            if (i <12)
                            {
                                lstItemImages.Add(new UpdateItemItemImage() { Name = (i == 0 ? "Main" : "Alt " + i), URL = _imageService.GetImageURL(jobItmImg.ImageID), Delete = false });
                                i++;
                            }
                        }
                    }
                }

                if (onlineListing != null && onlineListing.Images.Count() > i)
                {
                    while (i < onlineListing.Images.Count())
                    {
                        lstItemImages.Add(new UpdateItemItemImage() { Name = (i == 0 ? "Main" : "Alt " + i), Delete = true });
                        i++;
                    }
                }

                retImageObj.Image = lstItemImages.ToArray();

                return retImageObj;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private string GenerateProductDesc(ExportProductListing localListing, IEnumerable<D_JobItem> firstInvJobItems, D_Item item, GetItemResponseItem onlineListing = null)
        {
            try
            {
                var strDesc = new StringBuilder();
                //var firstInvJobItemNode = "";

                if ((firstInvJobItems == null || firstInvJobItems.Count() == 0)
                    && localListing.Condition.Equals(ThirdStoreJobItemCondition.USED.ToName())
                    //&&localListing.Qty==0 //commented because qty may be 1 and there is allocated job item
                    && onlineListing != null)
                {
                    //strDesc.Append(onlineListing.Description);
                    return string.Empty;
                }

                if (firstInvJobItems != null && firstInvJobItems.Count() > 0)
                {
                    var firstInvJobItemsReference = string.Format("<span id='{0}'>{1}</span>",Constants.FirstInvJobItemsRef, firstInvJobItems.Select(ji => GetJobItemReference(ji)).Aggregate((current, next) => current + "," + next));
                    var firstInvJobItemsID = firstInvJobItems.Select(ji => ji.ID.ToString()).Aggregate((current, next) => current + "," + next);
                    var firstInvJobItemNode = string.Format("<input type='hidden' id='{0}' value='{1}' />", Constants.FirstInvJobItems, firstInvJobItemsID);
                    strDesc.Append(firstInvJobItemsReference);
                    strDesc.Append(firstInvJobItemNode);

                    strDesc.Append("<br />");
                    foreach (var jobItem in firstInvJobItems)
                    {
                        if (!string.IsNullOrEmpty(jobItem.Note))
                        {
                            strDesc.Append($"<h3><span style='text-decoration:underline;color:#0000ff;'><strong> {nameof(jobItem.Note)}: {jobItem.Note} </strong></span></h3>");
                            strDesc.Append("<br />");
                        }
                    }
                }

                if (!string.IsNullOrEmpty(localListing.JobItemInvIDs))
                {
                    var jobItemInvListNode = string.Format("<input type='hidden' id='{0}' value='{1}' />", Constants.JobItemInvList, localListing.JobItemInvList);
                    var jobItemInvIDsNode = string.Format("<input type='hidden' id='{0}' value='{1}' />", Constants.JobItemInvIDs, localListing.JobItemInvIDs);
                    strDesc.Append($"{Constants.JobItemInvList}: {localListing.JobItemInvList}" );
                    strDesc.Append(jobItemInvListNode);
                    strDesc.Append(jobItemInvIDsNode);
                    strDesc.Append("<br />");
                }

                if (firstInvJobItems != null && firstInvJobItems.Count() > 0)
                {
                    int i;
                    if (item.ItemImages.Count >= 6)
                        i = 6;
                    else
                        i = item.ItemImages.Count;
                    var rnd = new Random();
                    var imgSKU = (onlineListing != null ? onlineListing.SKU : localListing.SKUWSuffix);
                    foreach(var jobItem in firstInvJobItems)
                    {
                        foreach (var jobItmImg in jobItem.JobItemImages.Where(img => !img.StatusID.Equals(0)).OrderBy(img => img.DisplayOrder))
                        {
                            if (i <12)
                            {
                                strDesc.Append($"<img src='https://www.3rdstore.com.au/assets/alt_{i}/{imgSKU}.jpg?{rnd.Next(1,int.MaxValue)}' style='margin-bottom:10px;' />");
                                i++;
                            }
                        }
                    }
                    strDesc.Append("<br />");
                }

                strDesc.Append(item.Description);
                strDesc.Append("<br />");

                //check if there is shipping term
                if (onlineListing != null)
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(onlineListing.Description);
                    var shippingNode = htmlDoc.DocumentNode.SelectSingleNode("//u[.='Shipping']");
                    if (shippingNode != null)
                    {
                        var parentNode = (((shippingNode.ParentNode ?? null) != null ? shippingNode.ParentNode.ParentNode : null) != null ? shippingNode.ParentNode.ParentNode.ParentNode : null);
                        if (parentNode != null)
                        {
                            var fullDesc = htmlDoc.DocumentNode.OuterHtml;
                            var shippingNodeIndex = fullDesc.IndexOf(parentNode.OuterHtml);
                            var reserveDesc = fullDesc.Substring(shippingNodeIndex, fullDesc.Length - shippingNodeIndex);
                            strDesc.Append(reserveDesc);
                        }
                    }
                }

                return strDesc.ToString();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private bool IsItemAEqualToOrContainItemB(int itemAID,int itemBID, IEnumerable<V_ItemRelationship> allRelation)
        {
            var itemARelation = allRelation.Where(r => r.ItemID.Equals(itemAID));
            //var itemABRelation = allRelation.Where(r => r.ItemID.Equals(itemAID)&&r.BottomItemID.Equals(itemBID));
            //var itemBRelation=allRelation.Where(r => r.ItemID.Equals(itemAID));
            if (itemAID.Equals(itemBID)||( itemARelation.Count() == 1 && itemARelation.FirstOrDefault().Qty == 1&& itemARelation.FirstOrDefault().BottomItemID.Equals(itemBID)))
                return true;
            else
                return false;
        }

        public D_JobItemLine GetJobItemLineByID(int jobItemLineID)
        {
            return _jobItemLineRepository.GetById(jobItemLineID);
        }

        public void DeleteJobItemLine(D_JobItemLine jobItemLine)
        {
            if (jobItemLine == null)
                throw new ArgumentNullException("jobItemLine null");

            _jobItemLineRepository.Delete(jobItemLine);
        }

        public ThirdStoreReturnMessage ShipOut(string jobItemLineID, string jobItemLineRef, string trackingNumber)
        {
            var returnMessage = new ThirdStoreReturnMessage();
            try
            {
                if (string.IsNullOrEmpty(jobItemLineID)&& string.IsNullOrEmpty(jobItemLineRef))
                {
                    throw new Exception("No job item line id or job item line reference was provided.");
                }

                D_JobItem jobItem = null;
                var inStatus = new int[] { ThirdStoreJobItemStatus.READY.ToValue(), ThirdStoreJobItemStatus.ALLOCATED.ToValue(), ThirdStoreJobItemStatus.BOOKED.ToValue() };
                if (!string.IsNullOrEmpty(jobItemLineID))
                {
                    var intJobItemLineID = Convert.ToInt32(jobItemLineID);
                    jobItem = _jobItemRepository.Table.FirstOrDefault(ji => ji.JobItemLines.Any(l => l.ID.Equals(intJobItemLineID))&&inStatus.Contains( ji.StatusID));
                }
                else if(!string.IsNullOrEmpty(jobItemLineRef))
                {
                    var jobItemsByRef = GetJobItemByReference(jobItemLineRef);
                    if(jobItemsByRef!=null&& jobItemsByRef.Count>0)
                    {
                        
                        jobItemsByRef = jobItemsByRef.Where(ji =>inStatus.Contains(ji.StatusID)).ToList();
                        if(jobItemsByRef.Count==1)
                        {
                            jobItem = jobItemsByRef.FirstOrDefault();
                        }
                    }
                }

                if(jobItem!=null)
                {
                    if(!string.IsNullOrWhiteSpace(trackingNumber))
                        jobItem.TrackingNumber =trackingNumber.Trim();
                    jobItem.StatusID = ThirdStoreJobItemStatus.SHIPPED.ToValue();
                    jobItem.ShipTime = DateTime.Now;
                    this.UpdateJobItem(jobItem);
                    returnMessage.IsSuccess = true;
                    returnMessage.Mesage += $"Job item { GetJobItemReference(jobItem)} has been shipped out.";
                }
                else
                {
                    returnMessage.IsSuccess = false;
                    returnMessage.Mesage += $"Cannot locate job item.";
                }

                return returnMessage;

                //var jobItemLine = _jobItemLineRepository.Table.FirstOrDefault(l => l.ID.Equals(intJobItemLineID));
                //if (jobItemLine != null)
                //{
                //    var jobItem = jobItemLine.JobItem;
                    
                    
                //}
                //else
                //{
                //    return default(D_JobItem);
                //}
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                returnMessage.IsSuccess = false;
                returnMessage.Mesage = ex.Message;
                return returnMessage;
            }
        }

        public IList<D_JobItem> GetJobItemsByIDs(IList<int> ids)
        {
            var jobItems = _jobItemRepository.Table.Where(ji => ids.Contains(ji.ID));
            return jobItems.ToList();
        }

        protected IQueryable<D_JobItem> GetJobItemByReference(string reference,IQueryable<D_JobItem> query=null)
        {
            if (string.IsNullOrEmpty(reference) || reference.Length < 5)
                return query;
            var jobItemRef = "";
            if(reference.IndexOf("/")!=-1)
            {
                jobItemRef = reference.Substring(0, reference.IndexOf("/"));
            }
            else
            {
                jobItemRef = reference;
            }

            var datePart = jobItemRef.Substring(0, 4);
            var numPart = jobItemRef.Substring(datePart.Length, jobItemRef.Length - datePart.Length);

            var jobItemQuery =(query?? _jobItemRepository.Table) ;
            //jobItemQuery = jobItemQuery.Where(ji => ji.CreateTime.ToString("ddMM").Equals(datePart) && ji.Ref1.Equals(numPart));
            jobItemQuery = jobItemQuery.Where(ji =>(DbFunctions.Right("000" + SqlFunctions.DatePart("dd", ji.CreateTime).Value, 2)+ DbFunctions.Right("000"+SqlFunctions.DatePart("mm", ji.CreateTime).Value, 2)).Equals(datePart) && ji.Ref1.Equals(numPart));
            //if (jobItemQuery.Count() == 1)
            //    return jobItemQuery.FirstOrDefault();
            //else
            //    return default(D_JobItem);
            return jobItemQuery;
        }

        protected IList<D_JobItem> GetJobItemByReference(string reference)
        {
            var query = this.GetJobItemByReference(reference, null);
            return query != null ? query.ToList() : null;
        }

        public string GetJobItemReference(DateTime jobItemCreateTime, string sequenceNum)
        {
            if(jobItemCreateTime.Equals(DateTime.MinValue)||jobItemCreateTime.Equals(DateTime.MaxValue)||string.IsNullOrEmpty(sequenceNum))
            {
                return string.Empty;
            }
            else
            {
                return $"{jobItemCreateTime.ToString("ddMM")}{sequenceNum}"; 
            }
            
        }

        public string GetJobItemReference(D_JobItem jobItem)
        {
            return GetJobItemReference(jobItem.CreateTime, jobItem.Ref1);
        }

        public string GetJobItemReference(int jobItemID)
        {
            var jobItem = GetJobItemByID(jobItemID);
            return GetJobItemReference(jobItem);
        }

        public string GetJobItemLineReference(D_JobItemLine jobItemLine)
        {
            var jobItem = jobItemLine.JobItem;
            var reference = GetJobItemReference(jobItem);
            var jobItemLines = jobItem.JobItemLines;
            if (jobItemLines.Count == 1)
                return reference;
            var jobItemLineIndex = jobItem.JobItemLines.ToList().FindIndex(l=>l==jobItemLine);
            if (jobItemLineIndex != -1 && !string.IsNullOrEmpty(reference))
            {
                return $"{reference}/{jobItemLineIndex+1}";
            }
            else
                return string.Empty;
        }

        protected string GetJobItemSequenceNumber()
        {
            var pID = new SqlParameter();
            pID.ParameterName = "id";
            pID.Direction = ParameterDirection.Input;
            pID.DbType = DbType.Int32;
            pID.Value = 1;

            var sqlString = @"EXECUTE [dbo].[GetSequenceID] @id";
            var sequenceNo = _dbContext.SqlQuery<string>(sqlString, pID).FirstOrDefault();
            if (string.IsNullOrEmpty(sequenceNo))
                throw new Exception("Get Sequence Number Failed");
            return sequenceNo;
        }

        public void PrintJobItemLabel(IList<int> ids)
        {
            var jobItems =GetJobItemsByIDs(ids);
            var printData = jobItems.SelectMany(ji => ji.JobItemLines).Select(l => new { JobItemLineRef = GetJobItemLineReference(l), JobItemLineSKU = $"{l.SKU},{l.ID}" }).ToList();
            var printingParam = new ThirdStoreBarcodeReportPrintingParameter();
            printingParam.Datasources.Add("DataSet1", printData);
            _reportPrintService.PrintLocalReport(printingParam);
        }

        public ThirdStoreReturnMessage SyncInventory(DateTime? fromTime, DateTime? toTime)
        {
            try
            {

                var strFromDate = fromTime.Value.ToString("yyyyMMdd");
                var strToDate= toTime.Value.ToString("yyyyMMdd");
                var sqlStr = new StringBuilder();
                sqlStr.Append("select * from fn_GetAffectedItems(@FromDate,@ToDate)");

                var paraFromDate = new SqlParameter();
                paraFromDate.ParameterName = "FromDate";
                paraFromDate.DbType = DbType.String;
                paraFromDate.Value = strFromDate;

                var paraToDate = new SqlParameter();
                paraToDate.ParameterName = "ToDate";
                paraToDate.DbType = DbType.String;
                paraToDate.Value = strToDate;

                var query = _dbContext.SqlQuery<int>(sqlStr.ToString(), paraFromDate, paraToDate).ToList();

                return this.SyncInventory(query);

            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return new ThirdStoreReturnMessage() { IsSuccess = false, Mesage = ex.Message };
            }
        }

        public ThirdStoreReturnMessage SyncInventory(int[] jobItemIDs)
        {
            try
            {
                if(jobItemIDs==null||jobItemIDs.Count()==0)
                {
                    return new ThirdStoreReturnMessage() { IsSuccess = true };
                }

                var strJobItemIDs = jobItemIDs.Select(id=>id.ToString()).Aggregate((current, next) => current + "," + next);
                var sqlStr = new StringBuilder();
                sqlStr.Append("select * from fn_GetAffectedItemsByJobItems(@JobItemIDs)");

                var paraJobItemIDs = new SqlParameter();
                paraJobItemIDs.ParameterName = "JobItemIDs";
                paraJobItemIDs.DbType = DbType.String;
                paraJobItemIDs.Value = strJobItemIDs;

                var query = _dbContext.SqlQuery<int>(sqlStr.ToString(), paraJobItemIDs).ToList();

                return this.SyncInventory(query);

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return new ThirdStoreReturnMessage() { IsSuccess = false, Mesage = ex.Message };
            }
        }

        public ThirdStoreReturnMessage ConfirmStock(IList<string> jobItemLineIDs, IList<string> jobItemLineRefs,string location)
        {
            var returnMessage = new ThirdStoreReturnMessage();
            try
            {
                if (jobItemLineIDs==null && jobItemLineRefs==null)
                {
                    throw new Exception("No job item line id or job item line reference was provided.");
                }

                var jobItems = new List<D_JobItem>() ;
                if (jobItemLineIDs != null)
                {
                    var intJobItemLineIDs = jobItemLineIDs.Select(id => Convert.ToInt32(id));
                    var notInStatus = new int[] { ThirdStoreJobItemStatus.SHIPPED.ToValue() };
                    jobItems = (from ji in _jobItemRepository.Table
                                   from line in ji.JobItemLines
                                   where intJobItemLineIDs.Contains(line.ID)
                                   && !notInStatus.Contains(ji.StatusID)
                                    select ji).ToList();

                    var notLocateLineIDs = from jilid in jobItemLineIDs.Select(lid=>Convert.ToInt32(lid))
                                           where !jobItems.SelectMany(ji => ji.JobItemLines).Select(l => l.ID).Any(lid => lid.Equals(jilid))
                                           select jilid;

                    if(notLocateLineIDs.Count()>0)
                    {
                        foreach(var lid in notLocateLineIDs)
                        {
                            returnMessage.Mesage += $"Cannot locate job item line id {lid}, ";
                        }
                    }
                       
                }
                else if(jobItemLineRefs!=null)
                {
                    foreach(var jir in jobItemLineRefs)
                    {
                        var jobItemsByRef = GetJobItemByReference(jir);
                        if (jobItemsByRef != null && jobItemsByRef.Count > 0)
                        {
                            jobItemsByRef = jobItemsByRef.Where(ji => ji.StatusID != ThirdStoreJobItemStatus.SHIPPED.ToValue()).ToList();
                            if (jobItemsByRef.Count == 1)
                            {
                                jobItems.Add(jobItemsByRef.FirstOrDefault());
                            }
                            else
                            {
                                returnMessage.Mesage += $"Cannot locate job item {jir}, ";
                            }
                        }
                    }
                }

                if(jobItems.Count>0)
                {
                    foreach(var ji in jobItems)
                    {
                        ji.StocktakeTime = DateTime.Now;
                        ji.Location = (!string.IsNullOrWhiteSpace(location) ? location.Trim() : ji.Location);
                        this.UpdateJobItem(ji);
                        returnMessage.Mesage += $"Job item { GetJobItemReference(ji)} confirmed, ";
                    }
                    returnMessage.IsSuccess = true;
                }
                else
                {
                    returnMessage.IsSuccess = false;
                }

                return returnMessage;
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                returnMessage.IsSuccess = false;
                returnMessage.Mesage = ex.Message;
                return returnMessage;
            }
        }

        

        protected class JobItemInv
        {
            public int JobItemID { get; set; }
            public DateTime CreateTime { get; set; }
            public int StatusID { get; set; }
            public string DesignatedSKU { get; set; }
            public int ConditionID { get; set; }
            public string JobItemReference { get; set; }
            public int ItemID { get; set; }
            public int RelationQty { get; set; }
            public int BottomItemID { get; set; }
            public int SumQty { get; set; }
        }

        protected class ExportProductListing
        {
            public int ItemID { get; set; }
            public string SKU { get; set; }
            public string Condition { get; set; }
            public int Qty { get; set; }
            public string FirstInvJobItemIDs { get; set; }
            public string JobItemInvList { get; set; }
            public string JobItemInvIDs { get; set; }
            public string SKUWSuffix { get; set; }
        }
    }

    

    
}
