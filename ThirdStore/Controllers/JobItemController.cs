using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStoreFramework.Kendoui;
using ThirdStoreBusiness.JobItem;
using ThirdStore.Extensions;
using ThirdStoreFramework.Controllers;
using ThirdStoreCommon;
using ThirdStoreFramework.MVC;
using ThirdStore.Models.JobItem;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.Image;
using ThirdStoreCommon.Models.Image;
using ThirdStoreBusiness.ReportPrint;
using System.Collections;
using LINQtoCSV;
using ThirdStoreCommon.Models.JobItem;
using System.Net;
using ThirdStoreData;
using System.Globalization;
using System.IO;
using System.Drawing;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreCommon.Models;
using System.Text.RegularExpressions;
using ThirdStoreBusiness.AccessControl;

namespace ThirdStore.Controllers
{
    public class JobItemController : BaseController
    {
        private readonly IJobItemService _jobItemService;
        private readonly IItemService _itemService;
        private readonly IImageService _imageService;
        //private readonly IReportPrintService _reportPrintService;
        private readonly IDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IUserService _userService;
        public JobItemController(IJobItemService jobItemService,
            IItemService itemService,
            IImageService imageService,
            //IReportPrintService reportPrintService,
            IDbContext dbContext,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IUserService userService)
        {
            _jobItemService = jobItemService;
            _itemService = itemService;
            _imageService = imageService;
            //_reportPrintService = reportPrintService;
            _dbContext = dbContext;
            _workContext = workContext;
            _cacheManager = cacheManager;
            _userService = userService;
        }

