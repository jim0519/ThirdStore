﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStore.Models.Order;
using ThirdStoreFramework.Kendoui;
using ThirdStoreBusiness.Order;
using ThirdStore.Extensions;
using ThirdStoreFramework.Controllers;
using ThirdStoreCommon;
using ThirdStoreFramework.MVC;
using ThirdStoreCommon.Infrastructure;
using System.IO;
using ThirdStoreBusiness.AccessControl;

namespace ThirdStore.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;

        public OrderController(IOrderService orderService,
            IPermissionService permissionService,
            IWorkContext workContext
            )
        {
            _orderService = orderService;
            _workContext = workContext;
            _permissionService = permissionService;
        }

        // GET: Item
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            //var allowOrderDownloadUserIDs = new int[] { 1,4,6,10,11,12,14,16,17,22};
            
            //if (!allowOrderDownloadUserIDs.Contains(_workContext.CurrentUser.ID))
            //{
            //    ErrorNotification("You do not have permission to process this page.");
            //    return Redirect("~/"); ;
            //}

            if (!_permissionService.Authorize(ThirdStorePermission.OrderList.ToName()))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/");
            }

            var model = new OrderListViewModel();

            model.OrderStatuses = ThirdStoreOrderStatus.AllGood.ToSelectList(false).ToList();
            model.OrderStatuses.Insert(0, new SelectListItem { Text = "All", Value = "0", Selected = true });

            //var canEditOrder = new int[] { 1, 4,6, 10,11,12, 14, 16, 17,22 };
            //model.CanEditOrder = canEditOrder.Contains(_workContext.CurrentUser.ID);
            model.CanEditOrder = _permissionService.Authorize(ThirdStorePermission.OrderEdit.ToName());

            return View(model);
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command, OrderListViewModel model)
        {
            var orders = _orderService.SearchOrders(
                orderTimeFrom:model.OrderTimeFrom,
                orderTimeTo:model.OrderTimeTo,
                channelOrderID:model.SearchChannelOrderID,
                jobItemID:model.SearchJobItemID,
                customerID:model.SearchCustomerID,
                consigneeName:model.SearchConsigneeName,
                sku:model.SearchSKU,
                statusID:model.SearchStatusID,
                pageIndex: command.Page - 1,
                pageSize:command.PageSize);

            var orderGridViewList = orders.Select(i => {
                var viewModel = i.ToModel();
                if (i.OrderLines.Count > 0)
                { 
                    viewModel.OrderTransactions = i.OrderLines.Select(l => l.Ref2.ToString()).Aggregate((current, next) => current + ";" + next);
                    viewModel.SKUs=i.OrderLines.Select(l=>l.SKU+":"+l.Qty+(!string.IsNullOrWhiteSpace(l.Ref5)?"("+l.Ref5+")":string.Empty)).Aggregate((current, next) => current + ";" + next);
                }
                return viewModel;
            });

            var gridModel = new DataSourceResult() { Data = orderGridViewList, Total = orders.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        public ActionResult GridDataUpdate(OrderGridViewModel model)
        {
            if (model != null)
            {
                var order = _orderService.GetOrderByID(model.ID);
                if(order!=null)
                {
                    order.StatusID = model.StatusID;
                    order.Ref2 = model.Ref2;
                    order.Ref3 = model.Ref3;
                    order.Ref4 = model.Ref4;
                    order.Ref5 = model.Ref5;
                    _orderService.UpdateOrder(order);
                }
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult UpdateOrderDeliveryInstruction(DateTime updateOrderFrom, DateTime updateOrderTo)
        {
            try
            {
                _orderService.UpdateOrderDeliveryInstruction(updateOrderFrom, updateOrderTo);
                return Json(new { Result = true });
            }
            catch(Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ExportDSZFile(string orderIds)
        {
            try
            {
                string handle = Guid.NewGuid().ToString();
                if (orderIds != null)
                {
                    var ids = orderIds
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Convert.ToInt32(x))
                        .ToList();

                    using (var stream = _orderService.ExportDSImportFile(ids) as MemoryStream)
                    {
                        TempData[handle] = stream.ToArray();
                    }
                }

                return new JsonResult()
                {
                    Data = new { Result = true, FileGuid = handle }
                };
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                ErrorNotification("Export file failed." + ex.Message);
                return Json(new { Result = false, FileGuid = string.Empty });
            }
        }

        [HttpGet]
        public virtual ActionResult Download(string fileGuid)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "text/csv, application/zip", CommonFunc.ToCSVFileName("ExportOrders").Replace("csv", "zip"));
            }
            else
            {
                // Problem - Log the error, generate a blank file,
                //           redirect to another controller action - whatever fits with your application
                return new EmptyResult();
            }
        }

        [HttpPost]
        public ActionResult UploadTracking()
        {
            try
            {
                var files = Request.Files;
                if (!Directory.Exists(ThirdStoreConfig.Instance.UploadTrackingPath))
                {
                    Directory.CreateDirectory(ThirdStoreConfig.Instance.UploadTrackingPath);
                }
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        using (var sr = new StreamReader(file.InputStream))
                        {
                            _orderService.UploadTracking(sr);

                            file.SaveAs(ThirdStoreConfig.Instance.UploadTrackingPath + "\\" + CommonFunc.ToCSVFileName(file.FileName));
                        }
                    }
                }
                SuccessNotification("Upload Tracking Success.");
                //if (file != null && file.ContentLength > 0)
                //{

                //    var success = _salesOrderService.UpdateSalesOrderShipment(file.InputStream, uploadFileType);
                //    if (!success)
                //    {
                //        ErrorNotification("Update Sales Order Shipment Failed.");
                //        return RedirectToAction("List");
                //    }

                //}
                //else
                //{
                //    ErrorNotification("Update Sales Order Shipment Failed.");
                //    return RedirectToAction("List");
                //}
                //SuccessNotification("Update Sales Order Shipment Success.");
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                LogManager.Instance.Error(exc.Message);
                ErrorNotification("Upload Tracking Failed." + exc.Message);
                return RedirectToAction("List");
            }
        }

        public ActionResult ScreenshotDisplay(string orderTran)
        {
            if (!string.IsNullOrWhiteSpace(orderTran) && orderTran.Split('-').Count() == 2)
            {
                var itemID = orderTran.Split('-')[0];
                var tranID= orderTran.Split('-')[1];
                var htmlStr = _orderService.GetOrderScreenshot(orderTran);
                htmlStr = $"<a href='http://cgi.ebay.com.au/ws/eBayISAPI.dll?ViewItemVersion&item={itemID}&tid={tranID}&view=all' target='_blank'>http://cgi.ebay.com.au/ws/eBayISAPI.dll?ViewItemVersion&item={itemID}&tid={tranID}&view=all</a><br /><br />" + htmlStr;
                return Content(htmlStr);
            }
            else
                return Redirect("~/");
            
        }

    }
}