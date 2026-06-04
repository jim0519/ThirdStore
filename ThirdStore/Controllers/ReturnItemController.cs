using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStore.Extensions;
using ThirdStore.Models.Item;
using ThirdStore.Models.ReturnItem;
using ThirdStoreBusiness.Image;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.ReturnItem;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Image;
using ThirdStoreCommon.Models.ReturnItem;
using ThirdStoreFramework.Controllers;
using ThirdStoreFramework.Kendoui;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Controllers
{
    public class ReturnItemController : BaseController
    {
        private readonly IReturnItemService _returnItemService;
        private readonly IItemService _itemService;
        private readonly IImageService _imageService;
        public ReturnItemController(IReturnItemService returnItemService,
            IImageService imageService,
             IItemService itemService)
        {
            _returnItemService = returnItemService;
            _itemService = itemService;
            _imageService = imageService;
        }

        public ActionResult List()
        {
            var model = new ReturnItemListViewModel();

            model.ReturnItemTypes = ThirdStoreReturnItemType.Parts.ToSelectList(false).ToList();
            model.ReturnItemTypes.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, ReturnItemListViewModel model)
        {

            var returnItems = _returnItemService.SearchReturnItems(
                sku: model.SearchSKU,
                trackingNumber: model.SearchTrackingNumber,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var returnItemGridViewList = returnItems.Select(i => {
                var viewModel = i.ToModel();
                if (i.ReturnItemLines.Count > 0)
                    viewModel.SKUs = GetSKUsDetails(i);
                return viewModel;
            });

            var gridModel = new DataSourceResult() { Data = returnItemGridViewList, Total = returnItems.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        public ActionResult ReadReturnItemLines(DataSourceRequest command, int returnItemID)
        {
            if (returnItemID > 0)
            {
                IList<ReturnItemViewModel.ReturnItemLineViewModel> returnItemLines = null;
                var returnItem = _returnItemService.GetReturnItemByID(returnItemID);
                if (returnItem != null)
                {
                    returnItemLines = returnItem.ReturnItemLines.Select(r => r.ToModel()).ToList();
                }


                var gridModel = new DataSourceResult() { Data = returnItemLines, Total = returnItemLines.Count };


                //return View();
                return new JsonResult
                {
                    Data = gridModel
                };
            }
            else
                return Json(new object { });
        }

        public ActionResult Create()
        {
            var newReturnItemViewModel = new ReturnItemViewModel();

            FillDropDownDS(newReturnItemViewModel);

            return View(newReturnItemViewModel);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-print", "isPrintLabel")]
        public ActionResult Create(ReturnItemViewModel model, bool isPrintLabel)
        {
            FillDropDownDS(model);
            //checking
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Values.SelectMany(v => v.Errors.Select(er => er.ErrorMessage)).Aggregate((current, next) => current + Environment.NewLine + next);
                ErrorNotification(errMsg);
                return View(model);
            }

            var newEntityModel = model.ToCreateNewEntity();
            if (!string.IsNullOrEmpty(newEntityModel.DesignatedSKU))
                newEntityModel.DesignatedSKU = newEntityModel.DesignatedSKU.Trim().ToUpper();
            //if (!string.IsNullOrEmpty(newEntityModel.Location))
            //    newEntityModel.Location = newEntityModel.Location.Trim().ToUpper();
            if (model.ReturnItemViewLines != null && model.ReturnItemViewLines.Count > 0)
            {
                foreach (var lModel in model.ReturnItemViewLines)
                {
                    var newEntityLine = lModel.ToEntity().FillOutNull();
                    newEntityLine.ItemID = _itemService.GetItemBySKU(lModel.SKU).ID;
                    newEntityLine.SKU = newEntityLine.SKU.Trim().ToUpper();
                    newEntityModel.ReturnItemLines.Add(newEntityLine);
                }
            }

            if (model.ReturnItemViewImages != null && model.ReturnItemViewImages.Count > 0)
            {
                foreach (var lModel in model.ReturnItemViewImages)
                {
                    var newEntityLine = lModel.ToEntity().FillOutNull();
                    newEntityModel.ReturnItemImages.Add(newEntityLine);
                }
            }

            _returnItemService.InsertReturnItem(newEntityModel);

            //if (isPrintLabel)
            //{
            //    _returnItemService.PrintReturnItemLabel(new int[] { newEntityModel.ID });
            //}

            SuccessNotification($"Return item {newEntityModel.ID} has been created.");
            return RedirectToAction("Edit", new { returnItemID = newEntityModel.ID });
        }

        public ActionResult Edit(int returnItemID)
        {
            var editItemViewModel = new ReturnItemViewModel();

            var returnItem = _returnItemService.GetReturnItemByID(returnItemID);
            if (returnItem != null)
            {
                editItemViewModel = returnItem.ToCreateNewModel();
            }

            FillDropDownDS(editItemViewModel);
            return View(editItemViewModel);
        }


        [HttpPost]
        [ParameterBasedOnFormName("save-print", "isPrintLabel")]
        public ActionResult Edit(ReturnItemViewModel model, bool isPrintLabel)
        {
            FillDropDownDS(model);
            //checking
            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Values.SelectMany(v => v.Errors.Select(er => er.ErrorMessage)).Aggregate((current, next) => current + Environment.NewLine + next);
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

            var editEntityModel = _returnItemService.GetReturnItemByID(model.ID);
            editEntityModel = model.ToCreateNewEntity(editEntityModel).FillOutNull();
            if (!string.IsNullOrEmpty(editEntityModel.DesignatedSKU))
                editEntityModel.DesignatedSKU = editEntityModel.DesignatedSKU.Trim().ToUpper();
            //editEntityModel.D_Order_Line.Remove(editEntityModel.D_Order_Line.FirstOrDefault());
            //editEntityModel.D_Order_Line.Clear();
            //foreach (var removeLine in editEntityModel.D_Order_Line)
            //{ 

            //}
            if (model.ReturnItemViewLines != null && model.ReturnItemViewLines.Count > 0)
            {
                foreach (var lModel in model.ReturnItemViewLines)
                {
                    //if (lModel.ID == 0)
                    //{
                    //    var itemID = _itemService.GetItemBySKU(lModel.SKU).ID;
                    //    lModel.ItemID = itemID;
                    //}
                    var itemID = _itemService.GetItemBySKU(lModel.SKU).ID;
                    if (lModel.ID > 0)
                    {
                        var originLine = editEntityModel.ReturnItemLines.Where(l => l.ID == lModel.ID).FirstOrDefault();
                        if (originLine != null)
                        {
                            originLine = lModel.ToEntity(originLine).FillOutNull();
                            originLine.ItemID = itemID;
                            originLine.SKU = lModel.SKU.Trim().ToUpper();
                        }
                    }
                    else
                    {
                        var editEntityLine = lModel.ToEntity().FillOutNull();
                        editEntityLine.ItemID = itemID;
                        editEntityLine.SKU = lModel.SKU.Trim().ToUpper();
                        editEntityModel.ReturnItemLines.Add(editEntityLine);
                    }
                }
            }


            if (model.ReturnItemViewImages != null && model.ReturnItemViewImages.Count > 0)
            {
                foreach (var lModel in model.ReturnItemViewImages)
                {
                    if (lModel.ID > 0)
                    {
                        var originLine = editEntityModel.ReturnItemImages.Where(l => l.ID == lModel.ID).FirstOrDefault();
                        if (originLine != null)
                        {
                            originLine = lModel.ToEntity(originLine).FillOutNull();
                        }
                    }
                    else
                    {
                        var editEntityLine = lModel.ToEntity().FillOutNull();
                        editEntityModel.ReturnItemImages.Add(editEntityLine);
                    }
                }
            }

            _returnItemService.UpdateReturnItem(editEntityModel);

            //if (isPrintLabel)
            //{
            //    _returnItemService.PrintReturnItemLabel(new int[] { editEntityModel.ID });
            //}

            SuccessNotification($"Return item {editEntityModel.ID} has been updated.");
            return RedirectToAction("Edit", new { returnItemID = editEntityModel.ID });

        }

        [HttpPost]
        public ActionResult ReturnItemLineDelete(ReturnItemViewModel.ReturnItemLineViewModel model)
        {
            if (model.ID > 0)
            {
                var returnItemLine = _returnItemService.GetReturnItemLineByID(model.ID);
                if (returnItemLine != null)
                    _returnItemService.DeleteReturnItemLine(returnItemLine);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult FindReturnItemByTrackingNumber(string trackingNumber)
        {
            D_ReturnItem returnItem = null;
            var trackingNumberRule = _returnItemService.GetTrackingNumberRule(trackingNumber);
            if(trackingNumberRule!=null)
            {
                var realTrackingNumber = trackingNumber.Substring(trackingNumberRule.TrackingPrefixDigit, trackingNumberRule.TrackingMainDigit);
                returnItem = _returnItemService.FindByTrackingNumber(realTrackingNumber);
            }

            if (returnItem == null)
            {
                return Json(new { Result = false, ErrMessage = "Cannot match the return item or there are more than one return item matches the tracking number",SupplierID=(trackingNumberRule!=null? trackingNumberRule.SupplierID:0),CarrierName= (trackingNumberRule != null ? trackingNumberRule.CarrierName : string.Empty) });
            }
            else
            {
                //return RedirectToAction("Edit", new { returnItemID = returnItem.ID });
                SuccessNotification($"Return item {returnItem.ID } found.");
                return Json(new { Result = true, RedirectToUrl = Url.Action("Edit", new { returnItemID = returnItem.ID }) });
            }
            
        }


        [HttpPost]
        //[ParameterBasedOnFormName("save-print", "isPrintLabel")]
        public ActionResult ValidateInput(ReturnItemViewModel model, bool isPrintLabel)
        {

            if (string.IsNullOrWhiteSpace(model.Location))
                return Json(new { Result = false, Message = "Please make sure return item have valid location." });

            if (!string.IsNullOrEmpty(model.DesignatedSKU))
            {
                var designatedItem = _itemService.GetItemBySKU(model.DesignatedSKU);
                if (designatedItem != null)
                {
                    if (designatedItem.ChildItems.Count == 0)
                    {
                        return Json(new { Result = false, Message = "DesignatedSKU must contain child item." });
                    }
                    else
                    {
                        //var childItems = _itemService.GetItemsBySKUs(model.ReturnItemViewLines.Select(vl=>vl.SKU).ToList());
                        var grpViewItemLines = from vl in model.ReturnItemViewLines
                                               group vl by vl.SKU.ToUpper() into grp
                                               select new
                                               {
                                                   SKU = grp.Key,
                                                   Qty = grp.Count()
                                               };

                        var leftJoinResult = from vl in grpViewItemLines
                                                 //join itm in childItems on vl.SKU.ToUpper() equals itm.SKU.ToUpper()
                                             join dc in designatedItem.ChildItems on new { SKU = vl.SKU.ToUpper(), Qty = vl.Qty } equals new { SKU = dc.ChildItem.SKU.ToUpper(), Qty = dc.ChildItemQty } into leftJoin
                                             from lj in leftJoin.DefaultIfEmpty()
                                             select lj;

                        var rightJoinResult = from dc in designatedItem.ChildItems
                                              join vl in grpViewItemLines on new { SKU = dc.ChildItem.SKU.ToUpper(), Qty = dc.ChildItemQty } equals new { SKU = vl.SKU.ToUpper(), Qty = vl.Qty } into rightJoin
                                              from rj in rightJoin.DefaultIfEmpty()
                                              select rj;

                        if (leftJoinResult.Any(lj => lj == null) || rightJoinResult.Any(rj => rj == null))
                        {
                            if(model.ReturnTypeID==ThirdStoreReturnItemType.Full.ToValue())
                                return Json(new { Result = false, Message = "The status cannot be FULL as the item structure is not completed." });
                        }
                        else if(model.ReturnTypeID == ThirdStoreReturnItemType.Parts.ToValue())
                        {
                            return Json(new { Result = false, Message = "The status should be FULL as the item structure is completed." });
                        }

                    }

                }
                else
                {
                    return Json(new { Result = false, Message = "DesignatedSKU does not exist." });
                }
            }
            else
            {
                if (model.ReturnItemViewLines.Count > 1)
                {
                    return Json(new { Result = false, Message = "Return item can only contain one item." });
                }

                if (model.ReturnTypeID == ThirdStoreReturnItemType.Parts.ToValue())
                {
                    return Json(new { Result = false, Message = "The status should be FULL as the item structure is completed." });
                }
            }

            if (model.ReturnItemViewLines.Count > 0 )
            {
                if(model.ReturnItemViewLines.Any(l => l.Width.Equals(0) || l.Length.Equals(0) || l.Height.Equals(0) || l.Weight.Equals(0) || l.CubicWeight.Equals(0)))
                    return Json(new { Result = false, Message = "The length, width, height, weight and cubic weight must not be equal to 0." });
                
                
            }


            //if(model.ReturnItemViewLines.Any(l=>l.Qty>1))
            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult ValidateFullSet(IList<ReturnItemViewModel.ReturnItemLineViewModel> returnItemLines,string designatedSKU)
        {
            try
            {
                if (returnItemLines == null || returnItemLines.Count==0|| string.IsNullOrWhiteSpace(designatedSKU))
                    return Json(new { Result = true, Fullset = false });
                //else if(string.IsNullOrWhiteSpace(designatedSKU))
                var designatedItem = _itemService.GetItemBySKU(designatedSKU);
                if (designatedItem == null)
                    return Json(new { Result = true, Fullset = false });

                var grpViewItemLines = from vl in returnItemLines
                                       group vl by vl.SKU.ToUpper() into grp
                                       select new
                                       {
                                           SKU = grp.Key,
                                           Qty = grp.Count()
                                       };

                var leftJoinResult = from vl in grpViewItemLines
                                         //join itm in childItems on vl.SKU.ToUpper() equals itm.SKU.ToUpper()
                                     join dc in designatedItem.ChildItems on new { SKU = vl.SKU.ToUpper(), Qty = vl.Qty } equals new { SKU = dc.ChildItem.SKU.ToUpper(), Qty = dc.ChildItemQty } into leftJoin
                                     from lj in leftJoin.DefaultIfEmpty()
                                     select lj;

                var rightJoinResult = from dc in designatedItem.ChildItems
                                      join vl in grpViewItemLines on new { SKU = dc.ChildItem.SKU.ToUpper(), Qty = dc.ChildItemQty } equals new { SKU = vl.SKU.ToUpper(), Qty = vl.Qty } into rightJoin
                                      from rj in rightJoin.DefaultIfEmpty()
                                      select rj;

                if (leftJoinResult.Any(lj => lj == null) || rightJoinResult.Any(rj => rj == null))
                {
                    return Json(new { Result = true, Fullset = false });
                }
                else
                {
                    return Json(new { Result = true, Fullset = true });
                }
            }
            catch(Exception ex)
            {
                return Json(new { Result = false, ErrMessage=ex.Message });
            }

            
        }

        [HttpPost]
        public ActionResult UploadImages(HttpPostedFileBase[] returnItemImages)
        {

            var lstSavedImages = new List<ReturnItemViewModel.ReturnItemImageViewModel>();
            if (returnItemImages != null)
            {
                foreach (var imgFile in returnItemImages)
                {
                    var img = _imageService.SaveImage(imgFile.InputStream, imgFile.FileName);
                    var imgViewModel = new ReturnItemViewModel.ReturnItemImageViewModel() { ImageID = img.ID, ImageName = img.ImageName, ImageURL = _imageService.GetImageURL(img.ID) };
                    lstSavedImages.Add(imgViewModel);
                }
            }

            return Json(new { ImageList = lstSavedImages });
        }

        [HttpPost]
        public ActionResult ReadReturnItemImages(DataSourceRequest command, int returnItemID)
        {
            if (returnItemID > 0)
            {
                IList<ReturnItemViewModel.ReturnItemImageViewModel> returnItemImages = null;
                var returnItem = _returnItemService.GetReturnItemByID(returnItemID);
                if (returnItem != null)
                {
                    returnItemImages = returnItem.ReturnItemImages.Select(r =>
                    {
                        var viewModel = r.ToModel();
                        viewModel.ImageURL = _imageService.GetImageURL(r.ImageID);
                        viewModel.ImageName = r.Image.ImageName;
                        return viewModel;
                    }).ToList();
                }


                var gridModel = new DataSourceResult() { Data = returnItemImages, Total = returnItemImages.Count };


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
        public ActionResult ReturnItemImageDelete(ReturnItemViewModel.ReturnItemImageViewModel model)
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
        public ActionResult UploadReturnItem()
        {
            try
            {
                var files = Request.Files;
                HttpPostedFileBase zipFile = null;
                HttpPostedFileBase excelFile = null;

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];

                    if (file == null || file.ContentLength == 0)
                        continue;

                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (ext == ".zip")
                        zipFile = file;
                    else if (ext == ".xlsx" || ext == ".xls")
                        excelFile = file;
                }

                if (zipFile == null || excelFile == null)
                    throw new Exception("Please upload zip and excel together.");

                ProcessFiles(zipFile, excelFile);

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                LogManager.Instance.Error(exc.Message);
                ErrorNotification("Upload return item failed." + exc.Message);
                return RedirectToAction("List");
            }
        }

        private void ProcessFiles(HttpPostedFileBase zipFile, HttpPostedFileBase excelFile)
        {
            List<ReturnItemImportLine> returnItemImportLines;
            using (var excelStream = excelFile.InputStream)
            {
                returnItemImportLines = CommonFunc.ReadExcelToList<ReturnItemImportLine>(excelStream);
            }

            //check if import lines are valid
            var emptySKUs = returnItemImportLines.Where(r => string.IsNullOrWhiteSpace(r.Detected_SKU)).ToList();
            if(emptySKUs.Count>0)
            {
                var noSKURecordIDs = emptySKUs.Select(r => r.Record_ID).Aggregate((current, next) => current + "," + next);
                throw new Exception($"The following record IDs have no detected SKU: {noSKURecordIDs}");
            }

            var wrongSKUs= returnItemImportLines.Where(r => _itemService.GetItemBySKU(r.Detected_SKU)==null).ToList();
            if (wrongSKUs.Count > 0)
            {
                var wrongSKURecordIDs = wrongSKUs.Select(r => r.Record_ID).Aggregate((current, next) => current + "," + next);
                throw new Exception($"The following record IDs have no detected SKU: {wrongSKURecordIDs}");
            }

            List<D_Image> images;
            using (var zipStream = zipFile.InputStream)
            {
                images = ReadZipImages(zipStream);
            }

            foreach (var returnItemImportLine in returnItemImportLines)
            {
                //if (string.IsNullOrWhiteSpace(returnItemImportLine.Detected_SKU))
                //    continue;
                var item = _itemService.GetItemBySKU(returnItemImportLine.Detected_SKU.Trim());
                //if (item == null)
                //    continue;

                var newReturnItem = new D_ReturnItem();
                newReturnItem.Ref1= returnItemImportLine.Record_ID;
                newReturnItem.ReceivedDate = DateTime.ParseExact(returnItemImportLine.Received_Date,"M/d/yyyy", CultureInfo.InvariantCulture);
                newReturnItem.Location = returnItemImportLine.Location;
                newReturnItem.SupplierID = ThirdStoreConfig.Instance.SupplierIDMapping[returnItemImportLine.Supplier];
                newReturnItem.TrackingNumber= returnItemImportLine.Tracking;
                newReturnItem.ReturnTypeID = returnItemImportLine.Return_Type.ToEnumValue<ThirdStoreReturnItemType>();
                newReturnItem.NOP = returnItemImportLine.NOP == "Y" ? true : false;
                newReturnItem.Note= returnItemImportLine.Comment;
                newReturnItem.ProcessDate = !string.IsNullOrWhiteSpace( returnItemImportLine.Process_Date)? DateTime.ParseExact(returnItemImportLine.Process_Date, "M/d/yyyy", CultureInfo.InvariantCulture) : (DateTime?)null;
                newReturnItem.StatusID = 1;//Todo: need to confirm the status with business
                newReturnItem.CreateBy = returnItemImportLine.Created_By;
                newReturnItem.CreateTime = DateTime.Now;
                newReturnItem.EditBy = returnItemImportLine.Created_By;
                newReturnItem.EditTime = DateTime.Now;

                newReturnItem.FillOutNull();

                var newReturnItemLine = new D_ReturnItemLine();
                
                newReturnItemLine.SKU = item.SKU;
                newReturnItemLine.ItemID = item.ID;
                newReturnItemLine.Qty = 1;
                newReturnItemLine.Weight = item.GrossWeight;
                newReturnItemLine.Length = item.Length;
                newReturnItemLine.Width = item.Width;
                newReturnItemLine.Height = item.Height;
                newReturnItemLine.CubicWeight = item.Length * item.Width * item.Height * 250;
                newReturnItemLine.Ref1 = (item.Length * item.Width * item.Height).ToString();
                newReturnItemLine.CreateBy = returnItemImportLine.Created_By;
                newReturnItemLine.CreateTime = DateTime.Now;
                newReturnItemLine.EditBy = returnItemImportLine.Created_By;
                newReturnItemLine.EditTime = DateTime.Now;

                newReturnItemLine.FillOutNull();
                newReturnItem.ReturnItemLines.Add(newReturnItemLine);

                var matchImages = images.Where(x => x.ImageName.StartsWith(returnItemImportLine.Record_ID));
                if (matchImages != null)
                {
                    newReturnItem.ReturnItemImages = matchImages.Select(img => new M_ReturnItemImage()
                    {
                        ImageID = img.ID,
                        CreateBy = returnItemImportLine.Created_By,
                        CreateTime = DateTime.Now,
                        EditBy = returnItemImportLine.Created_By,
                        EditTime = DateTime.Now
                    }).ToList();
                }


                _returnItemService.InsertReturnItem(newReturnItem);


                
            }
        }

        private List<D_Image> ReadZipImages(Stream zipStream)
        {
            var list = new List<D_Image>();

            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    var ext = Path.GetExtension(entry.Name).ToLower();
                    if (!(ext == ".jpg" || ext == ".png" || ext == ".jpeg"))
                        continue;

                    var savePath = Path.Combine(ThirdStoreConfig.Instance.ThirdStoreImagesPath, entry.Name);

                    using (var entryStream = entry.Open())
                    {
                        var img = _imageService.SaveImage(entryStream, entry.Name);
                        list.Add(img);
                    }
                }
            }

            return list;
        }

        private void FillDropDownDS(ReturnItemViewModel model)
        {
            model.ReturnItemStatuses = ThirdStoreReturnItemStatus.Received.ToSelectList(false).ToList();
            model.Suppliers=ThirdStoreSupplier.T.ToSelectList(false).ToList();
            //model.Couriers = ThirdStoreCouriers.AustraliaPost.ToSelectList(false).ToList();
            model.ReturnTypes= ThirdStoreReturnItemType.Full.ToSelectList(false).ToList();
        }

        private string GetSKUsDetails(D_ReturnItem returnItem)
        {
            if (returnItem == null)
                return string.Empty;
            return (!string.IsNullOrWhiteSpace(returnItem.DesignatedSKU) ? returnItem.DesignatedSKU + ",0" : returnItem.ReturnItemLines.Select(l => l.SKU + "," + l.ItemID).Aggregate((current, next) => current + ";" + next));
        }
    }
}