using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStoreFramework.Controllers;
using ThirdStoreCommon;
using ThirdStoreFramework.Kendoui;
using ThirdStore.Models.ReturnItem;
using ThirdStore.Extensions;
using ThirdStoreBusiness.ReturnItem;
using ThirdStoreBusiness.Item;
using ThirdStoreCommon.Models.ReturnItem;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Controllers
{
    public class ReturnItemController : BaseController
    {
        private readonly IReturnItemService _returnItemService;
        private readonly IItemService _itemService;
        public ReturnItemController(IReturnItemService returnItemService,
             IItemService itemService)
        {
            _returnItemService = returnItemService;
            _itemService = itemService;
        }

        public ActionResult List()
        {
            var model = new ReturnItemListViewModel();

            model.ReturnItemStatuses = ThirdStoreReturnItemStatus.PARTIAL.ToSelectList(false).ToList();
            model.ReturnItemStatuses.Insert(0, new SelectListItem { Text = "All", Value = "0" });

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

            //if (model.ReturnItemViewImages != null && model.ReturnItemViewImages.Count > 0)
            //{
            //    foreach (var lModel in model.ReturnItemViewImages)
            //    {
            //        var newEntityLine = lModel.ToEntity().FillOutNull();
            //        newEntityModel.ReturnItemImages.Add(newEntityLine);
            //    }
            //}

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


            //if (model.ReturnItemViewImages != null && model.ReturnItemViewImages.Count > 0)
            //{
            //    foreach (var lModel in model.ReturnItemViewImages)
            //    {
            //        if (lModel.ID > 0)
            //        {
            //            var originLine = editEntityModel.ReturnItemImages.Where(l => l.ID == lModel.ID).FirstOrDefault();
            //            if (originLine != null)
            //            {
            //                originLine = lModel.ToEntity(originLine).FillOutNull();
            //            }
            //        }
            //        else
            //        {
            //            var editEntityLine = lModel.ToEntity().FillOutNull();
            //            editEntityModel.ReturnItemImages.Add(editEntityLine);
            //        }
            //    }
            //}

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
            var returnItem = _returnItemService.FindByTrackingNumber(trackingNumber);
            if (returnItem == null)
            {
                return Json(new { Result = false, ErrMessage = "Cannot match the return item or there are more than one return item matches the tracking number" });
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
                            if(model.StatusID==ThirdStoreReturnItemStatus.FULL.ToValue())
                                return Json(new { Result = false, Message = "The status cannot be FULL as the item structure is not completed." });
                        }
                        else if(model.StatusID == ThirdStoreReturnItemStatus.PARTIAL.ToValue())
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

                if (model.StatusID == ThirdStoreReturnItemStatus.PARTIAL.ToValue())
                {
                    return Json(new { Result = false, Message = "The status should be FULL as the item structure is completed." });
                }
            }

            if (model.ReturnItemViewLines.Count > 0 )
            {
                if(model.ReturnItemViewLines.Any(l => l.Width.Equals(0) || l.Length.Equals(0) || l.Height.Equals(0) || l.Weight.Equals(0) || l.CubicWeight.Equals(0)))
                    return Json(new { Result = false, Message = "The length, width, height, weight and cubic weight must not be equal to 0." });
                
                if(model.ReturnItemViewLines.Any(l=>string.IsNullOrWhiteSpace( l.Location)))
                    return Json(new { Result = false, Message = "Please make sure all return line items have valid locations." });
            }


            //if(model.ReturnItemViewLines.Any(l=>l.Qty>1))
            return Json(new { Result = true });
        }




        private void FillDropDownDS(ReturnItemViewModel model)
        {
            model.ReturnItemStatuses = ThirdStoreReturnItemStatus.PARTIAL.ToSelectList(false).ToList();
        }

        private string GetSKUsDetails(D_ReturnItem returnItem)
        {
            if (returnItem == null)
                return string.Empty;
            return (!string.IsNullOrWhiteSpace(returnItem.DesignatedSKU) ? returnItem.DesignatedSKU + ",0" : returnItem.ReturnItemLines.Select(l => l.SKU + "," + l.ItemID).Aggregate((current, next) => current + ";" + next));
        }
    }
}