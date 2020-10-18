using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStore.Models.Item;
using ThirdStoreFramework.Kendoui;
using ThirdStoreBusiness.Item;
using ThirdStore.Extensions;
using ThirdStoreFramework.Controllers;
using ThirdStoreCommon;
using ThirdStoreFramework.MVC;
using ThirdStoreBusiness.Image;
using ThirdStoreData;
using ThirdStoreCommon.Models.Item;
using System.Net;
using System.IO;
using ThirdStoreCommon.Models.Image;
using LINQtoCSV;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreBusiness.JobItem;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using ThirdStoreBusiness.AccessControl;

namespace ThirdStore.Controllers
{
    public class ItemController : BaseController
    {
        private readonly IItemService _itemService;
        private readonly IImageService _imageService;
        private readonly IDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly IJobItemService _jobItemService;
        private readonly IPermissionService _permissionService;

        public ItemController(IItemService itemService,
            IImageService imageService,
            IDbContext dbContext,
            IWorkContext workContext,
            IPermissionService permissionService,
            IJobItemService jobItemService)
        {
            _itemService = itemService;
            _imageService = imageService;
            _dbContext = dbContext;
            _workContext = workContext;
            _jobItemService = jobItemService;
            _permissionService = permissionService;
        }

        // GET: Item
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            
            var model = new ItemListViewModel();
            model.SearchTypes = ThirdStoreItemType.PART.ToSelectList(false).ToList();
            model.SearchTypes.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            model.Suppliers = ThirdStoreSupplier.P.ToSelectList(false).ToList();
            model.Suppliers.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            model.YesOrNo = YesNo.Y.ToSelectList(false).ToList();
            model.YesOrNo.Insert(0, new SelectListItem { Text = "", Value = "-1", Selected = true });
            model.SearchReadyForList  = -1;

            model.BulkUpdate.IsReadyForList = -1;
            model.BulkUpdate.IsActive = -1;

