using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon.Models.Item;
using ThirdStoreData;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;
using System.Text.RegularExpressions;
using System.IO;
using LINQtoCSV;
using ThirdStoreBusiness.Image;
using ThirdStoreCommon.Models.Image;
using ThirdStoreBusiness.API.Neto;
using ThirdStoreBusiness.DSChannel;
using ThirdStoreCommon.Models.Misc;

namespace ThirdStoreBusiness.Item
{
    public class ItemService:IItemService
    {
        private readonly IRepository<D_Item> _itemRepository;
        private readonly IRepository<D_Item_Relationship> _itemRelationshipRepository;
        private readonly IRepository<M_ItemImage> _itemImageRepository;
        private readonly IRepository<NetoProducts> _netoProductsRepository;
        private readonly IRepository<D_NewAimSKUBarcode> _newAimSKUBarcodeRepository;
        private readonly IDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly IImageService _imageService;
        private readonly INetoAPICallManager _netoAPIManager;
        private readonly IEnumerable<IDSChannel> _dsChannels;
        private readonly CsvContext _csvContext;
        private readonly CsvFileDescription _csvFileDescription;

        public ItemService(IRepository<D_Item> itemRepository,
            IRepository<D_Item_Relationship> itemRelationshipRepository,
            IRepository<M_ItemImage> itemImageRepository,
            IRepository<NetoProducts> netoProductsRepository,
            IRepository<D_NewAimSKUBarcode> newAimSKUBarcodeRepository,
            IWorkContext workContext,
            IDbContext dbContext,
            IImageService imageService,
            INetoAPICallManager netoAPIManager,
            IEnumerable<IDSChannel> dsChannels,
            CsvContext csvContext,
            CsvFileDescription csvFileDescription)
        {
            _itemRepository = itemRepository;
            _itemRelationshipRepository = itemRelationshipRepository;
            _itemImageRepository = itemImageRepository;
            _newAimSKUBarcodeRepository = newAimSKUBarcodeRepository;
            _dbContext = dbContext;
            _workContext = workContext;
            _imageService = imageService;
            _netoAPIManager = netoAPIManager;
            _netoProductsRepository = netoProductsRepository;
            _dsChannels = dsChannels.OrderBy(c => c.Order);
            _csvContext = csvContext;
            _csvFileDescription = csvFileDescription;
        }

        public IList<D_Item> GetAllItems()
        {
            var items = _itemRepository.Table.Where(i=>i.IsActive).ToList();
            return items;
        }

        public D_Item GetItemBySKU(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return default(D_Item);
            var item = _itemRepository.Table.Where(i => i.SKU.ToUpper().Equals(sku.ToUpper())&&i.IsActive).FirstOrDefault();
            return item;

        }

        public D_Item GetItemBySKUorBarcode(string skubarcode)
        {
            if (string.IsNullOrWhiteSpace(skubarcode))
                return default(D_Item);
            var newaimSKU = _newAimSKUBarcodeRepository.Table.Where(i => i.AlternateSKU2.ToUpper().Equals(skubarcode.ToUpper())).FirstOrDefault();
            if (newaimSKU != null)
                return this.GetItemBySKU(newaimSKU.SKU);
            else
                return this.GetItemBySKU(skubarcode);

        }


