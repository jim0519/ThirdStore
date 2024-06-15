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
using ThirdStoreBusiness.ReturnItem;
using ThirdStoreBusiness.Setting;
using ThirdStore.Models.Item;
using ThirdStoreBusiness.Attachment;

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
        private readonly IPermissionService _permissionService;
        private readonly IReturnItemService _returnItemService;
        private readonly ISettingService _settingService;
        private readonly CommonSettings _commonSetting;
        private readonly IAttachmentService _attachmentService;

        public JobItemController(IJobItemService jobItemService,
            IItemService itemService,
            IImageService imageService,
            //IReportPrintService reportPrintService,
            IDbContext dbContext,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IPermissionService permissionService,
            IUserService userService,
            IReturnItemService returnItemService,
            ISettingService settingService,
            IAttachmentService attachmentService)
        {
            _jobItemService = jobItemService;
            _itemService = itemService;
            _imageService = imageService;
            //_reportPrintService = reportPrintService;
            _dbContext = dbContext;
            _workContext = workContext;
            _cacheManager = cacheManager;
            _userService = userService;
            _permissionService = permissionService;
            _returnItemService = returnItemService;
            _settingService = settingService;
            _commonSetting = _settingService.LoadSetting<CommonSettings>();
            _attachmentService = attachmentService;
        }

        public ActionResult List(JobItemListViewModel model)
        {
            //_jobItemService.GenerateProductFeed();
            //JobItemListViewModel model;
            //if (m == null||string.IsNullOrWhiteSpace( m.SearchSKU))
            //    model = new JobItemListViewModel();
            //else
            //    model = m;
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
            
            model.ReviewStatuses=ThirdStoreReviewStatus.PendingReview.ToSelectList(false).ToList();
            model.ReviewStatuses.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            //var showSyncInvUsers = new int[] { 1, 4, 10, 14, 16, 17 };
            //if (showSyncInvUsers.Contains(_workContext.CurrentUser.ID))
            //    model.ShowSyncInventory = true;
            model.ShowSyncInventory = _permissionService.Authorize(ThirdStorePermission.JobItemSync.ToName());

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, JobItemListViewModel model)
        {
            ThirdStoreJobItemStatus? jobItemStatus = model.SearchStatus > 0 ? (ThirdStoreJobItemStatus?)(model.SearchStatus) : null;
            ThirdStoreJobItemCondition? jobItemCondition = model.SearchCondition > 0 ? (ThirdStoreJobItemCondition?)(model.SearchCondition) : null;
            ThirdStoreJobItemType? jobItemType = model.SearchType > 0 ? (ThirdStoreJobItemType?)(model.SearchType) : null;
            ThirdStoreSupplier? supplier = model.SearchSupplier > 0 ? (ThirdStoreSupplier?)(model.SearchSupplier) : null;
            ThirdStoreReviewStatus? reviewStatus = model.ReviewStatus > 0 ? (ThirdStoreReviewStatus?)(model.ReviewStatus) : null;
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
                shipTimeFrom:model.ShipTimeFrom,
                shipTimeTo:model.ShipTimeTo,
                hasStocktakeTime:model.HasStocktakeTime,
                reviewStatus: reviewStatus,
                isExcludeShippedStatus:model.IsExcludeShippedStatus,
                isBulkQtyItem:model.IsBulkQtyItem,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var jobItemGridViewList = jobItems.Select(i => {
                var viewModel = i.ToModel();
                viewModel.Condition = _cacheManager.Get<IList<SelectOptionEntity>>(ThirdStoreCacheKey.ThirdStoreJobItemConditionListCache).FirstOrDefault(itm=>itm.ID.Equals(i.ConditionID)).Name;
                if (i.JobItemLines.Count > 0)
                    viewModel.SKUs = GetSKUsDetails(i);
                viewModel.Reference = _jobItemService.GetJobItemReference(i);
                viewModel.CBM = i.JobItemLines.Sum(l =>Convert.ToDecimal( l.Ref1));
                return viewModel;
            } );

            var gridModel = new DataSourceResult() { Data = jobItemGridViewList, Total = jobItems.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Create(int fromJobItemID = 0,int fromReturnItemID=0)
        {
            var newJobItemViewModel = new JobItemViewModel();
            newJobItemViewModel.Qty = -1;

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
                    newJobItemViewModel.Note += " From job item id " + fromJobItemID;
                    //newJobItemViewModel.JobItemViewImages.Clear();
                    //foreach (var oriImg in jobItem.JobItemImages)
                    //{
                    //    var img = _imageService.DuplicateImageByID(oriImg.ImageID);
                    //    var imgViewModel = new JobItemViewModel.JobItemImageViewModel() { ImageID = img.ID, ImageName = img.ImageName, ImageURL = _imageService.GetImageURL(img.ID) };
                    //    newJobItemViewModel.JobItemViewImages.Add(imgViewModel);
                    //}
                }
            }
            else if(fromReturnItemID!=0)
            {
                var returnItem = _returnItemService.GetReturnItemByID(fromReturnItemID);
                if (returnItem != null)
                {
                    //newJobItemViewModel.Reference = string.Empty;
                    //newJobItemViewModel.ShipTime = null;
                    //newJobItemViewModel.TrackingNumber = string.Empty;
                    newJobItemViewModel.Type = ThirdStoreJobItemType.SELFSTORED.ToValue();
                    newJobItemViewModel.ConditionID = ThirdStoreJobItemCondition.NEW.ToValue();
                    newJobItemViewModel.StatusID = ThirdStoreJobItemStatus.PENDING.ToValue();
                    newJobItemViewModel.DesignatedSKU = returnItem.DesignatedSKU;
                    newJobItemViewModel.Note = "From return item id " + fromReturnItemID;
                    //newJobItemViewModel.JobItemViewImages.Clear();
                    //foreach (var returnItemLine in returnItem.ReturnItemLines)
                    //{
                    //    var jobItemLineViewModel = new JobItemViewModel.JobItemLineViewModel();
                    //    jobItemLineViewModel.ItemID = returnItemLine.ItemID;
                    //    jobItemLineViewModel.SKU = returnItemLine.SKU;
                    //    jobItemLineViewModel.Qty = returnItemLine.Qty;
                    //    jobItemLineViewModel.Weight = returnItemLine.Weight;
                    //    jobItemLineViewModel.Length = returnItemLine.Length;
                    //    jobItemLineViewModel.Width = returnItemLine.Width;
                    //    jobItemLineViewModel.Height = returnItemLine.Height;
                    //    jobItemLineViewModel.CubicWeight = returnItemLine.CubicWeight;
                    //    jobItemLineViewModel.Ref1 = returnItemLine.Ref1;

                    //    newJobItemViewModel.JobItemViewLines.Add(jobItemLineViewModel);
                    //}
                }
            }
            //else
            //{

            //    var templates = _commonSetting.JobItemNoteAutoFillTemplates.Split(new[] { "|-|" }, StringSplitOptions.None);
            //    if (templates.Length > 0)
            //    {
            //        var rnd = new Random();
            //        var prefillNote = templates[rnd.Next(0, templates.Length - 1)];
            //        newJobItemViewModel.Note = prefillNote;
            //    }
            //}
           

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
            if (!string.IsNullOrEmpty(newEntityModel.Location))
                newEntityModel.Location = newEntityModel.Location.Trim().ToUpper();
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

            if (model.JobItemViewAttachments != null && model.JobItemViewAttachments.Count > 0)
            {
                foreach (var lModel in model.JobItemViewAttachments)
                {
                    var newEntityLine = lModel.ToEntity().FillOutNull();
                    newEntityModel.JobItemAttachments.Add(newEntityLine);
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
            //checking
            if(!editEntityModel.EditTime.TrimMilliseconds().Equals(model.EditTime.TrimMilliseconds()))
            {
                var errMsg = "Job item was edited by someone else, please try again.";
                ErrorNotification(errMsg);
                return RedirectToAction("Edit", new { jobItemID=editEntityModel.ID });
            }
            
            var preLocation = editEntityModel.Location;
            editEntityModel = model.ToCreateNewEntity(editEntityModel).FillOutNull();
            if (!string.IsNullOrEmpty(editEntityModel.DesignatedSKU))
                editEntityModel.DesignatedSKU = editEntityModel.DesignatedSKU.Trim().ToUpper();
            if (!string.IsNullOrEmpty(editEntityModel.Location))
            {
                editEntityModel.Ref4 = preLocation;
                editEntityModel.Location = editEntityModel.Location.Trim().ToUpper();
            }
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

            if (model.JobItemViewAttachments != null && model.JobItemViewAttachments.Count > 0)
            {
                foreach (var lModel in model.JobItemViewAttachments)
                {
                    if (lModel.ID > 0)
                    {
                        var originLine = editEntityModel.JobItemAttachments.Where(l => l.ID == lModel.ID).FirstOrDefault();
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
                        editEntityModel.JobItemAttachments.Add(editEntityLine);
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
            if (string.IsNullOrWhiteSpace(model.JobItemLineID) 
                && string.IsNullOrWhiteSpace(model.JobItemLineReference)
                &&!model.StocktakeTimeFrom.HasValue
                &&!model.StocktakeTimeTo.HasValue)
                return new JsonResult { Data = new DataSourceResult() { Data = new List<JobItemGridViewModel>(), Total = 0 } };

            var jobItemLineID = (!string.IsNullOrWhiteSpace(model.JobItemLineID) ? Convert.ToInt32(model.JobItemLineID) : 0);
            var jobItems = _jobItemService.SearchJobItems(
                jobItemLineID: jobItemLineID,
                jobItemReference: model.JobItemLineReference,
                stocktakeTimeFrom: model.StocktakeTimeFrom,
                stocktakeTimeTo: model.StocktakeTimeTo,
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
        public ActionResult ReadJobItemLines(DataSourceRequest command, int jobItemID, int fromReturnItemID=0)
        {
            var jobItemLines = new List<JobItemViewModel.JobItemLineViewModel>();
            if (jobItemID > 0)
            {
                
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
            else if(fromReturnItemID>0)
            {
                var returnItem = _returnItemService.GetReturnItemByID(fromReturnItemID);
                if (returnItem != null)
                {
                    foreach (var returnItemLine in returnItem.ReturnItemLines)
                    {
                        var jobItemLineViewModel = new JobItemViewModel.JobItemLineViewModel();
                        jobItemLineViewModel.ItemID = returnItemLine.ItemID;
                        jobItemLineViewModel.SKU = returnItemLine.SKU;
                        jobItemLineViewModel.Qty = returnItemLine.Qty;
                        jobItemLineViewModel.Weight = returnItemLine.Weight;
                        jobItemLineViewModel.Length = returnItemLine.Length;
                        jobItemLineViewModel.Width = returnItemLine.Width;
                        jobItemLineViewModel.Height = returnItemLine.Height;
                        jobItemLineViewModel.CubicWeight = returnItemLine.CubicWeight;
                        jobItemLineViewModel.Ref1 = returnItemLine.Ref1;

                        jobItemLines.Add(jobItemLineViewModel);
                    }
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
        //[ParameterBasedOnFormName("save-print", "isPrintLabel")]
        public ActionResult ValidateInput(JobItemViewModel model, bool isPrintLabel)
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

                    if(model.ItemPrice>0&& model.ItemPrice< designatedItem.Cost*Convert.ToDecimal( 0.7))
                    {
                        return Json(new { Result = false, Message = "Item price cannot be smaller than the reasonal amount." });
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

            if(model.JobItemViewLines.Count>0&&model.JobItemViewLines.Any(l=>l.Width.Equals(0)||l.Length.Equals(0)||l.Height.Equals(0)||l.Weight.Equals(0)||l.CubicWeight.Equals(0)))
            {
                return Json(new { Result = false, Message = "The length, width, height, weight and cubic weight must not be equal to 0." });
            }

            if (!Regex.IsMatch(model.PricePercentage.ToString(), @"^[0-1]\.\d{1,2}$"))
            {
                return Json(new { Result = false, Message = "Percentage only can be decimal and 2 decimal places." });
            }

            if(model.Ref2==null||model.Ref2.Count==0)
            {
                return Json(new { Result = false, Message = "Please input as least one inspector." });
            }

            if(string.IsNullOrWhiteSpace( model.Location))
            {
                return Json(new { Result = false, Message = "Please input the item location." });
            }

            //if (isPrintLabel)
            //{
            //    if (model.JobItemViewImages == null || model.JobItemViewImages.Count < 6)
            //    {
            //        return Json(new { Result = false, Message = "Please upload ast least 6 photos." });
            //    }
            //}

            //if(model.JobItemViewLines.Any(l=>l.Qty>1))
            return Json(new { Result=true});
        }


        [HttpPost]
        public ActionResult BulkEditValidate(JobItemListViewModel.BulkUpdateJobItemModel bulkUpdate, string jobItemIdsBulkUpdate)
        {
            try
            {
                if (bulkUpdate.ItemPrice != 0)
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
                            if (!string.IsNullOrWhiteSpace(jobItem.DesignatedSKU))
                            {
                                var item = _itemService.GetItemBySKU(jobItem.DesignatedSKU);
                                if (bulkUpdate.ItemPrice < item.Cost * Convert.ToDecimal(0.85))
                                {
                                    throw new Exception($"{item.SKU } Item price is set to be smaller than the reasonal amount.");
                                }
                            }
                            else
                            {
                                var item = _itemService.GetItemByID(jobItem.JobItemLines.FirstOrDefault().ItemID);
                                if (bulkUpdate.ItemPrice < item.Cost * Convert.ToDecimal(0.85))
                                {
                                    throw new Exception($"{item.SKU } Item price is set to be smaller than the reasonal amount");
                                }
                            }
                        }
                    }
                }

                return Json(new { Result = true });

            }
            catch (Exception ex)
            {
                return Json(new { Result = false, ErrMsg = ex.Message });
            }
        }


        [HttpPost]
        public ActionResult ExportGumtreeFeed(string jobItemIDs)
        {
            try
            {
                //var csvContent = orderList.Aggregate((current, next) => current + "," + next);
                byte[] bytes = null;
                if (jobItemIDs != null)
                {
                    var ids = jobItemIDs
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Convert.ToInt32(x))
                        .ToArray();
                    using (var stream = _jobItemService.ExportGumtreeFeed(ids))
                    {
                        bytes = ((MemoryStream)stream).ToArray();
                    }
                }

                var fileName = CommonFunc.ToCSVFileName("ExportGumtreeFeed");


                if (!Directory.Exists(ThirdStoreConfig.Instance.GumtreeFeedPath))
                    Directory.CreateDirectory(ThirdStoreConfig.Instance.GumtreeFeedPath);
                System.IO.File.WriteAllBytes($"{ThirdStoreConfig.Instance.GumtreeFeedPath}/{fileName}", bytes);

                //return File(outputStream, "application/zip", "filename.zip");

                return File(bytes, "text/csv, application/zip", fileName);
            }
            catch (Exception exc)
            {
                LogManager.Instance.Error(exc.Message);
                ErrorNotification("Export Order Failed." + exc.Message);
                return RedirectToAction("List");
            }
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
                    return Json(new { Result = false, ErrMsg = retMessage.Mesage });
            }
            catch(Exception ex)
            {
                return Json(new { Result = false, ErrMsg=ex.Message });
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
        public ActionResult ReadJobItemImages(DataSourceRequest command, int jobItemID, int fromJobItemID = 0)
        {
            IList<JobItemViewModel.JobItemImageViewModel> jobItemImages = null;
            if (fromJobItemID>0)
            {
                var jobItem = _jobItemService.GetJobItemByID(fromJobItemID);
                if (jobItem != null)
                {
                    jobItemImages = jobItem.JobItemImages.Select(r =>
                    {
                        var img = _imageService.DuplicateImageByID(r.ImageID);
                        return new JobItemViewModel.JobItemImageViewModel() { ImageID = img.ID, ImageName = img.ImageName, ImageURL = _imageService.GetImageURL(img.ID), StatusID=Convert.ToBoolean( r.StatusID), DisplayOrder=r.DisplayOrder };
                    }).ToList();
                }


                var gridModel = new DataSourceResult() { Data = jobItemImages, Total = jobItemImages.Count };


                //return View();
                return new JsonResult
                {
                    Data = gridModel
                };
            }
            else if (jobItemID > 0)
            {

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

                        if (bulkUpdate.Type != 0)
                        {
                            jobItem.Type = bulkUpdate.Type;
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

                //SuccessNotification("Bulk Update Job Item Success.");
                //return RedirectToAction("List");

                return Json(new { Result = true });
            }
            catch (Exception exc)
            {
                LogManager.Instance.Error(exc.Message);
                //ErrorNotification("Bulk Update Job Item Failed." + exc.Message);
                //return RedirectToAction("List");
                return Json(new { Result = false, ErrMsg = exc.Message });
            }
        }

        [HttpPost]
        public ActionResult MatchJobItemVerifyTracking(JobItemShipOutViewModel model)
        {
            try
            {
                var result= _jobItemService.MatchJobItemVerifyTracking(model.JobItemLineID,model.JobItemLineReference,model.TrackingNumber);
                if(result.IsSuccess)
                    return Json(new { Result = true, Message=result.Mesage, LocatedJobItemID = result.Entity.ID });
                else
                    return Json(new { Result = false, Message = result.Mesage, LocatedJobItemID = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, ErrMsg = ex.Message });
            }
        }


        [HttpPost]
        public ActionResult Prefill(string selectedIDs)
        {
            try
            {
                var ids = selectedIDs
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();

                var jobitems = _jobItemService.GetJobItemsByIDs(ids);
                var templates = _commonSetting.JobItemNoteAutoFillTemplates.Split(new[] { "|-|" }, StringSplitOptions.None);
                var rnd = new Random();
                foreach (var jobitem in jobitems)
                {
                    var sku = (!string.IsNullOrEmpty(jobitem.DesignatedSKU) ? jobitem.DesignatedSKU : jobitem.JobItemLines.FirstOrDefault().SKU);
                    var item = _itemService.GetItemBySKU(sku);
                    if (item.Cost != 0&& jobitem.ItemPrice==0)
                        jobitem.ItemPrice =Math.Round( item.Cost * 11 / 9,2);
                    
                    if (templates.Length > 0&& string.IsNullOrEmpty( jobitem.Note))
                    {
                        var prefillNote = templates[rnd.Next(0, templates.Length - 1)];
                        jobitem.Note = prefillNote;
                    }

                    _jobItemService.UpdateJobItem(jobitem);
                }


                return Json(new { Result = true });
                
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, ErrMsg = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult UploadAttachments(HttpPostedFileBase[] attachments, string notes)
        {
            var lstSavedAttachments = new List<JobItemViewModel.JobItemAttachmentViewModel>();
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var savedAttachment = _attachmentService.SaveAttachment(attachment.InputStream, attachment.FileName);
                    var jobItemAttachmentViewModel = new JobItemViewModel.JobItemAttachmentViewModel() { AttachmentID = savedAttachment.ID, AttachmentName = savedAttachment.Name, AttachmentURL = _attachmentService.GetAttachmentURL(savedAttachment.ID), Notes = notes };
                    lstSavedAttachments.Add(jobItemAttachmentViewModel);
                }
            }

            return Json(new { AttachmentList = lstSavedAttachments });
        }


        [HttpPost]
        public ActionResult ReadJobItemAttachments(DataSourceRequest command, int jobItemID)
        {
            if (jobItemID > 0)
            {
                IList<JobItemViewModel.JobItemAttachmentViewModel> jobItemAttachments = null;
                var jobItem = _jobItemService.GetJobItemByID(jobItemID);
                if (jobItem != null)
                {
                    jobItemAttachments = jobItem.JobItemAttachments.Select(r =>
                    {
                        var viewModel = r.ToModel();
                        viewModel.AttachmentURL = _attachmentService.GetAttachmentURL(r.AttachmentID);
                        viewModel.AttachmentName = r.Attachment.Name;
                        return viewModel;
                    }).ToList();
                }


                var gridModel = new DataSourceResult() { Data = jobItemAttachments, Total = jobItemAttachments.Count };


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
        public ActionResult JobItemAttachmentDelete(JobItemViewModel.JobItemAttachmentViewModel model)
        {
            if (model.ID > 0)
            {
                var attachment = _attachmentService.GetAttachmentByID(model.AttachmentID);
                if (attachment != null)
                    _attachmentService.DeleteAttachment(attachment);
            }

            return new NullJsonResult();
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
            model.ReviewStatuses = ThirdStoreReviewStatus.PendingReview.ToSelectList(false).ToList();
            model.ReviewStatuses.Insert(0, new SelectListItem { Text = "", Value = "0" });
        }

        private string GetSKUsDetails(D_JobItem jobItem)
        {
            if (jobItem == null)
                return string.Empty;
            return (!string.IsNullOrWhiteSpace(jobItem.DesignatedSKU) ? jobItem.DesignatedSKU + ",0" : jobItem.JobItemLines.Select(l => l.SKU + "," + l.ItemID).Aggregate((current, next) => current + ";" + next));
        }

        #endregion

    }

    

}