        public ActionResult List()
        {
            //_jobItemService.GenerateProductFeed();
            var model = new JobItemListViewModel();
            model.JobItemTypes = ThirdStoreJobItemType.SELFSTORED.ToSelectList(false).ToList();
            model.JobItemTypes.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            model.Suppliers = ThirdStoreSupplier.P.ToSelectList(false).ToList();
            model.Suppliers.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            //model.JobItemConditions = ThirdStoreJobItemCondition.NEW.ToSelectList(false).ToList();
            model.JobItemConditions = _cacheManager.Get<IList<SelectOptionEntity>>(ThirdStoreCacheKey.ThirdStoreJobItemConditionListCache).ToSelectListByList().ToList();
            model.JobItemConditions.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            model.JobItemStatuses = ThirdStoreJobItemStatus.PENDING.ToSelectList(false).ToList();
            model.JobItemStatuses.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            model.InspectorList = new MultiSelectList(_userService.GetAllUsers().Where(u => !string.IsNullOrWhiteSpace(u.Description)).Select(u => new { ID = u.Description, Name = u.Description }), "ID", "Name").ToList();
            model.InspectorList.Insert(0, new SelectListItem { Text = "All", Value = "" });

            model.YesOrNo = YesNo.Y.ToSelectList(false).ToList();
            model.YesOrNo.Insert(0, new SelectListItem { Text = "", Value = "-1", Selected = true });
            model.HasStocktakeTime = -1;

            var showSyncInvUsers = new int[] {1,4, 14,17 };
            if (showSyncInvUsers.Contains(_workContext.CurrentUser.ID))
                model.ShowSyncInventory = true;

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, JobItemListViewModel model)
        {
            ThirdStoreJobItemStatus? jobItemStatus = model.SearchStatus > 0 ? (ThirdStoreJobItemStatus?)(model.SearchStatus) : null;
            ThirdStoreJobItemCondition? jobItemCondition = model.SearchCondition > 0 ? (ThirdStoreJobItemCondition?)(model.SearchCondition) : null;
            ThirdStoreJobItemType? jobItemType = model.SearchType > 0 ? (ThirdStoreJobItemType?)(model.SearchType) : null;
            ThirdStoreSupplier? supplier = model.SearchSupplier > 0 ? (ThirdStoreSupplier?)(model.SearchSupplier) : null;
            var inspector = model.SearchInspector!=null&& !model.SearchInspector.Contains("") ? model.SearchInspector : null;

            var jobItems = _jobItemService.SearchJobItems(
                id:model.SearchID,
                reference:model.SearchReference,
                sku:model.SearchSKU,
                jobItemCreateTimeFrom:model.CreateTimeFrom,
                jobItemCreateTimeTo:model.CreateTimeTo,
                jobItemType: jobItemType,
                jobItemStatus: jobItemStatus,
                jobItemCondition: jobItemCondition,
                jobItemSupplier:supplier,
                location:model.SearchLocation,
                inspector: inspector,
                trackingNumber:model.SearchTrackingNumber,
                hasStocktakeTime:model.HasStocktakeTime,
                isExcludeStatus:model.IsExcludeStatus,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var jobItemGridViewList = jobItems.Select(i => {
                var viewModel = i.ToModel();
                viewModel.Condition = _cacheManager.Get<IList<SelectOptionEntity>>(ThirdStoreCacheKey.ThirdStoreJobItemConditionListCache).FirstOrDefault(itm=>itm.ID.Equals(i.ConditionID)).Name;
                if (i.JobItemLines.Count > 0)
                    viewModel.SKUs = GetSKUsDetails(i);
                viewModel.Reference = _jobItemService.GetJobItemReference(i);
                return viewModel;
            } );

            var gridModel = new DataSourceResult() { Data = jobItemGridViewList, Total = jobItems.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Create(int fromJobItemID = 0)
        {
            var newJobItemViewModel = new JobItemViewModel();

            if(fromJobItemID!=0)
            {
                var jobItem = _jobItemService.GetJobItemByID(fromJobItemID);
                if(jobItem!=null)
                {
                    newJobItemViewModel = jobItem.ToCreateNewModel();

                    newJobItemViewModel.Reference = string.Empty;
                    newJobItemViewModel.ShipTime = null;
                    newJobItemViewModel.TrackingNumber = string.Empty;
                    newJobItemViewModel.StatusID = ThirdStoreJobItemStatus.PENDING.ToValue();
                }
            }

            FillDropDownDS(newJobItemViewModel);
            //newJobItemViewModel.JobItemCreateTime = DateTime.Now;

            return View(newJobItemViewModel);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-print", "isPrintLabel")]
        public ActionResult Create(JobItemViewModel model, bool isPrintLabel)
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
            if(!string.IsNullOrEmpty(newEntityModel.DesignatedSKU))
                newEntityModel.DesignatedSKU = newEntityModel.DesignatedSKU.Trim().ToUpper();
            if (model.JobItemViewLines != null && model.JobItemViewLines.Count > 0)
            {
                foreach (var lModel in model.JobItemViewLines)
                {
                    var newEntityLine = lModel.ToEntity().FillOutNull();
                    newEntityLine.ItemID = _itemService.GetItemBySKU(lModel.SKU).ID;
                    newEntityLine.SKU = newEntityLine.SKU.Trim().ToUpper();
                    newEntityModel.JobItemLines.Add(newEntityLine);
                }
            }

            if (model.JobItemViewImages != null && model.JobItemViewImages.Count > 0)
            {
                foreach (var lModel in model.JobItemViewImages)
                {
                    var newEntityLine = lModel.ToEntity().FillOutNull();
                    newEntityModel.JobItemImages.Add(newEntityLine);
                }
            }

            _jobItemService.InsertJobItem(newEntityModel);

            if (isPrintLabel)
            {
                _jobItemService.PrintJobItemLabel(new int[] { newEntityModel.ID });
            }

            SuccessNotification($"Job item {newEntityModel.ID} has been created.");
            return RedirectToAction("Edit",new { jobItemID = newEntityModel.ID });
        }

        public ActionResult Edit(int jobItemID)
        {
            var editItemViewModel = new JobItemViewModel();

            var jobItem = _jobItemService.GetJobItemByID(jobItemID);
            if (jobItem != null)
            {
                editItemViewModel = jobItem.ToCreateNewModel();
                editItemViewModel.Reference = _jobItemService.GetJobItemReference(jobItem);
                //editItemViewModel.Ref2 = jobItem.Ref2.ToCharArray().Select(c => c.ToString()).ToList();
            }

            FillDropDownDS(editItemViewModel);
            return View(editItemViewModel);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-print","isPrintLabel")]
        public ActionResult Edit(JobItemViewModel model,bool isPrintLabel)
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

            var editEntityModel = _jobItemService.GetJobItemByID(model.ID);
            editEntityModel = model.ToCreateNewEntity(editEntityModel).FillOutNull();
            if (!string.IsNullOrEmpty(editEntityModel.DesignatedSKU))
                editEntityModel.DesignatedSKU = editEntityModel.DesignatedSKU.Trim().ToUpper();
            //editEntityModel.D_Order_Line.Remove(editEntityModel.D_Order_Line.FirstOrDefault());
            //editEntityModel.D_Order_Line.Clear();
            //foreach (var removeLine in editEntityModel.D_Order_Line)
            //{ 

            //}
            if (model.JobItemViewLines != null && model.JobItemViewLines.Count > 0)
            {
                foreach (var lModel in model.JobItemViewLines)
                {
                    //if (lModel.ID == 0)
                    //{
                    //    var itemID = _itemService.GetItemBySKU(lModel.SKU).ID;
                    //    lModel.ItemID = itemID;
                    //}
                    var itemID = _itemService.GetItemBySKU(lModel.SKU).ID;
                    if (lModel.ID > 0)
                    {
                        var originLine = editEntityModel.JobItemLines.Where(l => l.ID == lModel.ID).FirstOrDefault();
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
                        editEntityModel.JobItemLines.Add(editEntityLine);
                    }
                }
            }


            if (model.JobItemViewImages != null && model.JobItemViewImages.Count > 0)
            {
                foreach (var lModel in model.JobItemViewImages)
                {
                    if (lModel.ID > 0)
                    {
                        var originLine = editEntityModel.JobItemImages.Where(l => l.ID == lModel.ID).FirstOrDefault();
                        if (originLine != null)
                        {
                            originLine = lModel.ToEntity(originLine).FillOutNull();
                        }
                    }
                    else
                    {
                        var editEntityLine = lModel.ToEntity().FillOutNull();
                        editEntityModel.JobItemImages.Add(editEntityLine);
                    }
                }
            }

            _jobItemService.UpdateJobItem(editEntityModel);

            if(isPrintLabel)
            {
               _jobItemService.PrintJobItemLabel( new int[] { editEntityModel.ID });
            }

            SuccessNotification($"Job item {editEntityModel.ID} has been updated.");
            return RedirectToAction("Edit", new { jobItemID = editEntityModel.ID });

        }


        public ActionResult ShipOut()
        {
            //var jobItemShipOutViewModel = new JobItemShipOutViewModel();

            return View();
        }

        [HttpPost]
        public ActionResult ShipOut(JobItemShipOutViewModel model)
        {
            try
            {
                //var jobItemShipOutViewModel = new JobItemShipOutViewModel();
                var returnMessage = _jobItemService.ShipOut(model.JobItemLineID, model.JobItemLineReference, model.TrackingNumber);
                if(returnMessage .IsSuccess)
                {
                    SuccessNotification(returnMessage.Mesage);
                }
                else
                {
                    ErrorNotification(returnMessage.Mesage);
                }
                return RedirectToAction("ShipOut");

            }
            catch(Exception ex)
            {
                ErrorNotification("Job item ship out failed. " + ex.Message);
                return RedirectToAction("ShipOut");
            }
        }


        public ActionResult StockTake(string location=null)
        {
            //var jobItemShipOutViewModel = new JobItemShipOutViewModel();
            var jobItemStockTakeViewModel = new JobItemStockTakeViewModel();
            if (!string.IsNullOrWhiteSpace(location))
            {
                jobItemStockTakeViewModel. Location = location.Trim();
            }

            return View(jobItemStockTakeViewModel);
        }

        [HttpPost]
        public ActionResult StockTake(JobItemStockTakeViewModel model)
        {
            try
            {
                var jobItemLineIDs =(model.JobItemLineID!=null? model.JobItemLineID.ToEnumerable().ToList():null) ;
                var jobItemLineRefs =(model.JobItemLineReference!=null? model.JobItemLineReference.ToEnumerable().ToList():null) ;
                //var jobItemShipOutViewModel = new JobItemShipOutViewModel();
                var returnMessage = _jobItemService.ConfirmStock(jobItemLineIDs, jobItemLineRefs,model.Location);
                if (returnMessage.IsSuccess)
                {
                    SuccessNotification(returnMessage.Mesage);
                }
                else
                {
                    ErrorNotification(returnMessage.Mesage);
                }
                return RedirectToAction("StockTake",new {Location= model.Location});

            }
            catch (Exception ex)
            {
                ErrorNotification("Job item confirm failed. " + ex.Message);
                return RedirectToAction("StockTake");
            }
        }

        [HttpPost]
        public ActionResult StocktakeFind(DataSourceRequest command, JobItemStockTakeViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.JobItemLineID) && string.IsNullOrWhiteSpace(model.JobItemLineReference))
                return new JsonResult { Data = new DataSourceResult() { Data = new List<JobItemGridViewModel>() , Total = 0 } };

            var jobItemLineID = (!string.IsNullOrWhiteSpace(model.JobItemLineID) ? Convert.ToInt32(model.JobItemLineID) : 0);
            var jobItems = _jobItemService.SearchJobItems(
                jobItemLineID: jobItemLineID,
                jobItemReference: model.JobItemLineReference,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var jobItemGridViewList = jobItems.Select(i => {
                var viewModel = i.ToModel();
                viewModel.Condition = _cacheManager.Get<IList<SelectOptionEntity>>(ThirdStoreCacheKey.ThirdStoreJobItemConditionListCache).FirstOrDefault(itm => itm.ID.Equals(i.ConditionID)).Name;
                if (i.JobItemLines.Count > 0)
                    viewModel.SKUs = GetSKUsDetails(i);
                viewModel.Reference = _jobItemService.GetJobItemReference(i);
                return viewModel;
            });

            var gridModel = new DataSourceResult() { Data = jobItemGridViewList, Total = jobItems.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };
        }



        [HttpPost]
        public ActionResult ReadJobItemLines(DataSourceRequest command, int jobItemID)
        {
            if (jobItemID > 0)
            {
                IList<JobItemViewModel.JobItemLineViewModel> jobItemLines = null;
                var jobItem = _jobItemService.GetJobItemByID(jobItemID);
                if (jobItem != null)
                {
                    jobItemLines = jobItem.JobItemLines.Select(r => r.ToModel()).ToList();
                }


                var gridModel = new DataSourceResult() { Data = jobItemLines, Total = jobItemLines.Count };


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
        public ActionResult CheckInputSKU(string inputSKU)
        {
            var item = _itemService.GetItemBySKU(inputSKU);
            if (item == null)
            {
                return Json(new { Result = false,ErrMessage="SKU Not Exists" });
            }
            else if(item.ChildItems.Count>1)
            {
                return Json(new { Result = false, ErrMessage = "SKU cannot contain more than 1 sub SKU." });
            }
            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult CheckDesignatedSKU(string designatedSKU, IList<JobItemViewModel.JobItemLineViewModel> jobItemViewLines)
        {
            var item = _itemService.GetItemBySKU(designatedSKU);
            if (item == null)
            {
                return Json(false);
            }
            return Json(true);
        }

        [HttpPost]
        public ActionResult ValidateInput(JobItemViewModel model)
        {
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
                        //var childItems = _itemService.GetItemsBySKUs(model.JobItemViewLines.Select(vl=>vl.SKU).ToList());
                        var grpViewItemLines = from vl in model.JobItemViewLines
                                               group vl by vl.SKU.ToLower() into grp
                                               select new {
                                                   SKU = grp.Key,
                                                   Qty = grp.Sum(l =>l.Qty)
                                               };

                        var leftJoinResult = from vl in grpViewItemLines
                                                 //join itm in childItems on vl.SKU.ToLower() equals itm.SKU.ToLower()
                                             join dc in designatedItem.ChildItems on new { SKU = vl.SKU.ToLower(), Qty = vl.Qty } equals new { SKU = dc.ChildItem.SKU.ToLower(), Qty = dc.ChildItemQty } into leftJoin
                                             from lj in leftJoin.DefaultIfEmpty()
                                             select lj;

                        var rightJoinResult = from dc in designatedItem.ChildItems
                                              join vl in grpViewItemLines on new { SKU = dc.ChildItem.SKU.ToLower(), Qty = dc.ChildItemQty } equals new { SKU = vl.SKU.ToLower(), Qty = vl.Qty } into rightJoin
                                              from rj in rightJoin.DefaultIfEmpty()
                                              select rj;

                        if(leftJoinResult.Any(lj=>lj==null)||rightJoinResult.Any(rj=>rj==null))
                        {
                            return Json(new { Result = false, Message = "The structure of the line item does not match designatedSKU." });
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
                if(model.JobItemViewLines.Count>1)
                {
                    return Json(new { Result = false, Message = "Job item can only contain one item." });
                }
            }

            if (!Regex.IsMatch(model.PricePercentage.ToString(), @"^[0-1]\.\d{1,2}$"))
            {
                return Json(new { Result = false, Message = "Percentage only can be decimal and 2 decimal places." });
            }

            if(model.Ref2==null||model.Ref2.Count==0)
            {
                return Json(new { Result = false, Message = "Please input as least one inspector." });
            }

            if (model.JobItemViewImages==null||model.JobItemViewImages.Count == 0)
            {
                return Json(new { Result = false, Message = "Please upload ast least one photo." });
            }

            //if(model.JobItemViewLines.Any(l=>l.Qty>1))
            return Json(new { Result=true});
        }

        //[HttpPost]
        //public ActionResult ChildItemCheck(ItemViewModel.ChildItemLineViewModel model)
        //{

        //    var item = _itemService.GetItemBySKU(model.ChildItemSKU);
        //    if (item == null)
        //    {
        //        var jsonResult = new DataSourceResult();
        //        //return jas("No item found with SKU");
        //        jsonResult.Errors = "No item found with SKU" ;
        //        return new JsonResult
        //        {
        //            Data = jsonResult
        //        };
        //    }
        //    model.ChildItemID = item.ID;//Todo: update new child item id to line in UI  
        //    //jsonResult.ExtraData=model;
        //    //return new JsonResult
        //    //    {
        //    //        Data = jsonResult
        //    //    };
        //    //return new NullJsonResult();
        //    return Json(new[] { model });
        //}

        [HttpPost]
        public ActionResult SyncInventory(JobItemListViewModel model)
        {
            try
            {
                //               var syncItemQuery = @"select I.ID
                //from D_Item I
                //where I.SKU in (select distinct SKU from OKSKUList20190830)
                //and Price<>0 and Description<>'' and Name<>'' and Type<>1 and LEN(SKU)<=23";
                //               var syncItemIDs = _dbContext.SqlQuery<int>(syncItemQuery).ToList();
                //               var retMessage = _jobItemService.SyncInventory(syncItemIDs);
                var retMessage = _jobItemService.SyncInventory(model.AffectTimeFrom, model.AffectTimeTo);

                if (retMessage.IsSuccess)
                    return Json(new { Result = true });
                else
                    return Json(new { Result = false, Message = retMessage.Mesage });
            }
            catch(Exception ex)
            {
                return Json(new { Result = false, ex.Message });
            }
            
        }

        [HttpPost]
        public ActionResult SyncByJobItem(string selectedIDs)
        {
            try
            {
                var ids = selectedIDs
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();

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
        public ActionResult JobItemLineDelete(JobItemViewModel.JobItemLineViewModel model)
        {
            if (model.ID > 0)
            {
                var jobItemLine = _jobItemService.GetJobItemLineByID(model.ID);
                if (jobItemLine != null)
                    _jobItemService.DeleteJobItemLine(jobItemLine);
            }

            return new NullJsonResult();
        }

        public ActionResult UploadImages(HttpPostedFileBase[] jobItemImages)
        {
            //if (Request.Files["JobItemImages"] != null)
            //{
            //    var images = Request.Files["JobItemImages"];

            //}

            var lstSavedImages = new List<JobItemViewModel.JobItemImageViewModel>();
            if (jobItemImages!=null)
            {
                var jobItemImagesOrdered = jobItemImages.OrderBy(img => img.FileName);
                foreach (var imgFile in jobItemImagesOrdered)
                {
                    var img=_imageService.SaveImage(imgFile.InputStream,imgFile.FileName);
                    var imgViewModel = new JobItemViewModel.JobItemImageViewModel() { ImageID=img.ID, ImageName=img.ImageName, ImageURL=_imageService.GetImageURL(img.ID) };
                    lstSavedImages.Add(imgViewModel);
                }
            }

            return Json(new {ImageList= lstSavedImages });
        }

        [HttpPost]
        public ActionResult ReadJobItemImages(DataSourceRequest command, int jobItemID)
        {
            if (jobItemID > 0)
            {
                IList<JobItemViewModel.JobItemImageViewModel> jobItemImages = null;
                var jobItem = _jobItemService.GetJobItemByID(jobItemID);
                if (jobItem != null)
                {
                    jobItemImages = jobItem.JobItemImages.Select(r =>
                    {
                        var viewModel = r.ToModel();
                        viewModel.ImageURL = _imageService.GetImageURL(r.ImageID);
                        viewModel.ImageName = r.Image.ImageName;
                        return viewModel;
                    }).ToList();
                }


                var gridModel = new DataSourceResult() { Data = jobItemImages, Total = jobItemImages.Count };


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
        public ActionResult JobItemImageDelete(JobItemViewModel.JobItemImageViewModel model)
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
        public ActionResult PrintLabel(string selectedIDs)
        {
            try
            {
                var ids = selectedIDs
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();

                _jobItemService.PrintJobItemLabel(ids);

                return Json(new { Result = true });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, ErrMsg = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult BulkUpdateJobItem(JobItemListViewModel.BulkUpdateJobItemModel bulkUpdate, string jobItemIdsBulkUpdate)
        {
            try
            {
                if (!string.IsNullOrEmpty(jobItemIdsBulkUpdate))
                {
                    var ids = jobItemIdsBulkUpdate
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();

                    var jobItems = _jobItemService.GetJobItemsByIDs(ids);
                    foreach (var jobItem in jobItems)
                    {
                        if (bulkUpdate.StatusID != 0)
                        {
                            jobItem.StatusID = bulkUpdate.StatusID;
                        }

                        if (!string.IsNullOrEmpty(bulkUpdate.Location))
                        {
                            jobItem.Location = bulkUpdate.Location.Trim();
                        }

                        if (bulkUpdate.ItemPrice>0)
                        {
                            jobItem.ItemPrice = bulkUpdate.ItemPrice;
                        }

                        if(bulkUpdate.PricePercentage>0)
                        {
                            jobItem.PricePercentage = bulkUpdate.PricePercentage;
                        }

                        _jobItemService.UpdateJobItem(jobItem);
                    }
                }

                SuccessNotification("Bulk Update Job Item Success.");
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                LogManager.Instance.Error(exc.Message);
                ErrorNotification("Bulk Update Job Item Failed." + exc.Message);
                return RedirectToAction("List");
            }
        }

        #region Private Methods

        private void FillDropDownDS(JobItemViewModel model)
        {
            model.JobItemTypes = ThirdStoreJobItemType.SELFSTORED.ToSelectList(false).ToList();
            //model.Suppliers = ThirdStoreSupplier.P.ToSelectList(false).ToList();
            model.JobItemStatuses = ThirdStoreJobItemStatus.PENDING.ToSelectList(false).ToList();
            model.JobItemConditions = _cacheManager.Get<IList<SelectOptionEntity>>(ThirdStoreCacheKey.ThirdStoreJobItemConditionListCache).ToSelectListByList().ToList();
            //model.InspectorList = _userService.GetAllUsers().Where(u=>!string.IsNullOrWhiteSpace( u.Description)).Select(u=>new {ID=u.Description, Name=u.Description }).ToSelectListByList().ToList();
            model.InspectorList=new MultiSelectList(_userService.GetAllUsers().Where(u => !string.IsNullOrWhiteSpace(u.Description)).Select(u => new { ID = u.Description, Name = u.Description }), "ID", "Name", model.Ref2).ToList();
            //var userList = _userService.GetAllUsers().Where(u => !string.IsNullOrWhiteSpace(u.Description)).Select(d=>d.Description);
            //var jobItemsInspectors = _jobItemService.GetAllJobItems().Select(ji=>ji.Ref2.ToUpper()).Where(r=>!string.IsNullOrWhiteSpace( r)).SelectMany(r=>r.ToCharArray().Select(c => c.ToString())).Distinct();
            //var result = jobItemsInspectors.Where(ins=>!userList.Contains(ins));
        }

        private string GetSKUsDetails(D_JobItem jobItem)
        {
            if (jobItem == null)
                return string.Empty;
            return (!string.IsNullOrWhiteSpace(jobItem.DesignatedSKU) ? jobItem.DesignatedSKU + ",0" : jobItem.JobItemLines.Select(l => l.SKU + "," + l.ItemID).Aggregate((current, next) => current + ";" + next));
        }

        #endregion

        #region Import Temp
        [HttpPost]
        public ActionResult ImportData()
        {
            try
            {
                var imageDI = @"C:\Users\gdutj\Downloads\3rdStockSystem\images";
                var csvContext = new CsvContext();
                var inputFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true };
                //var importData = csvContext.Read<JobItemImport>(@"C:\Users\gdutj\OneDrive\Document\Code\3rdStore\TODOList\File2\JobItem20190801.csv", inputFileDescription);
                var importData = csvContext.Read<JobItemImport>(@"C:\Users\gdutj\OneDrive\Document\Code\3rdStore\TODOList\FileNew20190820\ALL_Without_T_R.Jul.csv", inputFileDescription);
                var soldData = csvContext.Read<SoldJobItem>(@"C:\Users\gdutj\OneDrive\Document\Code\3rdStore\TODOList\SoldItem20190808.csv", inputFileDescription);
                var netoProducts = _dbContext.SqlQuery<NetoProduct>("select * from NetoProducts").ToList();
                var items = _itemService.GetAllItems();
                var grpImportData = from import in importData
                                    group import by new { import.JobItemCreateTime, import.Reference } into grp
                                    select grp;
                var existingJobItem = _jobItemService.GetAllJobItems();
                //var data = from import in importData
                //           join netoProduct in netoProducts on import.SKU.ToLower() equals netoProduct.SKU.ToLower()
                //           select new
                //           {
                //               import.SKU,
                //               ConditionID=import.ConditionID.ToUpper(),
                //               ItemName=netoProduct.Name,
                //               import.ItemDetail,
                //               ItemPrice=(import.ConditionID.ToUpper().Equals(ThirdStoreJobItemCondition.NEW.ToName())? netoProduct.DefaultPrice: import.ItemPrice),
                //               import.Location

                //           };


                foreach (var grp in grpImportData)
                {

                    var headerLine = grp.FirstOrDefault(l => !string.IsNullOrEmpty(l.JobItemCreateTime) && !string.IsNullOrEmpty(l.ConditionID));
                    if (headerLine != null)
                    {
                        try
                        {
                            var checkStr = (headerLine.OriginalReference.Trim().IndexOf("/") != -1 ? headerLine.OriginalReference.Trim().Substring(0, headerLine.OriginalReference.Trim().IndexOf("/")) : headerLine.OriginalReference.Trim()) + "-" + headerLine.SKU.Trim();
                            if (existingJobItem.FirstOrDefault(eji => eji.Ref5.Equals(checkStr)) != null)
                                continue;
                            //throw new Exception($"Job Item Reference: {headerLine.OriginalReference.Trim()} SKU {headerLine.SKU.Trim()} already exist");
                            var soldItem = soldData.FirstOrDefault(si => si.Reference.Trim().Equals(headerLine.OriginalReference.Trim()) && si.SKU.ToLower().Equals(headerLine.SKU.ToLower()));
                            if (soldItem != null)
                            {
                                LogManager.Instance.Error($"SKU {soldItem.SKU} Reference {soldItem.Reference} has been sold.");
                                continue;
                            }
                            var childLines = grp.Where(l => !l.Equals(headerLine));
                            var item = items.FirstOrDefault(itm => itm.SKU.ToLower().Equals(headerLine.SKU.ToLower()));
                            var netoProduct = netoProducts.FirstOrDefault(np => np.SKU.ToLower().Equals(headerLine.SKU.ToLower()));
                            if (item != null)
                            {
                                var isNew = headerLine.ConditionID.ToUpper().Equals(ThirdStoreJobItemCondition.NEW.ToName());
                                var newItem = new D_JobItem();
                                //newItem.JobItemCreateTime = DateTime.ParseExact(headerLine.JobItemCreateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                newItem.JobItemCreateTime = Convert.ToDateTime(headerLine.JobItemCreateTime);
                                newItem.Type = ThirdStoreJobItemType.SELFSTORED.ToValue();
                                newItem.StatusID = ThirdStoreJobItemStatus.PENDING.ToValue();
                                newItem.ConditionID = headerLine.ConditionID.ToUpper().ToEnumValue<ThirdStoreJobItemCondition>();
                                newItem.ItemName = (netoProduct != null ? netoProduct.Name : item.Name);
                                newItem.Note = headerLine.ItemDetail;
                                newItem.ItemPrice = Convert.ToDecimal((netoProduct != null ? netoProduct.DefaultPrice : headerLine.ItemPrice));
                                newItem.Location = headerLine.Location;
                                newItem.DesignatedSKU = (childLines != null && childLines.Count() > 0 ? headerLine.SKU : string.Empty);
                                newItem.Ref1 = headerLine.Reference.Trim();
                                newItem.Ref5 = (headerLine.OriginalReference.Trim().IndexOf("/") != -1 ? headerLine.OriginalReference.Trim().Substring(0, headerLine.OriginalReference.Trim().IndexOf("/")) : headerLine.OriginalReference.Trim()) + "-" + headerLine.SKU.Trim();
                                newItem.CreateTime = newItem.JobItemCreateTime;
                                newItem.EditTime = newItem.JobItemCreateTime;

                                if (childLines != null && childLines.Count() > 0)
                                {
                                    foreach (var line in childLines)
                                    {
                                        var lineItem = items.FirstOrDefault(itm => itm.SKU.ToLower().Equals(line.SKU.ToLower()));
                                        if (lineItem != null)
                                        {
                                            var newItemLine = new D_JobItemLine();
                                            newItemLine.SKU = line.SKU;
                                            newItemLine.ItemID = lineItem.ID;
                                            newItemLine.Qty = 1;
                                            newItemLine.Length = (string.IsNullOrEmpty(line.Length) ? lineItem.Length : Convert.ToDecimal(line.Length));
                                            newItemLine.Width = (string.IsNullOrEmpty(line.Width) ? lineItem.Width : Convert.ToDecimal(line.Width));
                                            newItemLine.Height = (string.IsNullOrEmpty(line.Height) ? lineItem.Height : Convert.ToDecimal(line.Height));
                                            newItemLine.Weight = (string.IsNullOrEmpty(line.Weight) ? lineItem.GrossWeight : Convert.ToDecimal(line.Weight));
                                            newItemLine.CreateTime = newItem.JobItemCreateTime;
                                            newItemLine.EditTime = newItem.JobItemCreateTime;

                                            newItem.JobItemLines.Add(newItemLine);
                                        }
                                        else
                                        {
                                            throw new Exception(line.SKU + " Job item line item info missed");
                                        }
                                    }
                                }
                                else
                                {
                                    var newItemLine = new D_JobItemLine();
                                    newItemLine.SKU = headerLine.SKU;
                                    newItemLine.ItemID = _itemService.GetItemBySKU(newItemLine.SKU).ID;
                                    newItemLine.Qty = 1;
                                    newItemLine.Length = Convert.ToDecimal((string.IsNullOrEmpty(headerLine.Length) ? item.Length.ToString() : headerLine.Length));
                                    newItemLine.Width = Convert.ToDecimal((string.IsNullOrEmpty(headerLine.Width) ? item.Width.ToString() : headerLine.Width));
                                    newItemLine.Height = Convert.ToDecimal((string.IsNullOrEmpty(headerLine.Height) ? item.Height.ToString() : headerLine.Height));
                                    newItemLine.Weight = Convert.ToDecimal((string.IsNullOrEmpty(headerLine.Weight) ? item.GrossWeight.ToString() : headerLine.Weight));
                                    newItemLine.CreateTime = newItem.JobItemCreateTime;
                                    newItemLine.EditTime = newItem.JobItemCreateTime;

                                    newItem.JobItemLines.Add(newItemLine);
                                }

                                if (!string.IsNullOrEmpty(headerLine.ImagePath))
                                {
                                    if (Directory.Exists(imageDI + "\\" + headerLine.ImagePath))
                                    {
                                        var imageFiles = Directory.GetFiles(imageDI + "\\" + headerLine.ImagePath, "*", SearchOption.AllDirectories);
                                        int j = 0;
                                        foreach (var imgFile in imageFiles)
                                        {
                                            //Image img = Image.FromFile(imgFile);

                                            using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(imgFile)))
                                            {
                                                var fileName = headerLine.SKU + "-" + newItem.JobItemCreateTime.ToString("ddMM") + headerLine.Reference + "-" + j.ToString().PadLeft(2, '0') + ".jpg";
                                                var imgObj = _imageService.SaveImage(stream, fileName);
                                                newItem.JobItemImages.Add(new M_JobItemImage()
                                                {
                                                    Image = imgObj,
                                                    DisplayOrder = j,
                                                    StatusID = 0,//TODO Get item active status id
                                                    CreateTime = newItem.CreateTime,
                                                    EditTime = newItem.EditTime
                                                });
                                            }
                                            j++;
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception(headerLine.SKU + " Image file path not exists");
                                    }
                                }
                                else if (!string.IsNullOrEmpty(headerLine.Image1))
                                {
                                    var imagesURL = new List<string>();
                                    if (!string.IsNullOrEmpty(headerLine.Image1))
                                        imagesURL.Add(headerLine.Image1);
                                    if (!string.IsNullOrEmpty(headerLine.Image2))
                                        imagesURL.Add(headerLine.Image2);
                                    if (!string.IsNullOrEmpty(headerLine.Image3))
                                        imagesURL.Add(headerLine.Image3);
                                    if (!string.IsNullOrEmpty(headerLine.Image4))
                                        imagesURL.Add(headerLine.Image4);
                                    if (!string.IsNullOrEmpty(headerLine.Image5))
                                        imagesURL.Add(headerLine.Image5);
                                    if (!string.IsNullOrEmpty(headerLine.Image6))
                                        imagesURL.Add(headerLine.Image6);

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
                                                    var fileName = headerLine.SKU + "-" + newItem.JobItemCreateTime.ToString("ddMM") + headerLine.Reference + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                                                    var imgObj = _imageService.SaveImage(stream, fileName);
                                                    newItem.JobItemImages.Add(new M_JobItemImage()
                                                    {
                                                        Image = imgObj,
                                                        DisplayOrder = i,
                                                        StatusID = 0,//TODO Get item active status id
                                                        CreateTime = newItem.CreateTime,
                                                        EditTime = newItem.EditTime
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

                                _jobItemService.InsertJobItem(newItem);
                                LogManager.Instance.Info($"Reference {headerLine.OriginalReference} SKU {headerLine.SKU} Supplier {headerLine.Supplier} import successfully.");
                            }
                            else
                            {
                                throw new Exception("Item or Neto Product Info Missed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Instance.Error($"Reference {headerLine.OriginalReference} SKU {headerLine.SKU} Supplier {headerLine.Supplier} import failed. " + ex.Message);
                        }

                    }


                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }


            return Json(new { Result = true });
        }

        #endregion

        #region Update DSZ and Sync Temp

        [HttpPost]
        public ActionResult UpdateDSZandSync()
        {
            try
            {
                var type2 = System.Type.GetType("ThirdStoreBusiness.ScheduleTask.SyncInventoryForLastDayTask, ThirdStoreBusiness");
                object instance;
                if (!ThirdStoreWebContext.Instance.TryResolve(type2, ThirdStoreWebContext.Instance.ContainerManager.Scope(), out instance))
                {
                    //not resolved
                    instance = ThirdStoreWebContext.Instance.ResolveUnregistered(type2, ThirdStoreWebContext.Instance.ContainerManager.Scope());
                }
                ThirdStoreBusiness.ScheduleTask.ITask task = instance as ThirdStoreBusiness.ScheduleTask.ITask;
                task.Execute();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }


            return Json(new { Result = true });
        }

        #endregion
    }

    #region Import Temp 2
    public class JobItemImport
    {
        [CsvColumn(Name = "OriginalReference")]
        public string OriginalReference { get; set; }

        [CsvColumn(Name = "reference")]
        public string Reference { get; set; }

        public string JobItemCreateTime{ get; set; }

        public string ConditionID { get; set; }

        public string SKU { get; set; }

        public string ItemName { get; set; }

        public string ItemDetail { get; set; }

        public string ItemPrice { get; set; }

        public string Length { get; set; }

        public string Width { get; set; }

        public string Height { get; set; }

        public string Weight { get; set; }

        public string Supplier { get; set; }

        [CsvColumn(Name = "imagepath")]
        public string ImagePath { get; set; }

        public string Location { get; set; }

        [CsvColumn(Name = "img1")]
        public string Image1 { get; set; }

        [CsvColumn(Name = "img2")]
        public string Image2 { get; set; }

        [CsvColumn(Name = "img3")]
        public string Image3 { get; set; }

        [CsvColumn(Name = "img4")]
        public string Image4 { get; set; }

        [CsvColumn(Name = "img5")]
        public string Image5 { get; set; }

        [CsvColumn(Name = "img6")]
        public string Image6 { get; set; }
    }

    public class NetoProduct
    {
        public string ID { get; set; }
        public string SKU { get; set; }
        public string DefaultPrice { get; set; }
        public string Name { get; set; }
        public string PrimarySupplier { get; set; }
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
        public string ShippingLength { get; set; }
        public string ShippingHeight { get; set; }
        public string ShippingWidth { get; set; }
        public string ShippingWeight { get; set; }
    }

    public class SoldJobItem
    {
        public string Reference { get; set; }
        public string SKU { get; set; }
    }

    #endregion

}