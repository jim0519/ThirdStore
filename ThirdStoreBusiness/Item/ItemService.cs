﻿using System;
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

namespace ThirdStoreBusiness.Item
{
    public class ItemService:IItemService
    {
        private readonly IRepository<D_Item> _itemRepository;
        private readonly IRepository<D_Item_Relationship> _itemRelationshipRepository;
        private readonly IDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly IImageService _imageService;
        private readonly CsvContext _csvContext;
        private readonly CsvFileDescription _csvFileDescription;

        public ItemService(IRepository<D_Item> itemRepository,
            IRepository<D_Item_Relationship> itemRelationshipRepository,
            IWorkContext workContext,
            IDbContext dbContext,
            IImageService imageService,
            CsvContext csvContext,
            CsvFileDescription csvFileDescription)
        {
            _itemRepository = itemRepository;
            _itemRelationshipRepository = itemRelationshipRepository;
            _dbContext = dbContext;
            _workContext = workContext;
            _imageService = imageService;
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
            var item = _itemRepository.Table.Where(i => i.SKU.ToLower().Equals(sku.ToLower())&&i.IsActive).FirstOrDefault();
            return item;

        }


        public IPagedList<D_Item> SearchItems(string sku = null, 
            ThirdStoreItemType? itemType = null, 
            string name = null,
            string aliasSKU = null,
            ThirdStoreSupplier? supplier = null,
            int isReadyForList = -1,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _itemRepository.Table;

            if (sku != null)
                query = query.Where(i => i.SKU.Contains(sku.ToLower()));
            if (itemType.HasValue)
            {
                var itemTypeID = itemType.Value.ToValue();
                query = query.Where(i => i.Type.Equals(itemTypeID));
            }
            if (name != null)
                query = query.Where(i => i.Name.Contains(name.ToLower()));

            if(aliasSKU!=null)
            {
                if(Regex.IsMatch( aliasSKU, "(_D){1}$"))
                {
                    aliasSKU = Regex.Replace(aliasSKU, "(_D){1}$", "");
                    query = query.Where(i => i.Ref2.Contains(aliasSKU));
                }
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
            skus = skus.Select(s => s.ToLower()).ToList();
            var items = _itemRepository.Table.Where(i => skus.Contains(i.SKU.ToLower()) && i.IsActive);
            return items.ToList();
        }

        public IList<D_Item> GetItemsByIDs(IList<int> ids)
        {
            var query = _itemRepository.Table.Where(i => ids.Contains(i.ID));
            return query.ToList();
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
                    item.Price = item.Cost * Convert.ToDecimal(1.3);

                    UpdateItem(item);
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
                            throw new Exception($"SKU {additem.SKU} length is larger than 23.");
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
                        newItem.IsReadyForList = false;
                        newItem.IsActive = true;
                        newItem.CreateTime = createTime;
                        newItem.CreateBy = createBy;
                        newItem.EditTime = createTime;
                        newItem.EditBy = createBy;
                        newItem.FillOutNull();


                       

                        var imagesURL = new List<string>();
                        if (!string.IsNullOrEmpty(additem.Image1))
                            imagesURL.Add(additem.Image1);
                        if (!string.IsNullOrEmpty(additem.Image2))
                            imagesURL.Add(additem.Image2);
                        if (!string.IsNullOrEmpty(additem.Image3))
                            imagesURL.Add(additem.Image3);
                        if (!string.IsNullOrEmpty(additem.Image4))
                            imagesURL.Add(additem.Image4);
                        if (!string.IsNullOrEmpty(additem.Image5))
                            imagesURL.Add(additem.Image5);
                        if (!string.IsNullOrEmpty(additem.Image6))
                            imagesURL.Add(additem.Image6);
                        if (!string.IsNullOrEmpty(additem.Image7))
                            imagesURL.Add(additem.Image7);
                        if (!string.IsNullOrEmpty(additem.Image8))
                            imagesURL.Add(additem.Image8);
                        if (!string.IsNullOrEmpty(additem.Image9))
                            imagesURL.Add(additem.Image9);
                        if (!string.IsNullOrEmpty(additem.Image10))
                            imagesURL.Add(additem.Image10);
                        if (!string.IsNullOrEmpty(additem.Image11))
                            imagesURL.Add(additem.Image11);
                        if (!string.IsNullOrEmpty(additem.Image12))
                            imagesURL.Add(additem.Image12);
                        if (!string.IsNullOrEmpty(additem.Image13))
                            imagesURL.Add(additem.Image13);
                        if (!string.IsNullOrEmpty(additem.Image14))
                            imagesURL.Add(additem.Image14);
                        if (!string.IsNullOrEmpty(additem.Image15))
                            imagesURL.Add(additem.Image15);

                        #region Download and Save Item Images
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

                        DirectoryInfo di = new DirectoryInfo(ThirdStoreConfig.Instance.ThirdStoreImagesPath + "\\" + additem.SKU + "\\");
                        #region Pre-Download and Save Images grouped by SKU

                        if (!di.Exists)
                        {
                            di.Create();
                        }

                        using (var wc = new ThirdStoreWebClient())
                        {
                            int i = 1;
                            foreach (var imageURL in imagesURL)
                            {
                                try
                                {
                                    var imageFileName = additem.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
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

                        #region Fetch/Restore from Pre-Downloaded Images

                        //if (di.Exists)
                        //{
                        //    var imageFiles = di.GetFiles("*", SearchOption.AllDirectories);
                        //    int j = 0;
                        //    foreach (var fi in imageFiles)
                        //    {
                        //        //Image img = Image.FromFile(imgFile);

                        //        using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(fi.FullName)))
                        //        {
                        //            var fileName = additem.SKU + "-" + j.ToString().PadLeft(2, '0') + ".jpg";
                        //            var imgObj = _imageService.SaveImage(stream, fileName);
                        //            newItem.ItemImages.Add(new M_ItemImage()
                        //            {
                        //                Image = imgObj,
                        //                DisplayOrder = j,
                        //                StatusID = 0,//TODO Get item active status id
                        //                CreateTime = DateTime.Now,
                        //                CreateBy = "System",
                        //                EditTime = DateTime.Now,
                        //                EditBy = "System"
                        //            });
                        //        }
                        //        j++;
                        //    }
                        //}
                        //else
                        //{
                        //    int i = 0;
                        //    using (var wc = new ThirdStoreWebClient())
                        //    {
                        //        foreach (var imageURL in imagesURL)
                        //        {
                        //            try
                        //            {
                        //                var imgBytes = wc.DownloadData(imageURL);
                        //                using (var stream = new MemoryStream(imgBytes))
                        //                {
                        //                    var fileName = additem.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                        //                    var imgObj = _imageService.SaveImage(stream, fileName);
                        //                    newItem.ItemImages.Add(new M_ItemImage()
                        //                    {
                        //                        Image = imgObj,
                        //                        DisplayOrder = i,
                        //                        StatusID = 0,//TODO Get item active status id
                        //                        CreateTime = createTime,
                        //                        CreateBy = createBy,
                        //                        EditTime = createTime,
                        //                        EditBy = createBy
                        //                    });
                        //                }
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                LogManager.Instance.Error(imageURL + " download failed. " + ex.Message);
                        //            }

                        //            i++;
                        //        }
                        //    }
                        //}

                        #endregion

                        _itemRepository.Insert(newItem);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error(ex.Message);
                    }
                }


                #endregion


                #region Persist DSZ Item Data
                if (!Directory.Exists(ThirdStoreConfig.Instance.ThirdStoreDSZData))
                    Directory.CreateDirectory(ThirdStoreConfig.Instance.ThirdStoreDSZData);

                var fullFileName = ThirdStoreConfig.Instance.ThirdStoreDSZData+"\\"+CommonFunc.ToCSVFileName("DSZData");
                _csvContext.Write(skuList, fullFileName,_csvFileDescription);

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


        private IList<DSZSKUModel> GetSupplierSKUList()
        {
            IList<DSZSKUModel> skuList = null;
            using (var webClient = new ThirdStoreWebClient())
            {
                var byCsv = webClient.DownloadData(Constants.DSZSKUListURL);

                using (var ms = new MemoryStream(byCsv))
                {
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        //var csvFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true, TextEncoding = Encoding.Default };
                        //var csvContext = new CsvContext();
                        skuList = _csvContext.Read<DSZSKUModel>(sr, _csvFileDescription).ToList();

                    }
                }
            }
            skuList = skuList.Where(s => !s.SKU.Contains("*")&&!Regex.IsMatch(s.SKU, @"^V\d+")).ToList();
            return skuList;
        }
    }
}
