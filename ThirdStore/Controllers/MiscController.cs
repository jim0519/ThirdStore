using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStore.Extensions;
using ThirdStoreCommon;
using ThirdStoreFramework.Controllers;
using ThirdStoreFramework.MVC;
using ThirdStoreFramework.Kendoui;
using ThirdStoreBusiness.Report;
using ThirdStoreCommon.Models;
using ThirdStoreBusiness.AccessControl;
using ThirdStoreCommon.Infrastructure;
using ThirdStore.Models.Misc;
using ThirdStoreBusiness.Misc;
using ThirdStoreBusiness.JobItem;
using System.IO;

namespace ThirdStore.Controllers
{
    public class MiscController : BaseController
    {
        private readonly IGumtreeFeedService _gumtreeFeedService;
        private readonly IPermissionService _permissionService;
        private readonly ILogService _logService;

        public MiscController(
            IGumtreeFeedService gumtreeFeedService,
            IPermissionService permissionService,
            ILogService logService
            )
        {
            _gumtreeFeedService = gumtreeFeedService;
            _permissionService = permissionService;
            _logService = logService;
        }
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GumtreeFeedList()
        {

            //if (!_permissionService.Authorize(ThirdStorePermission.KPIReport.ToName()))
            //{
            //    ErrorNotification("You do not have permission to process this page.");
            //    return Redirect("~/");
            //}


            var model = new GumtreeFeedListViewModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult GumtreeFeedList(DataSourceRequest command, GumtreeFeedListViewModel model)
        {
            var gumtreeFeedDS = _gumtreeFeedService.SearchGumtreeFeeds(
                model.SearchSKU,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            if(gumtreeFeedDS!=null&&gumtreeFeedDS.Count>0)
            {
                var gridModel = new DataSourceResult() { Data = gumtreeFeedDS, Total = gumtreeFeedDS.TotalCount };
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
        public ActionResult DownloadImage(IList<GumtreeFeed> selectedLines)
        {
            try
            {
                string handle = Guid.NewGuid().ToString();
                if (selectedLines != null && selectedLines.Count > 0)
                {
                    using (var stream = _gumtreeFeedService.ExportImages(selectedLines) as MemoryStream)
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
                return Json(new { Result = false, ErrMsg = ex.Message });
            }
        }

        [HttpGet]
        public virtual ActionResult Download(string fileGuid)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "text/csv, application/zip", CommonFunc.ToFileName("ExportImages", "zip"));
            }
            else
            {
                // Problem - Log the error, generate a blank file,
                //           redirect to another controller action - whatever fits with your application
                return new EmptyResult();
            }
        }

        [HttpPost]
        public ActionResult UploadDSZFile()
        {
            try
            {
                var files = Request.Files;
                var DSDataPath = ThirdStoreConfig.Instance.ThirdStoreDSZData + "\\DSZData";
                if (!Directory.Exists(DSDataPath))
                {
                    Directory.CreateDirectory(DSDataPath);
                }
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file != null && file.ContentLength > 0)
                    {
                        file.SaveAs(DSDataPath + "\\" + CommonFunc.ToCSVFileName("DSZData"));
                    }
                }
                SuccessNotification("Upload File Success.");
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                LogManager.Instance.Error(exc.Message);
                ErrorNotification("Upload File Failed." + exc.Message);
                return RedirectToAction("List");
            }
        }


        public ActionResult LogList()
        {

            //if (!_permissionService.Authorize(ThirdStorePermission.KPIReport.ToName()))
            //{
            //    ErrorNotification("You do not have permission to process this page.");
            //    return Redirect("~/");
            //}


            var model = new LogListViewModel();
            model.LogTimeFrom = DateTime.Today;
            model.LogTimeTo = DateTime.Today;

            return View(model);
        }

        [HttpPost]
        public ActionResult LogList(DataSourceRequest command, LogListViewModel model)
        {
            var logDS = _logService.SearchGumtreeFeeds(
                model.SearchMessage,
                model.LogTimeFrom,
                model.LogTimeTo,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);


            var logGridViewList = logDS.Select(i => i.ToModel());

            var gridModel = new DataSourceResult() { Data = logGridViewList, Total = logDS.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };


        }

    }
}