        public IPagedList<D_Item> SearchItems(string sku = null, 
            ThirdStoreItemType? itemType = null, 
            string name = null,
            string aliasSKU = null,
            string refSKU = null,
            ThirdStoreSupplier? supplier = null,
            int isReadyForList = -1,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _itemRepository.Table;

            if (sku != null)
            {
                sku = GetRealSKU(sku);
                query = query.Where(i => i.SKU.Contains(sku.ToLower())||i.ChildItems.Any(ir=>ir.ChildItem.SKU.Contains(sku)));
            }
            if (itemType.HasValue)
            {
                var itemTypeID = itemType.Value.ToValue();
                query = query.Where(i => i.Type.Equals(itemTypeID));
            }
            if (name != null)
                query = query.Where(i => i.Name.Contains(name.ToLower()));

            if(aliasSKU!=null)
            {
                aliasSKU = GetRealSKU(aliasSKU);
                query = query.Where(i => i.Ref2.Contains(aliasSKU));
                //if (Regex.IsMatch( aliasSKU, "(_D){1}$"))
                //{
                //    aliasSKU = Regex.Replace(aliasSKU, "(_D){1}$", "");
                //    query = query.Where(i => i.Ref2.Contains(aliasSKU));
                //}
            }
            if(refSKU!=null)
            {
                refSKU = GetRealSKU(refSKU);
                query = query.Where(i => i.Ref1.Contains(refSKU));
            }

            if (supplier.HasValue)
            {
                var supplierID = supplier.Value.ToValue();
                query = query.Where(i => i.SupplierID.Equals(supplierID));
            }
            if(isReadyForList != -1)
            {
                var blIsReadyForList = Convert.ToBoolean(isReadyForList);
                query = query.Where(l => l.IsReadyForList.Equals(blIsReadyForList));
            }

            query = query.OrderByDescending(i=>i.SKU);

            return new PagedList<D_Item>(query, pageIndex, pageSize);
        }


        public D_Item GetItemByID(int id)
        {
            var item = _itemRepository.GetById(id);
            return item;
        }


        public bool IsDuplicateSKU(string sku)
        {
            return GetItemBySKU(sku) != null ? true : false;
        }


        public void InsertItem(D_Item item)
        {
            if (item == null)
                throw new ArgumentNullException("item null");

            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
                currentUser = _workContext.CurrentUser.Email;
            if (item.CreateTime.Equals(DateTime.MinValue))
                item.CreateTime = currentTime;
            item.EditBy = currentUser;
            if (item.EditTime.Equals(DateTime.MinValue))
                item.EditTime = currentTime;

            _itemRepository.Insert(item);
        }

        public void UpdateItem(D_Item item)
        {
            if (item == null)
                throw new ArgumentNullException("item null");

            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
                currentUser = _workContext.CurrentUser.Email;
            item.EditBy = currentUser;
            item.EditTime = currentTime;

            _itemRepository.Update(item);
        }

        public void DeleteItem(D_Item item)
        {
            throw new NotImplementedException();
        }


        public D_Item_Relationship GetChildItemByID(int id)
        {
            return _itemRelationshipRepository.GetById(id);
        }


        public void DeleteChildItem(D_Item_Relationship childItem)
        {
            if (childItem == null)
                throw new ArgumentNullException("childItem null");

            _itemRelationshipRepository.Delete(childItem);
        }

        public IList<V_ItemRelationship> GetAllItemsWithRelationship()
        {
            var query = _dbContext.SqlQuery<V_ItemRelationship>("select * from V_SKURelationship_Recursive");
            return query.ToList();
        }

        public IList<D_Item> GetItemsBySKUs(IList<string> skus)
        {
            if (skus == null || skus.Count == 0)
                return default(IList<D_Item>);

            skus = skus.Select(s =>GetRealSKU( s).ToLower()).ToList();
            var items = _itemRepository.Table.Where(i => skus.Contains(i.SKU.ToLower()) && i.IsActive);
            return items.ToList();
        }

        public IList<D_Item> GetItemsByIDs(IList<int> ids)
        {
            var query = _itemRepository.Table.Where(i => ids.Contains(i.ID));
            return query.ToList();
        }


