
using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.JobItem;
using ThirdStoreBusiness.Report;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Listing;
using ThirdStoreData;

namespace ThirdStoreBusiness.Listing
{
    public interface IListingService
    {
        D_Listing GetListingByID(int id);

        IList<D_Listing> GetListingsByIDs(IEnumerable<int> ids);

        IPagedList<D_Listing> SearchListings(
            string sku = null,
            string title = null,
            int isAuto = -1,
            string itemID = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        bool SyncLocalListings();

        bool SyncOnlineInventory();

        bool SyncOnlineInventory(int[] listingIDs);

        void InsertListing(D_Listing listing);

        void UpdateListing(D_Listing listing);

        void DeleteListing(D_Listing listing);

    }



    public class ListingService : IListingService
    {
        private readonly IRepository<D_Listing> _listingRepository;
        private readonly CsvContext _csvContext;
        private readonly IItemService _itemService;
        private readonly IJobItemService _jobItemService;

        public ListingService(IRepository<D_Listing> listingRepository,
            CsvContext csvContext,
            IItemService itemService,
            IJobItemService jobItemService)
        {
            _listingRepository = listingRepository;
            _csvContext = csvContext;
            _itemService = itemService;
            _jobItemService = jobItemService;
        }





        public IPagedList<D_Listing> SearchListings(
            string sku = null,
            string title = null,
            int isAuto = -1,
            string itemID = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _listingRepository.Table;

            //if (sku != null)
            //    query = query.Where(l => l.ListingSKU.ToLower().Contains(sku.ToLower()));
            if (sku != null)
            {
                var splitSKUs = sku.Split(' ');
                if (splitSKUs.Count() > 1)
                    query = query.Where(l => splitSKUs.Contains(l.ListingSKU));
                else
                    query = query.Where(l => l.ListingSKU.Contains(sku));
            }

            if (title != null)
                query = query.Where(l => l.ListingTitle.ToLower().Contains(title.ToLower()));

            if (itemID != null)
                query = query.Where(l => l.ListingID.Contains(itemID));

            if (isAuto != -1)
            {
                var blIsAuto = Convert.ToBoolean(isAuto);
                query = query.Where(l => l.IsAuto.Equals(blIsAuto));
            }


            query = query.Where(l => l.ListingStatusID == 1);
            query = query.OrderByDescending(l => l.ListingSKU);

            return new PagedList<D_Listing>(query, pageIndex, pageSize);
        }

