using System;
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

namespace ThirdStore.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;

        public OrderController(IOrderService orderService,
            IWorkContext workContext
            )
        {
            _orderService = orderService;
            _workContext = workContext;
        }

        // GET: Item
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            var allowOrderDownloadUserIDs = new int[] { 1,4,14,17 };
            if (!allowOrderDownloadUserIDs.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }

            var model = new OrderListViewModel();
            
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
                pageIndex: command.Page - 1,
                pageSize:command.PageSize);

            var orderGridViewList = orders.Select(i => {
                var viewModel = i.ToModel();
                if (i.OrderLines.Count > 0)
                    viewModel.OrderTransactions = i.OrderLines.Select(l => l.Ref2.ToString()).Aggregate((current, next) => current + ";" + next);
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