            //var showSyncInvUsers = new int[] { 1,4 ,10, 14,16,17 };
            //if (showSyncInvUsers.Contains(_workContext.CurrentUser.ID))
            //    model.ShowSyncInventory = true;
            model.ShowSyncInventory = _permissionService.Authorize(ThirdStorePermission.JobItemSync.ToName());

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, ItemListViewModel model)
        {
            ThirdStoreItemType? itemType = model.SearchType > 0 ? (ThirdStoreItemType?)(model.SearchType) : null;
            ThirdStoreSupplier? supplier = model.SearchSupplier > 0 ? (ThirdStoreSupplier?)(model.SearchSupplier) : null;
            var items = _itemService.SearchItems(
                sku:model.SearchSKU,
                itemType: itemType, 
                name:model.SearchName,
                aliasSKU:model.SearchAliasSKU,
                refSKU:model.SearchReferenceSKU,
                supplier: supplier,
                isReadyForList: model.SearchReadyForList,
                pageIndex: command.Page - 1,
                pageSize:command.PageSize);

            var itemGridViewList = items.Select(i => i.ToModel());

            var gridModel = new DataSourceResult() { Data = itemGridViewList, Total = items.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Create()
        {
            //var allowCreateNewItemUserIDs = new int[] { 1,4,6,7, 14, 17 };
            //if (!allowCreateNewItemUserIDs.Contains(_workContext.CurrentUser.ID))
            //{
            //    ErrorNotification("You do not have permission to process this page.");
            //    return Redirect("~/"); ;
            //}

            if (!_permissionService.Authorize(ThirdStorePermission.SKUEdit.ToName()))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/");
            }

            var newItemViewModel = new ItemViewModel() { IsActive=1,Type=ThirdStoreItemType.SINGLE.ToValue(),IsReadyForList=0};

            FillDropDownDS(newItemViewModel);

            return View(newItemViewModel);
        }

        [HttpPost]
        public ActionResult Create(ItemViewModel model)
        {
            FillDropDownDS(model);
            //checking
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Values.SelectMany(v => v.Errors.Select(er => er.ErrorMessage)).Aggregate((current, next) => current + Environment.NewLine + next);
                ErrorNotification(errMsg);
                return View(model);
            }
            //if (!IsComboSingleQualify(model))
            //{
            //    ErrorNotification("Combo should have child item and single should have not");
            //    return View(model);
            //}
            //if (_itemService.IsDuplicateSKU(model.SKU))
            //{
            //    ErrorNotification("Duplicate SKU exists");
            //    return View(model);
            //}
            var createTime = DateTime.Now;
            var createBy = Constants.SystemUser;
            var newEntityModel = model.ToCreateNewEntity().FillOutNull();
            newEntityModel.SKU = newEntityModel.SKU.Trim().ToUpper();
            //newEntityModel.CreateTime = createTime;
            //newEntityModel.CreateBy = createBy;
            //newEntityModel.EditTime = createTime;
            //newEntityModel.EditBy = createBy;
            if (model.ChildItemLines != null && model.ChildItemLines.Count > 0)
            {
                foreach (var lModel in model.ChildItemLines)
                {
                    var newEntityLine = lModel.ToEntity().FillOutNull();
                    newEntityLine.ChildItemID = _itemService.GetItemBySKU(lModel.ChildItemSKU).ID;
                    newEntityLine.CreateTime = createTime;
                    newEntityLine.CreateBy = createBy;
                    newEntityLine.EditTime = createTime;
                    newEntityLine.EditBy = createBy;
                    newEntityModel.ChildItems.Add(newEntityLine);
                }
            }

            if (model.ItemViewImages != null && model.ItemViewImages.Count > 0)
            {
                foreach (var lModel in model.ItemViewImages)
                {
                    var newEntityLine = lModel.ToEntity().FillOutNull();
                    newEntityLine.CreateTime = createTime;
                    newEntityLine.CreateBy = createBy;
                    newEntityLine.EditTime = createTime;
                    newEntityLine.EditBy = createBy;
                    newEntityModel.ItemImages.Add(newEntityLine);
                }
            }
            _itemService.InsertItem(newEntityModel);

            return RedirectToAction("Edit", new { itemID = newEntityModel.ID });
        }
        //[ActionName("Delete")]
        public ActionResult EditBySKU(string sku)
        {
            var item = _itemService.GetItemBySKU(sku);
            if(item==null)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { itemID = item.ID });
        }

        public ActionResult Edit(int itemID)
        {
            

            var editItemViewModel = new ItemViewModel();
            

            var item = _itemService.GetItemByID(itemID);
            if (item != null)
            {
                editItemViewModel = item.ToCreateNewModel();
            }

            //var showSyncInvUsers = new int[] { 1, 4, 10, 14, 16, 17 };
            //if (showSyncInvUsers.Contains(_workContext.CurrentUser.ID))
            //    editItemViewModel.ShowSyncInventory = true;
            editItemViewModel.ShowSyncInventory = _permissionService.Authorize(ThirdStorePermission.JobItemSync.ToName());

            FillDropDownDS(editItemViewModel);
            return View(editItemViewModel);
        }

        [HttpPost]
        public ActionResult Edit(ItemViewModel model)
        {
            //var allowEditItemUserIDs = new int[] { 1,4, 6, 7,10, 14,16, 17 };
            //if (!allowEditItemUserIDs.Contains(_workContext.CurrentUser.ID))
            //{
            //    ErrorNotification("You do not have permission to process this page.");
            //    return Redirect("~/"); ;
            //}

            if (!_permissionService.Authorize(ThirdStorePermission.SKUEdit.ToName()))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/");
            }

            FillDropDownDS(model);
            //checking
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Values.SelectMany(v => v.Errors.Select(er=>er.ErrorMessage)).Aggregate((current, next) => current + Environment.NewLine + next);
                ErrorNotification(errMsg);
                return View(model);
            }
            //if (_itemService.IsDuplicateSKU(model.SKU)&&!_itemService.GetItemByID(model.ID).SKU.ToLower().Equals(model.SKU.ToLower()))
            //{
            //    ErrorNotification("Duplicate SKU exists");
            //    return View(model);
            //}

            var editTime = DateTime.Now;
            var editBy = Constants.SystemUser;

            var editEntityModel = _itemService.GetItemByID(model.ID);
            editEntityModel = model.ToCreateNewEntity(editEntityModel).FillOutNull();
            editEntityModel.SKU = editEntityModel.SKU.Trim().ToUpper();
            //editEntityModel.EditTime = editTime;
            //editEntityModel.EditBy = editBy;

            //editEntityModel.D_Order_Line.Remove(editEntityModel.D_Order_Line.FirstOrDefault());
            //editEntityModel.D_Order_Line.Clear();
            //foreach (var removeLine in editEntityModel.D_Order_Line)
            //{ 

            //}
            if (model.ChildItemLines != null && model.ChildItemLines.Count > 0)
            {
                foreach (var lModel in model.ChildItemLines)
                {
                    var childItemID = _itemService.GetItemBySKU(lModel.ChildItemSKU).ID;
                    if(lModel.ID>0)
                    { 
                        var originLine = editEntityModel.ChildItems.Where(l => l.ID == lModel.ID).FirstOrDefault();
                        if (originLine != null)
                        {
                            originLine = lModel.ToEntity(originLine).FillOutNull();
                            originLine.ChildItemID = childItemID;
                            originLine.EditTime = editTime;
                            originLine.EditBy = editBy;
                        }
                    }
                    else
                    {
                        var editEntityLine = lModel.ToEntity().FillOutNull();
                        editEntityLine.ChildItemID = childItemID;
                        editEntityLine.CreateTime = editTime;
                        editEntityLine.CreateBy = editBy;
                        editEntityLine.EditTime = editTime;
                        editEntityLine.EditBy = editBy;
                        editEntityModel.ChildItems.Add(editEntityLine);
                    }
                }
            }

            if (model.ItemViewImages != null && model.ItemViewImages.Count > 0)
            {
                foreach (var lModel in model.ItemViewImages)
                {
                    if (lModel.ID > 0)
                    {
                        var originLine = editEntityModel.ItemImages.Where(l => l.ID == lModel.ID).FirstOrDefault();
                        if (originLine != null)
                        {
                            originLine = lModel.ToEntity(originLine).FillOutNull();
                            originLine.EditTime = editTime;
                            originLine.EditBy = editBy;
                        }
                    }
                    else
                    {
                        var editEntityLine = lModel.ToEntity().FillOutNull();
                        editEntityLine.CreateTime = editTime;
                        editEntityLine.CreateBy = editBy;
                        editEntityLine.EditTime = editTime;
                        editEntityLine.EditBy = editBy;
                        editEntityModel.ItemImages.Add(editEntityLine);
                    }
                }
            }

            _itemService.UpdateItem(editEntityModel);

            return RedirectToAction("Edit", new { itemID = editEntityModel.ID });

        }


        [HttpPost]
        public ActionResult ReadChildItemLines(DataSourceRequest command, int itemID)
        {
            if (itemID > 0)
            {
                IList<ItemViewModel.ChildItemLineViewModel> childItemLines = null;
                var item = _itemService.GetItemByID(itemID);
                if (item != null)
                {
                    childItemLines = item.ChildItems.Select(r => r.ToModel()).ToList();
                }


                var gridModel = new DataSourceResult() { Data = childItemLines, Total = childItemLines.Count };


                //return View();
                return new JsonResult
                {
                    Data = gridModel
                };
            }
            else
                return Json(new object{  });
        }

        [HttpPost]
        public ActionResult ChildItemCheck(ItemViewModel.ChildItemLineViewModel model)
        {
            
            var item = _itemService.GetItemBySKU(model.ChildItemSKU);
            if (item == null)
            {
                var jsonResult = new DataSourceResult();
                //return jas("No item found with SKU");
                jsonResult.Errors = "No item found with SKU" ;
                return new JsonResult
                {
                    Data = jsonResult
                };
            }
            model.ChildItemID = item.ID;//Todo: update new child item id to line in UI  
            //jsonResult.ExtraData=model;
            //return new JsonResult
            //    {
            //        Data = jsonResult
            //    };
            //return new NullJsonResult();
            return Json(new[] { model });
        }

        [HttpPost]
        public ActionResult CheckInputSKU(string inputSKU)
        {
            var item = _itemService.GetItemBySKU(inputSKU);
            if (item == null)
            {
                return Json(new { Result = false, ErrMessage = "SKU Not Exists" });
            }
            else if (item.ChildItems.Count > 1)
            {
                return Json(new { Result = false, ErrMessage = "SKU cannot contain more than 1 sub SKU." });
            }
            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult ChildItemDelete(ItemViewModel.ChildItemLineViewModel model)
        {
            if (model.ID > 0)
            {
                var childItem = _itemService.GetChildItemByID(model.ID);
                if (childItem != null)
                    _itemService.DeleteChildItem(childItem);
            }
            
            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult GetItemInfo(string sku)
        {
            var item = _itemService.GetItemBySKU(sku);
            if (item == null)
            {
                return Json(new { Result = false });
            }
            return Json(new { Result = true,ItemID=item.ID, ItemLength= item.Length, ItemHeight=item.Height, ItemWidth=item.Width, ItemWeight=item.GrossWeight,ItemCubicWeight=item.Length*item.Width*item.Height*250, ItemCubicMeter= item.Length * item.Width * item.Height });
        }

        [HttpPost]
        public ActionResult UploadImages(HttpPostedFileBase[] itemImages)
        {

            var lstSavedImages = new List<ItemViewModel.ItemImageViewModel>();
            if (itemImages != null)
            {
                foreach (var imgFile in itemImages)
                {
                    var img = _imageService.SaveImage(imgFile.InputStream, imgFile.FileName);
                    var imgViewModel = new ItemViewModel.ItemImageViewModel() { ImageID = img.ID, ImageName = img.ImageName, ImageURL = _imageService.GetImageURL(img.ID) };
                    lstSavedImages.Add(imgViewModel);
                }
            }

            return Json(new { ImageList = lstSavedImages });
        }


        [HttpPost]
        public ActionResult ReadItemImages(DataSourceRequest command, int itemID)
        {
            if (itemID > 0)
            {
                IList<ItemViewModel.ItemImageViewModel> itemImages = null;
                var item = _itemService.GetItemByID(itemID);
                if (item != null)
                {
                    itemImages = item.ItemImages.Select(r =>
                    {
                        var viewModel = r.ToModel();
                        viewModel.ImageURL = _imageService.GetImageURL(r.ImageID);
                        viewModel.ImageName = r.Image.ImageName;
                        return viewModel;
                    }).ToList();
                }


                var gridModel = new DataSourceResult() { Data = itemImages, Total = itemImages.Count };


                //return View();
                return new JsonResult
                {
                    Data = gridModel
                };
            }
            else
                return Json(new object { });
        }


        [HttpPost]
        public ActionResult ItemImageDelete(ItemViewModel.ItemImageViewModel model)
        {
            if (model.ID > 0)
            {
                var image = _imageService.GetImageByID(model.ImageID);
                if (image != null)
                    _imageService.DeleteImage(image);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult FetchNetoProducts()
        {
            try
            {
                _itemService.FetchNetoProducts();
                return Json(new { Result = true });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, ErrMsg = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SyncInventory(string selectedIDs)
        {
            try
            {
                var ids = selectedIDs
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();

                var retMessage = _jobItemService.SyncInventory(ids);
                if (retMessage.IsSuccess)
                    return Json(new { Result = true });
                else
                    return Json(new { Result = false, Message = retMessage.Mesage });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, ErrMsg = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult RedownloadImage(string selectedIDs)
        {
            try
            {
                var ids = selectedIDs
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();

                 _itemService.RedownloadImage(ids);
                return Json(new { Result = true });
            }
            catch(Exception ex)
            {
                return Json(new { Result = false, ErrMsg = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ImportData()
        {
            try
            {
                var sqlStr = @"
                select 
                DS.SKU,
                2 as Type,
                DS.Title as Name,
                DS.Description as Description,
                convert(decimal(18,2),isnull(DS.price,0)) as Cost,
                convert(decimal(18,2),isnull(DS.Price*1.2,0)) as Price,
                convert(decimal(18,8),isnull(DS.[Weight (kg)],0)) as GrossWeight,
                convert(decimal(18,8),isnull(DS.[Weight (kg)],0)) as NetWeight,
                convert(decimal(18,8),0) as CubicWeight,
                convert(decimal(18,8),isnull(DS.[Carton Length (cm)],0)/100) as Length,
                convert(decimal(18,8),isnull(DS.[Carton Width (cm)],0)/100) as Width,
                convert(decimal(18,8),isnull(DS.[Carton Height (cm)],0)/100) as Height,
                1 as SupplierID,
                convert(bit,1) as IgnoreListing,
                convert(bit,1) as IsActive,
                '' as Ref1,
                '' as Ref2,
                '' as Ref3,
                '' as Ref4,
                '' as Ref5,
                DS.[Image 1] as Image1,
                DS.[Image 2] as Image2,
                DS.[Image 3] as Image3,
                DS.[Image 4] as Image4,
                DS.[Image 5] as Image5,
                DS.[Image 6] as Image6,
                '' as Image7,
                '' as Image8,
                '' as Image9,
                '' as Image10,
                '' as Image11,
                '' as Image12,
                GETDATE() as CreateTime,
                'System' as CreateBy,
                GETDATE() as EditTime,
                'System' as EditBy
                from
                DropshipzoneSKU DS 
                where [EAN Code] is not null
                and not exists 
                (select 1 from D_Item E where E.SKU=DS.SKU)";
                var importData = _dbContext.SqlQuery<ImportItem>(sqlStr).ToList();
                //var csvContext = new CsvContext();
                //var inputFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true };
                //var importData = csvContext.Read<SelloItem>(@"C:\Users\gdutj\OneDrive\Document\Code\3rdStore\TODOList\FileNew20190820\ALL_Without_T_R.Jul.csv", inputFileDescription);
                foreach (var d in importData)
                {
                    var newItem = new D_Item();
                    newItem.SKU = d.SKU;
                    newItem.Type = d.Type;
                    newItem.Name = d.Name;
                    newItem.Description = d.Description;
                    newItem.Cost = d.Cost;
                    newItem.Price = d.Price;
                    newItem.GrossWeight = d.GrossWeight;
                    newItem.NetWeight = d.NetWeight;
                    newItem.CubicWeight = d.CubicWeight;
                    newItem.Length = d.Length;
                    newItem.Width = d.Width;
                    newItem.Height = d.Height;
                    newItem.SupplierID = d.SupplierID;
                    newItem.IsReadyForList = d.IsReadyForList;
                    newItem.IsActive = d.IsActive;
                    newItem.Ref1 = d.Ref1;
                    newItem.Ref2 = d.Ref2;
                    newItem.Ref3 = d.Ref3;
                    newItem.Ref4 = d.Ref4;
                    newItem.Ref5 = d.Ref5;
                    newItem.CreateTime = DateTime.Now;
                    newItem.CreateBy = "System";
                    newItem.EditTime = DateTime.Now;
                    newItem.EditBy = "System";

                    var imagesURL = new List<string>();
                    if (!string.IsNullOrEmpty(d.Image1))
                        imagesURL.Add(d.Image1);
                    if (!string.IsNullOrEmpty(d.Image2))
                        imagesURL.Add(d.Image2);
                    if (!string.IsNullOrEmpty(d.Image3))
                        imagesURL.Add(d.Image3);
                    if (!string.IsNullOrEmpty(d.Image4))
                        imagesURL.Add(d.Image4);
                    if (!string.IsNullOrEmpty(d.Image5))
                        imagesURL.Add(d.Image5);
                    if (!string.IsNullOrEmpty(d.Image6))
                        imagesURL.Add(d.Image6);
                    if (!string.IsNullOrEmpty(d.Image7))
                        imagesURL.Add(d.Image7);
                    if (!string.IsNullOrEmpty(d.Image8))
                        imagesURL.Add(d.Image8);
                    if (!string.IsNullOrEmpty(d.Image9))
                        imagesURL.Add(d.Image9);
                    if (!string.IsNullOrEmpty(d.Image10))
                        imagesURL.Add(d.Image10);
                    if (!string.IsNullOrEmpty(d.Image11))
                        imagesURL.Add(d.Image11);
                    if (!string.IsNullOrEmpty(d.Image12))
                        imagesURL.Add(d.Image12);

                    int i = 0;
                    using (var wc = new WebClient())
                    {
                        foreach (var imageURL in imagesURL)
                        {
                            try
                            {
                                var imgBytes = wc.DownloadData(imageURL);
                                using (var stream = new MemoryStream(imgBytes))
                                {
                                    var fileName= d.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                                    var imgObj = _imageService.SaveImage(stream, fileName);
                                    newItem.ItemImages.Add(new M_ItemImage()
                                    {
                                        Image = imgObj,
                                        DisplayOrder = i ,
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
                    _itemService.InsertItem(newItem);
                }
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }
           

            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult ImportNetoOZAuctionData()
        {
            try
            {
                var csvContext = new CsvContext();
                var inputFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true };
                var importData = csvContext.Read<ImportNetoOZAuctionItem>(@"C:\Users\gdutj\OneDrive\Document\Code\3rdStore\TODOList\ExportAuctionDatabase.csv", inputFileDescription);
                var allItems = _itemService.GetAllItems();
                foreach (var d in importData)
                {
                    if(allItems.FirstOrDefault(itm=>itm.SKU.ToLower().Equals(d.SKU.Trim().ToLower()))!=null)
                    {
                        LogManager.Instance.Error($"SKU {d.SKU} already exists.");
                        continue;
                    }
                    var newItem = new D_Item();
                    newItem.SKU = d.SKU;
                    newItem.Type = 2;
                    newItem.Name = d.Name;
                    newItem.Description = d.Description;
                    newItem.Price =Convert.ToDecimal( d.Price);
                    newItem.GrossWeight = d.GrossWeight;
                    newItem.NetWeight = d.GrossWeight;
                    newItem.Length = d.Length/100;
                    newItem.Width = d.Width / 100;
                    newItem.Height = d.Height / 100;
                    newItem.SupplierID =ThirdStoreSupplier.A.ToValue();
                    newItem.IsReadyForList = false;
                    newItem.IsActive = true;
                    newItem.Ref5 = d.CategoryPath;
                    newItem.CreateTime = DateTime.Now;
                    newItem.CreateBy = "System";
                    newItem.EditTime = DateTime.Now;
                    newItem.EditBy = "System";

                    var imagesURL = new List<string>();
                    if (!string.IsNullOrEmpty(d.Image1))
                        imagesURL.Add(d.Image1);
                    if (!string.IsNullOrEmpty(d.Image2))
                        imagesURL.Add(d.Image2);
                    if (!string.IsNullOrEmpty(d.Image3))
                        imagesURL.Add(d.Image3);
                    if (!string.IsNullOrEmpty(d.Image4))
                        imagesURL.Add(d.Image4);
                    if (!string.IsNullOrEmpty(d.Image5))
                        imagesURL.Add(d.Image5);
                    if (!string.IsNullOrEmpty(d.Image6))
                        imagesURL.Add(d.Image6);

                    int i = 0;
                    using (var wc = new WebClient())
                    {
                        foreach (var imageURL in imagesURL)
                        {
                            try
                            {
                                var imgBytes = wc.DownloadData(imageURL);
                                using (var stream = new MemoryStream(imgBytes))
                                {
                                    var fileName = d.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
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
                    newItem.FillOutNull();
                    _itemService.InsertItem(newItem);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }


            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult ImportSelloData()
        {
            try
            {
                var csvContext = new CsvContext();
                var inputFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true };
                var selloData = csvContext.Read<SelloItem>(@"C:\Users\gdutj\Downloads\3rdStockSystem\SelloData6.csv", inputFileDescription);
                var importItems = new List<ImportItem>();
                foreach(var sd in selloData)
                {
                    var item = new ImportItem();
                    item.SKU = sd.SKU.Trim();
                    item.Name = sd.Name.Trim();
                    item.Type = ThirdStoreItemType.SINGLE.ToValue();
                    var selloDesc = this.TextToHtml( sd.Description + "\r\n\r\n Specification:\r\n" + sd.Specification + "\r\n\r\n Package Content:\r\n" + sd.PackageContent);
                    item.Description = selloDesc;
                    item.Price = 0;
                    item.Cost = 0;
                    item.GrossWeight =  (sd.Weight.ToString().Replace(".", "").Length > 10 ? 0 : sd.Weight);
                    var width = sd.Width / 100;
                    var length= sd.Length / 100;
                    var height= sd.Height / 100;
                    item.Width = (width.ToString().Replace(".", "").Length > 10 ? 0 : width);
                    item.Length = (length.ToString().Replace(".", "").Length > 10 ? 0 : length);
                    item.Height = (height.ToString().Replace(".", "").Length > 10 ? 0 : height);
                    item.SupplierID = ThirdStoreSupplier.S.ToValue();
                    item.IsReadyForList = false;
                    item.IsActive = true;
                    item.Image1 = sd.Image1;
                    item.Image2 = sd.Image2;
                    item.Image3 = sd.Image3;
                    item.Image4 = sd.Image4;
                    item.Image5 = sd.Image5;
                    item.Image6 = sd.Image6;
                    item.Image7 = sd.Image7;
                    item.Image8 = sd.Image8;

                    importItems.Add(item);



                    //    var imagesURL = new List<string>();
                    //    if (!string.IsNullOrEmpty(sd.Image1))
                    //        imagesURL.Add(sd.Image1);
                    //    if (!string.IsNullOrEmpty(sd.Image2))
                    //        imagesURL.Add(sd.Image2);
                    //    if (!string.IsNullOrEmpty(sd.Image3))
                    //        imagesURL.Add(sd.Image3);
                    //    if (!string.IsNullOrEmpty(sd.Image4))
                    //        imagesURL.Add(sd.Image4);
                    //    if (!string.IsNullOrEmpty(sd.Image5))
                    //        imagesURL.Add(sd.Image5);
                    //    if (!string.IsNullOrEmpty(sd.Image6))
                    //        imagesURL.Add(sd.Image6);
                    //    if (!string.IsNullOrEmpty(sd.Image7))
                    //        imagesURL.Add(sd.Image7);
                    //    if (!string.IsNullOrEmpty(sd.Image8))
                    //        imagesURL.Add(sd.Image8);

                    //    DirectoryInfo di = new DirectoryInfo(ThirdStoreConfig.Instance.ThirdStoreImagesPath + "\\"+ item.SKU + "\\");

                    //    if (!di.Exists)
                    //    {
                    //        di.Create();
                    //    }

                    //    using (var wc = new WebClient())
                    //    {
                    //        int i = 1;
                    //        foreach (var imageURL in imagesURL)
                    //        {
                    //            try
                    //            {
                    //                var imageFileName = item.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                    //                var saveImageFileFullName = Path.Combine(di.FullName, imageFileName);

                    //                wc.DownloadFile(imageURL, saveImageFileFullName);
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                LogManager.Instance.Error(imageURL + " download failed. " + ex.Message);
                    //            }

                    //            i++;
                    //        }
                    //    }
                }

                this.SaveItems(importItems, true, false);


                }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }


            return Json(new { Result = true });
        }

        public async Task<ActionResult> MultiThreadDownloadPicAsyc()
        {
            try
            {
                var csvContext = new CsvContext();
                var inputFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true };
                var selloData = csvContext.Read<SelloItem>(@"C:\Users\gdutj\Downloads\3rdStockSystem\SelloData2.csv", inputFileDescription);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                // the code that you want to measure comes here
                var j = 0;
                foreach (var d in selloData.OrderBy(sd=>sd.Name))
                {
                    if (j > 20)
                        break;
                    var imagesURL = new List<string>();
                    if (!string.IsNullOrEmpty(d.Image1))
                        imagesURL.Add(d.Image1);
                    if (!string.IsNullOrEmpty(d.Image2))
                        imagesURL.Add(d.Image2);
                    if (!string.IsNullOrEmpty(d.Image3))
                        imagesURL.Add(d.Image3);
                    if (!string.IsNullOrEmpty(d.Image4))
                        imagesURL.Add(d.Image4);
                    if (!string.IsNullOrEmpty(d.Image5))
                        imagesURL.Add(d.Image5);
                    if (!string.IsNullOrEmpty(d.Image6))
                        imagesURL.Add(d.Image6);
                    if (!string.IsNullOrEmpty(d.Image7))
                        imagesURL.Add(d.Image7);
                    if (!string.IsNullOrEmpty(d.Image8))
                        imagesURL.Add(d.Image8);

                    int i = 0;
                    foreach (var imgUrl in imagesURL)
                    {
                        try
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                                {
                                    NoCache = true
                                };
                                using (var response = await client.GetAsync(imgUrl))
                                {
                                    response.EnsureSuccessStatusCode();

                                    using (var stream = await response.Content.ReadAsStreamAsync())
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            //var fileName = d.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                                            //_imageService.SaveImage(stream, fileName);
                                        }
                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {

                        }
                        i++;
                    }
                    j++;
                }
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("List");
        }


        public ActionResult MultiThreadDownloadPic()
        {
            try
            {
                var csvContext = new CsvContext();
                var inputFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true };
                var selloData = csvContext.Read<SelloItem>(@"C:\Users\gdutj\Downloads\3rdStockSystem\SelloData3.csv", inputFileDescription);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var j = 0;
                foreach (var d in selloData.OrderBy(sd => sd.Name))
                {
                    if (j > 20)
                        break;
                    var imagesURL = new List<string>();
                    if (!string.IsNullOrEmpty(d.Image1))
                        imagesURL.Add(d.Image1);
                    if (!string.IsNullOrEmpty(d.Image2))
                        imagesURL.Add(d.Image2);
                    if (!string.IsNullOrEmpty(d.Image3))
                        imagesURL.Add(d.Image3);
                    if (!string.IsNullOrEmpty(d.Image4))
                        imagesURL.Add(d.Image4);
                    if (!string.IsNullOrEmpty(d.Image5))
                        imagesURL.Add(d.Image5);
                    if (!string.IsNullOrEmpty(d.Image6))
                        imagesURL.Add(d.Image6);
                    if (!string.IsNullOrEmpty(d.Image7))
                        imagesURL.Add(d.Image7);
                    if (!string.IsNullOrEmpty(d.Image8))
                        imagesURL.Add(d.Image8);

                    int i = 0;
                    foreach (var imgUrl in imagesURL)
                    {
                        try
                        {
                            using (var client = new WebClient())
                            {
                                client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                                var imgBytes = client.DownloadData(imgUrl);
                                using (var stream = new MemoryStream(imgBytes))
                                {
                                    var fileName = d.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                                    //_imageService.SaveImage(stream, fileName);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        i++;
                    }
                    j++;
                }
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("List");
        }

        private string TextToHtml(string text)
        {
            //text= text.Replace("&nbsp;", "  ");
            text = HttpUtility.HtmlDecode(text);
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n", "\r");
            text = text.Replace("\r", "<br>\r\n");
            text = text.Replace("  ", " &nbsp;");
            return text;
        }

        private void SaveItems(IList<ImportItem> importItems, bool updateImage = false,bool updateProductDetail=false)
        {
            try
            {
                var allItems = _itemService.GetAllItems();
                foreach (var d in importItems)
                {
                    D_Item itemObj = null;
                    var existingItem = allItems.FirstOrDefault(itm => itm.SKU.ToLower().Equals(d.SKU.Trim().ToLower()));
                    if (existingItem != null)
                    {
                        LogManager.Instance.Info($"SKU {d.SKU} exists.");
                        //if (!updateItemForSameSKU)
                        //    continue;
                        itemObj = existingItem;
                    }
                    else
                    {
                        itemObj = new D_Item();
                    }
                    if (updateProductDetail || existingItem == null)
                    {
                        itemObj.SKU = d.SKU;
                        itemObj.Type = d.Type;
                        itemObj.Name = d.Name;
                        itemObj.Description = d.Description;
                        itemObj.Cost = d.Cost;
                        itemObj.Price = d.Price;
                        itemObj.GrossWeight = d.GrossWeight;
                        itemObj.NetWeight = d.NetWeight;
                        itemObj.CubicWeight = d.CubicWeight;
                        itemObj.Length = d.Length;
                        itemObj.Width = d.Width;
                        itemObj.Height = d.Height;
                        itemObj.SupplierID = d.SupplierID;
                        itemObj.IsReadyForList = d.IsReadyForList;
                        itemObj.IsActive = d.IsActive;
                        itemObj.Ref1 = d.Ref1;
                        itemObj.Ref2 = d.Ref2;
                        itemObj.Ref3 = d.Ref3;
                        itemObj.Ref4 = d.Ref4;
                        itemObj.Ref5 = d.Ref5;
                        itemObj.CreateTime = DateTime.Now;
                        itemObj.CreateBy = "System";
                        itemObj.EditTime = DateTime.Now;
                        itemObj.EditBy = "System";
                        itemObj.FillOutNull();
                    }


                    var imagesURL = new List<string>();
                    if (!string.IsNullOrEmpty(d.Image1))
                        imagesURL.Add(d.Image1);
                    if (!string.IsNullOrEmpty(d.Image2))
                        imagesURL.Add(d.Image2);
                    if (!string.IsNullOrEmpty(d.Image3))
                        imagesURL.Add(d.Image3);
                    if (!string.IsNullOrEmpty(d.Image4))
                        imagesURL.Add(d.Image4);
                    if (!string.IsNullOrEmpty(d.Image5))
                        imagesURL.Add(d.Image5);
                    if (!string.IsNullOrEmpty(d.Image6))
                        imagesURL.Add(d.Image6);
                    if (!string.IsNullOrEmpty(d.Image7))
                        imagesURL.Add(d.Image7);
                    if (!string.IsNullOrEmpty(d.Image8))
                        imagesURL.Add(d.Image8);
                    if (!string.IsNullOrEmpty(d.Image9))
                        imagesURL.Add(d.Image9);
                    if (!string.IsNullOrEmpty(d.Image10))
                        imagesURL.Add(d.Image10);
                    if (!string.IsNullOrEmpty(d.Image11))
                        imagesURL.Add(d.Image11);
                    if (!string.IsNullOrEmpty(d.Image12))
                        imagesURL.Add(d.Image12);

                    if (imagesURL.Count > 0 && updateImage)
                    {
                        //remove existing item images
                        if (itemObj.ItemImages.Count > 0)
                        {
                            var lstImageID = new List<int>();
                            foreach (var itemImage in itemObj.ItemImages)
                            {
                                lstImageID.Add(itemImage.ImageID);
                            }

                            foreach (var imageID in lstImageID)
                            {
                                var image = _imageService.GetImageByID(imageID);
                                _imageService.DeleteImage(image);
                            }
                        }

                        DirectoryInfo di = new DirectoryInfo(@"C:\Users\gdutj\Downloads\3rdStockSystem\SelloImage\" + d.SKU);
                        if (di.Exists)
                        {
                            var imageFiles = di.GetFiles("*", SearchOption.AllDirectories);
                            int j = 0;
                            foreach (var fi in imageFiles)
                            {
                                //Image img = Image.FromFile(imgFile);

                                using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(fi.FullName)))
                                {
                                    var fileName = d.SKU + "-" + j.ToString().PadLeft(2, '0') + ".jpg";
                                    var imgObj = _imageService.SaveImage(stream, fileName);
                                    itemObj.ItemImages.Add(new M_ItemImage()
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
                            using (var wc = new WebClient())
                            {
                                foreach (var imageURL in imagesURL)
                                {
                                    try
                                    {
                                        var imgBytes = wc.DownloadData(imageURL);
                                        using (var stream = new MemoryStream(imgBytes))
                                        {
                                            var fileName = d.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                                            var imgObj = _imageService.SaveImage(stream, fileName);
                                            itemObj.ItemImages.Add(new M_ItemImage()
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
                    }
                    if (existingItem == null)
                        _itemService.InsertItem(itemObj);
                    else
                        _itemService.UpdateItem(itemObj);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult BulkUpdateItem(ItemListViewModel.BulkUpdateItemModel bulkUpdate, string itemIdsBulkUpdate)
        {
            try
            {
                if (!string.IsNullOrEmpty(itemIdsBulkUpdate))
                {
                    var ids = itemIdsBulkUpdate
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();

                    var items = _itemService.GetItemsByIDs(ids.ToArray());
                    foreach (var item in items)
                    {
                        if (bulkUpdate.IsActive != -1)
                        {
                            item.IsActive = Convert.ToBoolean(bulkUpdate.IsActive);
                        }

                        if (bulkUpdate.IsReadyForList != -1)
                        {
                            item.IsReadyForList = Convert.ToBoolean(bulkUpdate.IsReadyForList);
                        }

                        _itemService.UpdateItem(item);
                    }
                }

                SuccessNotification("Bulk Update Item Success.");
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                LogManager.Instance.Error(exc.Message);
                ErrorNotification("Bulk Update Item Failed." + exc.Message);
                return RedirectToAction("List");
            }
        }

        private void FillDropDownDS(ItemViewModel model)
        {
            model.ItemTypes = ThirdStoreItemType.SINGLE.ToSelectList(false).ToList();
            model.Suppliers = ThirdStoreSupplier.P.ToSelectList(false).ToList();
            model.YesOrNo = YesNo.Y.ToSelectList(false).ToList();
        }
    }

    public class ImportItem
    {
        public string SKU { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public decimal GrossWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal CubicWeight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public int SupplierID { get; set; }
        public bool IsReadyForList { get; set; }
        public bool IsActive { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string Image4 { get; set; }
        public string Image5 { get; set; }
        public string Image6 { get; set; }
        public string Image7 { get; set; }
        public string Image8 { get; set; }
        public string Image9 { get; set; }
        public string Image10 { get; set; }
        public string Image11 { get; set; }
        public string Image12 { get; set; }
    }


    public class ImportNetoOZAuctionItem
    {
        public string SKU { get; set; }

        [CsvColumn(Name = "Name / Title")]
        public string Name { get; set; }

        [CsvColumn(Name = "Description html")]
        public string Description { get; set; }

        [CsvColumn(Name = "Price (A)")]
        public string Price { get; set; }

        [CsvColumn(Name = "Weight (Shipping)")]
        public decimal GrossWeight { get; set; }

        [CsvColumn(Name = "Length (Shipping)")]
        public decimal Length { get; set; }

        [CsvColumn(Name = "Width (Shipping)")]
        public decimal Width { get; set; }

        [CsvColumn(Name = "Height (Shipping)")]
        public decimal Height { get; set; }

        [CsvColumn(Name = "Product Category Full Path List")]
        public string CategoryPath { get; set; }

        [CsvColumn(Name = "Image URL")]
        public string Image1 { get; set; }

        [CsvColumn(Name = "Extra Image 01 URL")]
        public string Image2 { get; set; }

        [CsvColumn(Name = "Extra Image 02 URL")]
        public string Image3 { get; set; }

        [CsvColumn(Name = "Extra Image 03 URL")]
        public string Image4 { get; set; }

        [CsvColumn(Name = "Extra Image 04 URL")]
        public string Image5 { get; set; }

        [CsvColumn(Name = "Extra Image 05 URL")]
        public string Image6 { get; set; }
    }

    public class SelloItem
    {
        [CsvColumn(Name = "Item No.")]
        public string SKU { get; set; }
        [CsvColumn(Name = "Item Description")]
        public string Name { get; set; }
        [CsvColumn(Name = "Desc")]
        public string Description { get; set; }
        [CsvColumn(Name = "package content")]
        public string PackageContent { get; set; }
        [CsvColumn(Name = "Specification")]
        public string Specification { get; set; }
        [CsvColumn(Name = "Weight 1 - Purchasing Unit")]
        public decimal Weight { get; set; }
        [CsvColumn(Name = "Length 1 - Purchase Unit")]
        public decimal Length { get; set; }
        [CsvColumn(Name = "Width 1 - Purchasing Unit")]
        public decimal Width { get; set; }
        [CsvColumn(Name = "Height 1 - Purchasing Unit")]
        public decimal Height { get; set; }
        [CsvColumn(Name = "Image 1")]
        public string Image1 { get; set; }
        [CsvColumn(Name = "Image 2")]
        public string Image2 { get; set; }
        [CsvColumn(Name = "Image 3")]
        public string Image3 { get; set; }
        [CsvColumn(Name = "Image 4")]
        public string Image4 { get; set; }
        [CsvColumn(Name = "Image 5")]
        public string Image5 { get; set; }
        [CsvColumn(Name = "Image 6")]
        public string Image6 { get; set; }
        [CsvColumn(Name = "Image 7")]
        public string Image7 { get; set; }
        [CsvColumn(Name = "Image 8")]
        public string Image8 { get; set; }
    }
}