        public bool SyncLocalListings()
        {
            try
            {
                var temuListings = GetActiveProducts();
                var localListing = GetListings(false);
                var localItem = _itemService.GetAllItems();
                var createTime = DateTime.Now;
                var createBy = Constants.SystemUser;

                if (temuListings == null || temuListings.Count() == 0)
                    throw new Exception("No active listings on Temu.");

                #region UPDATE:whether temu listing match local listing(sku, itemID), update local listing with temu item

                var updateInfoList = from tl in temuListings
                                     join ll in localListing on new { sku = tl.SKU?.ToUpper(), listingID = tl.SKUID.ToString() } equals new { sku = ll.ListingSKU.ToUpper(), listingID = ll.ListingID }
                                     join it in localItem on ll.ListingSKU.ToUpper() equals it.SKU.ToUpper() into leftjoinLocalItem
                                     from ljli in leftjoinLocalItem.DefaultIfEmpty()
                                     select new
                                     {
                                         ID = ll.ID,
                                         ItemID = (ljli != null ? ljli.ID : 0),
                                         SKU = ll.ListingSKU,
                                         Title = tl.ProductName,
                                         Description = tl.ProductName,
                                         Price = tl.BasePrice,
                                         Qty = tl.Quantity,
                                         //GoodsID=tl.GoodsID,
                                         //Category=tl.Category,
                                         //CategoryID=tl.CategoryID,
                                         //ContributionGoods=tl.ContributionGoods,
                                         //Variation=tl.Variation,
                                         //ExternalProductID=tl.ExternalProductID,
                                         //DateCreated=tl.DateCreated,
                                         Listing = ll
                                     };
                foreach (var updateMatch in updateInfoList)
                {
                    var localListingItem = updateMatch.Listing;
                    if (localListingItem != null)
                    {
                        localListingItem.ItemID = updateMatch.ItemID;
                        localListingItem.ListingTitle = updateMatch.Title;
                        localListingItem.ListingInventoryQty = updateMatch.Qty;
                        localListingItem.ListingPrice = updateMatch.Price;
                        localListingItem.ListingDescription = updateMatch.Description;
                        localListingItem.ListingStatusID = 1;

                        localListingItem.LastUpdateTime = createTime;
                        _listingRepository.Update(localListingItem, l => l.ItemID, l => l.ListingInventoryQty, l => l.ListingPrice, l => l.LastUpdateTime, l => l.ListingStatusID, l => l.ListingDescription);
                    }
                }

                #endregion

                #region DELETE: if local listing not in active listing list, then disable the local listing

                var disableLocalListing = from ll in localListing
                                          where ll.ListingStatusID == 1
                                          && !temuListings.Any(tl => tl.SKUID == ll.ListingID && tl.SKU?.ToUpper() == ll.ListingSKU.ToUpper())
                                          select ll;

                foreach (var dl in disableLocalListing)
                {
                    dl.ListingStatusID = 2;
                    dl.LastUpdateTime = createTime;
                    _listingRepository.Update(dl, l => l.ListingStatusID, l => l.LastUpdateTime);
                }

                #endregion

                #region ADD: if cannot find local listing by sku but can find in item, then add local listing and link with item;

                var addListingList = from tl in temuListings
                                     join li in localItem on tl.SKU?.ToUpper() equals li.SKU.ToUpper() into g
                                     from alli in g.DefaultIfEmpty()
                                         //where !localListing.Select(ll => ll.ListingSKU.ToUpper()).Contains(al.SKU.ToUpper())
                                     where !localListing.Any(ll => ll.ListingID == tl.SKUID && ll.ListingSKU.ToUpper() == tl.SKU?.ToUpper())
                                     select new
                                     {
                                         ItemID = (alli == null ? 0 : alli.ID),
                                         ListingID = tl.SKUID.ToString(),
                                         ListingSKU = tl.SKU,//al.SKU.ToUpper(),
                                         ListingTitle = tl.ProductName,
                                         ListingDescription = tl.ProductName,
                                         ListingPrice = tl.BasePrice,
                                         ListingInventory = tl.Quantity,
                                         GoodsID = tl.GoodsID,
                                         Category = tl.Category,
                                         CategoryID = tl.CategoryID,
                                         ContributionGoods = tl.ContributionGoods,
                                         Variation = tl.Variation,
                                         ExternalProductID = tl.ExternalProductID,
                                         DateCreated = tl.DateCreated,
                                     };

                foreach (var addListing in addListingList)
                {
                    var newLocalListing = new D_Listing();

                    newLocalListing.ItemID = addListing.ItemID;
                    newLocalListing.ListingID = addListing.ListingID;
                    newLocalListing.ListingSKU = addListing.ListingSKU;
                    newLocalListing.ListingTitle = addListing.ListingTitle;
                    newLocalListing.ListingDescription = addListing.ListingDescription;
                    newLocalListing.ListingPrice = addListing.ListingPrice;
                    newLocalListing.ListingInventoryQty = addListing.ListingInventory;
                    newLocalListing.ListingStatusID = 1;//TODO: get active lising status ID in status list
                    newLocalListing.IsAuto = true;
                    newLocalListing.Ref1 = addListing.GoodsID;
                    newLocalListing.Ref2 = addListing.ExternalProductID;
                    newLocalListing.Ref3 = addListing.Variation;
                    newLocalListing.Ref4 = addListing.ContributionGoods;
                    newLocalListing.Ref5 = addListing.DateCreated.ToString();
                    //newLocalListing.ListingPriceRuleID = 1;//TODO: Default price rule ID
                    //newLocalListing.ListingInventoryQtyRuleID = 2;//TODO: Default inventory qty rule ID

                    //UpdateListingPostageRule(addListing.ShippingDetails, newLocalListing);
                    newLocalListing.LastUpdateTime = createTime;
                    newLocalListing.CreateTime = createTime;
                    newLocalListing.CreateBy = createBy;
                    newLocalListing.EditTime = createTime;
                    newLocalListing.EditBy = createBy;
                    newLocalListing.FillOutNull();

                    _listingRepository.Insert(newLocalListing);
                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return false;
            }
        }

        public bool SyncOnlineInventory()
        {
            try
            {
                var lstListing = GetListings();
                lstListing = lstListing.Where(l => l.IsAuto).ToList();

                return SyncOnlineInventory(lstListing);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return false;
            }
        }

        public bool SyncOnlineInventory(int[] listingIDs)
        {
            try
            {
                var lstListing = GetListingsByIDs(listingIDs);

                return SyncOnlineInventory(lstListing);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return false;
            }
        }

        public bool SyncOnlineInventory(IList<D_Listing> listings)
        {
            try
            {
                if (listings == null || listings.Count == 0)
                    return true;

                //get local inventory
                var localItem = _itemService.GetAllItems();
                var listingItems = listings.Where(l => l.ItemID != 0).Select(l => l.Item).Distinct().ToList();
                var itemInventoryWCon = _jobItemService.CalculateProductInventory(listingItems);
                var itemInventory = itemInventoryWCon.GroupBy(ic => new { ic.SKU, ic.ItemID }).Select(grp => new {ItemID=grp.Key.ItemID, SKU = grp.Key.SKU, Qty = grp.Sum(ic => ic.Qty) });

                foreach(var l in listings)
                {
                    if(l.ItemID==0)
                        continue;

                    var itemInv = itemInventory.FirstOrDefault(inv=>inv.ItemID.Equals(l.ItemID));
                    if(itemInv!=null)
                    {
                        l.ListingInventoryQty= (itemInv.Qty-1>=0? itemInv.Qty - 1:0);
                        UpdateListing(l);
                    }
                }

                var temuStockPriceLines = new List<TemuStockPriceLine>();
                foreach (var listing in listings)
                {
                    var temuStockPriceLine = new TemuStockPriceLine();
                    temuStockPriceLine.SKU = listing.ListingSKU;
                    temuStockPriceLine.StockQty = listing.ListingInventoryQty;
                    temuStockPriceLine.BasePrice = listing.ListingPrice;
                    temuStockPriceLine.SKUID = listing.ListingID;

                    temuStockPriceLines.Add(temuStockPriceLine);
                }

                var di = new DirectoryInfo(ThirdStoreConfig.Instance.TemuStockPriceFilePath);
                if (!di.Exists)
                    di.Create();
                
                var stockFileName = "Stock.xlsx";
                var priceFileName = "Price.xlsx";
                var updateStockLines = new List<TemuStockLine>();
                var updatePriceLines = new List<TemuPriceLine>();

                foreach (var stockPriceLine in temuStockPriceLines)
                {
                    var newStockLine = new TemuStockLine();
                    //newStockLine.SKU = stockPriceLine.SKU;
                    newStockLine.Qty = stockPriceLine.StockQty;
                    newStockLine.SKUID = stockPriceLine.SKUID;
                    updateStockLines.Add(newStockLine);

                    var newPriceLine = new TemuPriceLine();
                    //newPriceLine.SKU=stockPriceLine.SKU;
                    newPriceLine.NewPrice = stockPriceLine.BasePrice;
                    newPriceLine.SKUID = stockPriceLine.SKUID;
                    updatePriceLines.Add(newPriceLine);
                }

                var byteStockFile = CommonFunc.GenerateExcel(updateStockLines, 1);
                var bytePriceFile = CommonFunc.GenerateExcel(updatePriceLines);

                File.WriteAllBytes(ThirdStoreConfig.Instance.TemuStockPriceFilePath + "\\" + stockFileName, byteStockFile);
                File.WriteAllBytes(ThirdStoreConfig.Instance.TemuStockPriceFilePath + "\\" + priceFileName, bytePriceFile);
                

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return false;
            }
        }

        public void InsertListing(D_Listing listing)
        {
            if (listing == null)
                throw new ArgumentNullException("listing null");

            _listingRepository.Insert(listing);
        }

        public void UpdateListing(D_Listing listing)
        {
            if (listing == null)
                throw new ArgumentNullException("listing null");

            _listingRepository.Update(listing);
        }

        public void DeleteListing(D_Listing listing)
        {
            throw new NotImplementedException();
        }

        public D_Listing GetListingByID(int id)
        {
            var listing = _listingRepository.GetById(id);
            return listing;
        }

        public IList<D_Listing> GetListingsByIDs(IEnumerable<int> ids)
        {
            var listings = _listingRepository.Table.Where(l => ids.Contains(l.ID)).ToList();
            return listings;
        }



        private IList<TemuProduct> GetActiveProducts()
        {
            try
            {
                var retProducts = new List<TemuProduct>();
                var di = new DirectoryInfo(ThirdStoreConfig.Instance.TemuActiveProductFilePath);
                if (!di.Exists)
                    di.Create();

                var latesOrderFile = di.GetFiles().OrderByDescending(f => f.CreationTime).FirstOrDefault();
                if (latesOrderFile.Exists)
                {
                    using (var stream = File.OpenRead(latesOrderFile.FullName))
                    {
                        var temuProducts = CommonFunc.ReadExcelToList<TemuProduct>(stream);
                        retProducts.AddRange(temuProducts);
                    }
                }

                return retProducts;

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return default(IList<TemuProduct>);
            }
        }

        private IList<D_Listing> GetListings(bool isOnlyActive = true)
        {
            var list = _listingRepository.Table;

            if (isOnlyActive)
                list = list.Where(l => l.ListingStatusID == 1);

            return list.ToList();
        }


        #region Temu Objects

        public class TemuProduct
        {
            [Display(Name = "Category")]
            public string Category { get; set; }
            [Display(Name = "Category id")]
            public int CategoryID { get; set; }
            [Display(Name = "Product name")]
            public string ProductName { get; set; }
            [Display(Name = "Contribution Goods")]
            public string ContributionGoods { get; set; }
            [Display(Name = "SKU")]
            public string SKU { get; set; }
            [Display(Name = "Goods ID")]
            public string GoodsID { get; set; }
            [Display(Name = "SKU ID")]
            public string SKUID { get; set; }
            [Display(Name = "Variation")]
            public string Variation { get; set; }
            [Display(Name = "Quantity")]
            public int Quantity { get; set; }
            [Display(Name = "Base price")]
            public decimal BasePrice { get; set; }
            [Display(Name = "External product ID")]
            public string ExternalProductID { get; set; }
            [Display(Name = "Status")]
            public string Status { get; set; }
            [Display(Name = "Detail status")]
            public string DetailStatus { get; set; }
            [Display(Name = "Date created")]
            public DateTime DateCreated { get; set; }
        }

        public class TemuStockPriceLine
        {
            public string SKU { get; set; }
            public string SKUID { get; set; }
            public int StockQty { get; set; }
            public decimal BasePrice { get; set; }
        }

        public class TemuStockLine
        {
            [Display(Name = "SKU")]
            public string SKU { get; set; }
            [Display(Name = "*New quantity")]
            public int Qty { get; set; }
            [Display(Name = "SKU ID")]
            public string SKUID { get; set; }
        }

        public class TemuPriceLine
        {
            [Display(Name = "Seller SKU")]
            public string SKU { get; set; }
            [Display(Name = "Current base price")]
            public decimal CurrentPrice { get; set; }
            [Display(Name = "New base price")]
            public decimal NewPrice { get; set; }
            [Display(Name = "SKU ID")]
            public string SKUID { get; set; }
        }

        #endregion

    }

}