        public void AddOrUpdateItem(IList<D_Item> items)
        {
            try
            {
                var createTime = DateTime.Now;
                var createBy = _workContext.CurrentUserName;
                var localItemList = GetAllItems();

                #region Update Section

                var updateItemList = from sl in items
                                     join li in localItemList on sl.SKU.ToUpper() equals li.SKU.ToUpper()
                                     select new {updateData= sl, LocalItem= li };

                var upds = new List<D_Item>();
                foreach (var updateItem in updateItemList)
                {
                    var item = updateItem.LocalItem;
                    var updateData = updateItem.updateData;
                    if (!string.IsNullOrWhiteSpace(updateData.Description))
                        item.Description = updateData.Description;
                    item.Cost = updateData.Cost;
                    item.Price = updateData.Price;
                    upds.Add(item);
                }
                _itemRepository.Update(upds, itm => itm.Description, itm => itm.Cost, itm => itm.Price);
                #endregion

                #region Add Section
                var addItemList = from sl in items
                                  where !localItemList.Any(li => li.SKU.ToUpper().Equals(sl.SKU.ToUpper()))
                                  select sl;

                foreach (var additem in addItemList)
                {
                    try
                    {
                        var newItem = new D_Item();
                        //newItem.SKU = additem.SKU;
                        //newItem.Name = additem.Name;
                        //newItem.Cost = additem.Cost;
                        //newItem.Price = additem.Price;
                        //newItem.Description = additem.Description;
                        AutoMapper.Mapper.Map(additem,newItem);
                        newItem.CreateTime = createTime;
                        newItem.CreateBy = createBy;
                        newItem.EditTime = createTime;
                        newItem.EditBy = createBy;
                        newItem.FillOutNull();

                        var imageURLs = newItem.Ref5.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        DownloadItemImages(imageURLs, newItem);
                        //PreDownloadItemImages(imageURLs, newItem);
                        //RestoreItemImages(imageURLs, newItem);

                        _itemRepository.Insert(newItem);
                    }
                    catch(Exception ex)
                    {
                        LogManager.Instance.Error(ex.Message);
                    }
                }

                #endregion


            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        private void RestoreItemImages(List<string> imageURLs, D_Item newItem)
        {
            #region Fetch/Restore from Pre-Downloaded Images

            DirectoryInfo di = new DirectoryInfo(@"C:\Users\gdutj\Downloads\3rdStockSystem\DSImages20200615\" + newItem.SKU + "\\");
            if (di.Exists)
            {
                var imageFiles = di.GetFiles("*", SearchOption.AllDirectories);
                int j = 0;
                foreach (var fi in imageFiles)
                {
                    //Image img = Image.FromFile(imgFile);

                    using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(fi.FullName)))
                    {
                        var fileName = newItem.SKU + "-" + j.ToString().PadLeft(2, '0') + ".jpg";
                        var imgObj = _imageService.SaveImage(stream, fileName);
                        newItem.ItemImages.Add(new M_ItemImage()
                        {
                            Image = imgObj,
                            DisplayOrder = j,
                            StatusID = 0,//TODO Get item active status id
                            CreateTime = DateTime.Now,
                            CreateBy = "System",
                            EditTime = DateTime.Now,
                            EditBy = "System"
                        });
                    }
                    j++;
                }
            }
            else
            {
                int i = 0;
                using (var wc = new ThirdStoreWebClient())
                {
                    foreach (var imageURL in imageURLs)
                    {
                        try
                        {
                            var imgBytes = wc.DownloadData(imageURL);
                            using (var stream = new MemoryStream(imgBytes))
                            {
                                var fileName = newItem.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                                var imgObj = _imageService.SaveImage(stream, fileName);
                                newItem.ItemImages.Add(new M_ItemImage()
                                {
                                    Image = imgObj,
                                    DisplayOrder = i,
                                    StatusID = 0,//TODO Get item active status id
                                    CreateTime = DateTime.Now,
                                    CreateBy = "System",
                                    EditTime = DateTime.Now,
                                    EditBy = "System"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Instance.Error(imageURL + " download failed. " + ex.Message);
                        }

                        i++;
                    }
                }
            }

            #endregion
        }

        private void PreDownloadItemImages(List<string> imageURLs, D_Item newItem)
        {
            #region Pre-Download and Save Images grouped by SKU
            DirectoryInfo di = new DirectoryInfo(@"C:\Users\gdutj\Downloads\3rdStockSystem\DSImages20200615\" + newItem.SKU + "\\");

            if (!di.Exists)
            {
                di.Create();
            }

            using (var wc = new ThirdStoreWebClient())
            {
                int i = 1;
                foreach (var imageURL in imageURLs)
                {
                    try
                    {
                        var imageFileName = newItem.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                        var saveImageFileFullName = Path.Combine(di.FullName, imageFileName);

                        wc.DownloadFile(imageURL, saveImageFileFullName);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(imageURL + " download failed. " + ex.Message);
                    }

                    i++;
                }
            }


            #endregion

            
        }

        private void DownloadItemImages(List<string> imageURLs, D_Item item)
        {
            if (imageURLs == null || imageURLs.Count == 0)
                return;

            var createTime = DateTime.Now;
            var createBy = _workContext.CurrentUserName;
            if (item.ItemImages.Count > 0)
            {
                var itemImages = GetItemImagesByItemID(item.ID);
                foreach (var existPic in itemImages)
                    DeleteItemImage(existPic);
            }

            int i = 0;
            using (var wc = new ThirdStoreWebClient())
            {
                foreach (var imageURL in imageURLs)
                {
                    try
                    {
                        var imgBytes = wc.DownloadData(imageURL);
                        using (var stream = new MemoryStream(imgBytes))
                        {
                            var fileName = item.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                            var imgObj = _imageService.SaveImage(stream, fileName);
                            item.ItemImages.Add(new M_ItemImage()
                            {
                                Image = imgObj,
                                DisplayOrder = i,
                                StatusID = 0,//TODO Get item active status id
                                CreateTime = createTime,
                                CreateBy = createBy,
                                EditTime = createTime,
                                EditBy = createBy
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(imageURL + " download failed. " + ex.Message);
                    }

                    i++;
                }
            }
        }

        public void UpdateChannelData()
        {
            try
            {
                var createTime = DateTime.Now;
                var createBy = Constants.SystemUser;
                var skuList = GetSupplierSKUList();
                var localItemList = GetAllItems();
                var updateItemList = from sl in skuList
                                     join li in localItemList on sl.SKU.ToUpper() equals li.SKU.ToUpper()
                                     select new
                                     {
                                         li.ID,
                                         SKU = sl.SKU.ToUpper(),
                                         sl.Title,
                                         sl.InventoryQty,
                                         sl.Price,
                                         sl.Description,
                                         sl.IsBulkyItem,
                                         sl.RrpPrice,
                                         sl.Category,
                                         sl.Discontinued,
                                         sl.EANCode,
                                         sl.Brand,
                                         sl.MPN,
                                         sl.Weight,
                                         sl.Length,
                                         sl.Width,
                                         sl.Height,
                                         sl.VIC,
                                         sl.NSW,
                                         sl.SA,
                                         sl.QLD,
                                         sl.TAS,
                                         sl.WA,
                                         sl.NT,
                                         sl.Image1,
                                         sl.Image2,
                                         sl.Image3,
                                         sl.Image4,
                                         sl.Image5,
                                         sl.Image6,
                                         sl.Image7,
                                         sl.Image8,
                                         sl.Image9,
                                         sl.Image10,
                                         sl.Image11,
                                         sl.Image12,
                                         sl.Image13,
                                         sl.Image14,
                                         sl.Image15,
                                         LocalListing = li
                                     };

                foreach (var updateItem in updateItemList)
                {
                    var item = updateItem.LocalListing;
                    if(!string.IsNullOrWhiteSpace(updateItem.Description))
                        item.Description = updateItem.Description;
                    item.Cost = updateItem.Price;
                    
                    var postage = new List<decimal>() {
                        updateItem.VIC.IsNumeric()? Convert.ToDecimal(updateItem.VIC):0,
                        updateItem.NSW.IsNumeric()? Convert.ToDecimal(updateItem.NSW):0,
                        updateItem.SA.IsNumeric()? Convert.ToDecimal(updateItem.SA):0,
                        updateItem.QLD.IsNumeric()? Convert.ToDecimal(updateItem.QLD):0,
                        updateItem.TAS.IsNumeric()? Convert.ToDecimal(updateItem.TAS):0,
                        updateItem.WA.IsNumeric()? Convert.ToDecimal(updateItem.WA):0,
                        updateItem.NT.IsNumeric()? Convert.ToDecimal(updateItem.NT):0
                        }.Max();


                    item.Price = (item.Cost+ postage) * Convert.ToDecimal(1.3);
                    //UpdateItem(item);
                    _itemRepository.Update(item,itm=> itm.Description,itm=>itm.Cost,itm=>itm.Price);
                }



                #region Add New Item
                var addItemList = from sl in skuList
                                  where !localItemList.Any(li => li.SKU.ToUpper().Equals(sl.SKU.ToUpper()))
                                  select sl;


                foreach (var additem in addItemList)
                {
                    try
                    {
                        if (additem.SKU.Length > 23)
                            LogManager.Instance.Info($"SKU {additem.SKU} length is larger than 23.");
                            //throw new Exception($"SKU {additem.SKU} length is larger than 23.");
                        var newItem = new D_Item();
                        newItem.SKU = additem.SKU;
                        newItem.Name = additem.Title;
                        newItem.Cost = additem.Price;
                        newItem.Price= newItem.Cost * Convert.ToDecimal(1.3);
                        newItem.Description = additem.Description;
                        newItem.Type = ThirdStoreItemType.SINGLE.ToValue();
                        newItem.SupplierID = ThirdStoreSupplier.P.ToValue();
                        newItem.GrossWeight =(!string.IsNullOrWhiteSpace(additem.Weight) ? Convert.ToDecimal(additem.Weight):0);
                        newItem.NetWeight = (!string.IsNullOrWhiteSpace(additem.Weight) ? Convert.ToDecimal(additem.Weight) : 0);
                        newItem.Length =(!string.IsNullOrWhiteSpace(additem.Length) ? Convert.ToDecimal(additem.Length)/100 : 0);
                        newItem.Width = (!string.IsNullOrWhiteSpace(additem.Width) ? Convert.ToDecimal(additem.Width) / 100 : 0);
                        newItem.Height =  (!string.IsNullOrWhiteSpace(additem.Height) ? Convert.ToDecimal(additem.Height) / 100 : 0);
                        //newItem.GrossWeight = additem.Weight;
                        //newItem.NetWeight = additem.Weight;
                        //newItem.Length = additem.Length;
                        //newItem.Width = additem.Width;
                        //newItem.Height = additem.Height;
                        newItem.Ref3 = additem.EANCode.Trim();
                        newItem.IsReadyForList =(additem.SKU.Length <= 23&&additem.Price<= Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceBelow)? true :false) ;
                        newItem.IsActive = true;
                        newItem.CreateTime = createTime;
                        newItem.CreateBy = createBy;
                        newItem.EditTime = createTime;
                        newItem.EditBy = createBy;
                        newItem.FillOutNull();


                        DownloadItemImages(additem, newItem);
                        #region Download and Save Item Images (Already moved to DownloadItemImages)
                        //var imagesURL = new List<string>();
                        //if (!string.IsNullOrEmpty(additem.Image1))
                        //    imagesURL.Add(additem.Image1);
                        //if (!string.IsNullOrEmpty(additem.Image2))
                        //    imagesURL.Add(additem.Image2);
                        //if (!string.IsNullOrEmpty(additem.Image3))
                        //    imagesURL.Add(additem.Image3);
                        //if (!string.IsNullOrEmpty(additem.Image4))
                        //    imagesURL.Add(additem.Image4);
                        //if (!string.IsNullOrEmpty(additem.Image5))
                        //    imagesURL.Add(additem.Image5);
                        //if (!string.IsNullOrEmpty(additem.Image6))
                        //    imagesURL.Add(additem.Image6);
                        //if (!string.IsNullOrEmpty(additem.Image7))
                        //    imagesURL.Add(additem.Image7);
                        //if (!string.IsNullOrEmpty(additem.Image8))
                        //    imagesURL.Add(additem.Image8);
                        //if (!string.IsNullOrEmpty(additem.Image9))
                        //    imagesURL.Add(additem.Image9);
                        //if (!string.IsNullOrEmpty(additem.Image10))
                        //    imagesURL.Add(additem.Image10);
                        //if (!string.IsNullOrEmpty(additem.Image11))
                        //    imagesURL.Add(additem.Image11);
                        //if (!string.IsNullOrEmpty(additem.Image12))
                        //    imagesURL.Add(additem.Image12);
                        //if (!string.IsNullOrEmpty(additem.Image13))
                        //    imagesURL.Add(additem.Image13);
                        //if (!string.IsNullOrEmpty(additem.Image14))
                        //    imagesURL.Add(additem.Image14);
                        //if (!string.IsNullOrEmpty(additem.Image15))
                        //    imagesURL.Add(additem.Image15);

                        //
                        //int i = 0;
                        //using (var wc = new ThirdStoreWebClient())
                        //{
                        //    foreach (var imageURL in imagesURL)
                        //    {
                        //        try
                        //        {
                        //            var imgBytes = wc.DownloadData(imageURL);
                        //            using (var stream = new MemoryStream(imgBytes))
                        //            {
                        //                var fileName = additem.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                        //                var imgObj = _imageService.SaveImage(stream, fileName);
                        //                newItem.ItemImages.Add(new M_ItemImage()
                        //                {
                        //                    Image = imgObj,
                        //                    DisplayOrder = i,
                        //                    StatusID = 0,//TODO Get item active status id
                        //                    CreateTime = createTime,
                        //                    CreateBy = createBy,
                        //                    EditTime = createTime,
                        //                    EditBy = createBy
                        //                });
                        //            }
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            LogManager.Instance.Error(imageURL + " download failed. " + ex.Message);
                        //        }

                        //        i++;
                        //    }
                        //}

                        #endregion

                        //DirectoryInfo di = new DirectoryInfo(ThirdStoreConfig.Instance.ThirdStoreImagesPath + "\\" + additem.SKU + "\\");
                        //DirectoryInfo di = new DirectoryInfo(@"C:\Users\gdutj\Downloads\3rdStockSystem\DSZImages20191120\" + additem.SKU + "\\");
                        

                        _itemRepository.Insert(newItem);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(ex.Message);
                    }
                }


                #endregion


                #region Persist DSZ Item Data
                

                //var fullFileName = ThirdStoreConfig.Instance.ThirdStoreDSZData+"\\"+CommonFunc.ToCSVFileName("DSZData");
                //_csvContext.Write(skuList, fullFileName,_csvFileDescription);

                //_dszItemRepository.Clear();
                //var insertItems = new List<D_DSZItem>();
                //foreach(var item in skuList)
                //{
                //    var insertItem = AutoMapper.Mapper.Map<DSZSKUModel, D_DSZItem>(item);
                //    insertItem.FillOutNull();
                //    insertItems.Add(insertItem);
                //}
                //_dszItemRepository.Insert(insertItems);

                #endregion

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }
        }


        private void DownloadItemImages(DSZSKUModel dszData,D_Item item)
        {
            var createTime = DateTime.Now;
            var createBy= _workContext.CurrentUserName;
            if(item.ItemImages.Count>0)
            {
                var itemImages = GetItemImagesByItemID(item.ID);
                foreach (var existPic in itemImages)
                    DeleteItemImage(existPic);
            }

            var imagesURL = new List<string>();
            if (!string.IsNullOrEmpty(dszData.Image1))
                imagesURL.Add(dszData.Image1);
            if (!string.IsNullOrEmpty(dszData.Image2))
                imagesURL.Add(dszData.Image2);
            if (!string.IsNullOrEmpty(dszData.Image3))
                imagesURL.Add(dszData.Image3);
            if (!string.IsNullOrEmpty(dszData.Image4))
                imagesURL.Add(dszData.Image4);
            if (!string.IsNullOrEmpty(dszData.Image5))
                imagesURL.Add(dszData.Image5);
            if (!string.IsNullOrEmpty(dszData.Image6))
                imagesURL.Add(dszData.Image6);
            if (!string.IsNullOrEmpty(dszData.Image7))
                imagesURL.Add(dszData.Image7);
            if (!string.IsNullOrEmpty(dszData.Image8))
                imagesURL.Add(dszData.Image8);
            if (!string.IsNullOrEmpty(dszData.Image9))
                imagesURL.Add(dszData.Image9);
            if (!string.IsNullOrEmpty(dszData.Image10))
                imagesURL.Add(dszData.Image10);
            if (!string.IsNullOrEmpty(dszData.Image11))
                imagesURL.Add(dszData.Image11);
            if (!string.IsNullOrEmpty(dszData.Image12))
                imagesURL.Add(dszData.Image12);
            if (!string.IsNullOrEmpty(dszData.Image13))
                imagesURL.Add(dszData.Image13);
            if (!string.IsNullOrEmpty(dszData.Image14))
                imagesURL.Add(dszData.Image14);
            if (!string.IsNullOrEmpty(dszData.Image15))
                imagesURL.Add(dszData.Image15);

            int i = 0;
            using (var wc = new ThirdStoreWebClient())
            {
                foreach (var imageURL in imagesURL)
                {
                    try
                    {
                        var imgBytes = wc.DownloadData(imageURL);
                        using (var stream = new MemoryStream(imgBytes))
                        {
                            var fileName = dszData.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                            var imgObj = _imageService.SaveImage(stream, fileName);
                            item.ItemImages.Add(new M_ItemImage()
                            {
                                Image = imgObj,
                                DisplayOrder = i,
                                StatusID = 0,//TODO Get item active status id
                                CreateTime = createTime,
                                CreateBy = createBy,
                                EditTime = createTime,
                                EditBy = createBy
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(imageURL + " download failed. " + ex.Message);
                    }

                    i++;
                }
            }
        }


        private IList<DSZSKUModel> GetSupplierSKUList()
        {
            IList<DSZSKUModel> skuList = null;
            using (var webClient = new ThirdStoreWebClient())
            {
                if (!Directory.Exists(ThirdStoreConfig.Instance.ThirdStoreDSZData))
                    Directory.CreateDirectory(ThirdStoreConfig.Instance.ThirdStoreDSZData);
                //var byCsv = webClient.DownloadData(Constants.DSZSKUListURL);
                var fileName = ThirdStoreConfig.Instance.ThirdStoreDSZData + "\\" + CommonFunc.ToCSVFileName("DSZData");
                //var fileName = ThirdStoreConfig.Instance.ThirdStoreDSZData + "\\DSZData_20200602125553.csv";

                //webClient.DownloadFile(@"https://idropship.com.au/pub/media/export_product_all_generic.csv", fileName);
                skuList = _csvContext.Read<DSZSKUModel>(fileName, _csvFileDescription).ToList();

                //webClient.DownloadFile(Constants.DSZSKUListURL, fileName);
                //skuList=_csvContext.Read<DSZSKUModel>(fileName, _csvFileDescription).ToList();

                //using (var ms = new MemoryStream(byCsv))
                //{
                //    ms.Position = 0;
                //    using (var sr = new StreamReader(ms))
                //    {
                //        //var csvFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true, TextEncoding = Encoding.Default };
                //        //var csvContext = new CsvContext();
                //        skuList = _csvContext.Read<DSZSKUModel>(sr, _csvFileDescription).ToList();

                //    }
                //}
                if(skuList!=null)
                    skuList = skuList.Where(s => !s.SKU.Contains("*") && !Regex.IsMatch(s.SKU, @"^V\d+")).ToList();
            }
            
            return skuList;
        }

        private string GetRealSKU(string sku)
        {
            if (Regex.IsMatch(sku, "(_D){1}$"))
            {
                sku = Regex.Replace(sku, "(_D){1}$", "");
            }
            return sku;
        }

        public void FetchNetoProducts()
        {
            try
            {
                _netoProductsRepository.Clear();
                var netoProducts = _netoAPIManager.GetNetoProducts(false);
                
                foreach(var p in netoProducts)
                {
                    var newProduct = new NetoProducts();
                    newProduct.NetoProductID = p.ID;
                    newProduct.SKU = p.SKU;
                    if (p.WarehouseQuantity != null)
                        newProduct.Qty = p.WarehouseQuantity[0].Quantity;
                    else
                        newProduct.Qty = "0";
                    newProduct.DefaultPrice = p.DefaultPrice.ToString();
                    newProduct.Name = p.Name;
                    newProduct.PrimarySupplier = p.PrimarySupplier;
                    if(p.Images.Count()>=1)
                        newProduct.Image1 = p.Images[0].URL;
                    if (p.Images.Count() >= 2)
                        newProduct.Image2 = p.Images[1].URL;
                    if (p.Images.Count() >= 3)
                        newProduct.Image3 = p.Images[2].URL;
                    if (p.Images.Count() >= 4)
                        newProduct.Image4 = p.Images[3].URL;
                    if (p.Images.Count() >= 5)
                        newProduct.Image5 = p.Images[4].URL;
                    if (p.Images.Count() >= 6)
                        newProduct.Image6 = p.Images[5].URL;
                    if (p.Images.Count() >= 7)
                        newProduct.Image7 = p.Images[6].URL;
                    if (p.Images.Count() >= 8)
                        newProduct.Image8 = p.Images[7].URL;
                    if (p.Images.Count() >= 9)
                        newProduct.Image9 = p.Images[8].URL;
                    if (p.Images.Count() >= 10)
                        newProduct.Image10 = p.Images[9].URL;
                    if (p.Images.Count() >= 11)
                        newProduct.Image11 = p.Images[10].URL;
                    if (p.Images.Count() >= 12)
                        newProduct.Image12 = p.Images[11].URL;
                    newProduct.ShippingHeight = p.ShippingHeight.ToString();
                    newProduct.ShippingLength = p.ShippingLength.ToString();
                    newProduct.ShippingWeight = p.ShippingWeight.ToString();
                    newProduct.ShippingWidth = p.ShippingWidth.ToString();

                    newProduct.FillOutNull();
                    _netoProductsRepository.Insert(newProduct);
                }

            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public void RedownloadImage(IList<int> ids)
        {
            try
            {
                //var di = new DirectoryInfo(ThirdStoreConfig.Instance.ThirdStoreDSZData);
                //if (di.Exists)
                //{
                //    FileInfo[] files = di.GetFiles().ToArray();
                //    if (files.Count() > 0)
                //    {
                //        var topDataFile = files.OrderByDescending(fi => fi.CreationTime).FirstOrDefault();
                //        var dszDatas = _csvContext.Read<DSZSKUModel>(topDataFile.FullName, _csvFileDescription);
                //        var items = this.GetItemsByIDs(ids);

                //        foreach(var item in items)
                //        {
                //            var dszData = dszDatas.FirstOrDefault(d=>d.SKU.ToLower().Equals(item.SKU.ToLower()));
                //            if(dszData!=null)
                //            {
                //                DownloadItemImages(dszData,item);
                //            }

                //            UpdateItem(item);
                //        }

                //    }
                //}


                var lstSKUImages = new List<Tuple<string, IList<string>>>();
                var items = this.GetItemsByIDs(ids);
                var grpItems = items.GroupBy(i=>i.SupplierID);
                foreach(var grp in grpItems)
                {
                    var dsChannel = _dsChannels.FirstOrDefault(c => c.DSChannel.Equals(grp.Key));
                    lstSKUImages.AddRange(dsChannel.GetImagesPathsBySKUs(grp.Select(i => i.SKU).ToList()));
                }

                foreach(var item in items)
                {
                    var images = lstSKUImages.FirstOrDefault(img=>img.Item1.ToLower().Equals(item.SKU.ToLower()));
                    if(images!=null)
                    {
                        DownloadItemImages(images.Item2.ToList(), item);
                        UpdateItem(item);
                    }
                }


            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        private IList<M_ItemImage> GetItemImagesByItemID(int itemID)
        {
            var query = from ii in _itemImageRepository.Table
                        where ii.ItemID.Equals(itemID)
                        orderby ii.DisplayOrder, ii.ID
                        select ii;
            return query.ToList();
        }

        private void DeleteItemImage(M_ItemImage itemImage)
        {
            if (itemImage == null)
                throw new ArgumentNullException("item Image");

            _itemImageRepository.Delete(itemImage);
        }
    }
}
