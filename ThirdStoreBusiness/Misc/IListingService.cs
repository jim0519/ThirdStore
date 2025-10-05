
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

        public ListingService(IRepository<D_Listing> listingRepository,
            CsvContext csvContext,
            IItemService itemService)
        {
            _listingRepository = listingRepository;
            _csvContext = csvContext;
            _itemService = itemService;
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


            query = query.Where(l => l.ListingStatusID == 3);
            query = query.OrderByDescending(l => l.ListingSKU);

            return new PagedList<D_Listing>(query, pageIndex, pageSize);
        }

        public bool SyncLocalListings()
        {
            try
            {
               

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
                var temuListings = GetActiveProducts();
                var localListing = GetListings(false);
                var localItem = _itemService.GetAllItems();
                var createTime = DateTime.Now;
                var createBy = Constants.SystemUser;

                if (temuListings == null || temuListings.Count() == 0)
                    throw new Exception("No active listings on Temu.");

                #region UPDATE:whether temu listing match local listing(sku, itemID), update local listing with temu item

                var updateInfoList = from tl in temuListings
                                     join ll in localListing on new { sku = tl.SKU.ToUpper(), listingID = tl.SKUID.ToString() } equals new { sku = ll.ListingSKU.ToUpper(), listingID = ll.ListingID }
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
                        localListingItem.ListingStatusID = 3;

                        localListingItem.LastUpdateTime = createTime;
                        _listingRepository.Update(localListingItem, l => l.ItemID, l => l.ListingInventoryQty, l => l.ListingPrice, l => l.LastUpdateTime, l => l.ListingStatusID, l => l.ListingDescription);
                    }
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

        public bool SyncOnlineInventory(int[] listingIDs)
        {
            try
            {
                
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
                list = list.Where(l => l.ListingStatusID == 3);

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

        #endregion

    }

}