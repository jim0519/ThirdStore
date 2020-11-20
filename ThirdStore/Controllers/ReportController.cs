using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStore.Extensions;
using ThirdStore.Models.Report;
using ThirdStoreCommon;
using ThirdStoreFramework.Controllers;
using ThirdStoreFramework.MVC;
using ThirdStoreFramework.Kendoui;
using ThirdStoreBusiness.Report;
using ThirdStoreCommon.Models;
using ThirdStoreBusiness.AccessControl;
using ThirdStoreCommon.Infrastructure;

namespace ThirdStore.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IReportService _reportService;
        private readonly IPermissionService _permissionService;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;

        public ReportController(IReportService reportService,
            IWorkContext workContext,
            IPermissionService permissionService,
            ICacheManager cacheManager
            )
        {
            _reportService = reportService;
            _permissionService = permissionService;
            _cacheManager = cacheManager;
            _workContext = workContext;
        }
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult KPIReport()
        {
            //var kpiPermitUsers = new int[] { 1, 4, 17 };
            //if (!kpiPermitUsers.Contains(_workContext.CurrentUser.ID))
            //{
            //    ErrorNotification("You do not have permission to process this page.");
            //    return RedirectToAction("List", "JobItem");
            //}

            if (!_permissionService.Authorize(ThirdStorePermission.KPIReport.ToName()))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/");
            }


            var model = new KPIReportViewModel();

            model.CreateTimeFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            model.CreateTimeTo = DateTime.Now.Date;

            return View(model);
        }

        [HttpPost]
        public ActionResult KPIReport(DataSourceRequest command, KPIReportViewModel model)
        {
            var kpiRptDS = _reportService.GetKPIReport(
                createTimeFrom:model.CreateTimeFrom,
                createTimeTo:model.CreateTimeTo,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var gridModel = new DataSourceResult() { Data = kpiRptDS, Total = kpiRptDS.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        public ActionResult InventoryExcelExport(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

    }
}