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
using LINQtoCSV;
using System.Net;
using ThirdStoreCommon.Models.Image;
using ThirdStoreCommon.Models.JobItem;
using System.Data.Entity;
using ThirdStoreBusiness.Image;
using ThirdStoreBusiness.Item;
using ThirdStoreData;
using ThirdStoreCommon.Models.Item;

namespace ThirdStore.Controllers
{
    public class MiscController : BaseController
    {
        private readonly IGumtreeFeedService _gumtreeFeedService;
        private readonly IPermissionService _permissionService;
        private readonly ILogService _logService;
        private readonly IJobItemService _jobItemService;
        private readonly IItemService _itemService;
        private readonly IImageService _imageService;
        private readonly IDbContext _dbContext;

        public MiscController(
            IGumtreeFeedService gumtreeFeedService,
            IPermissionService permissionService,
            ILogService logService,
            IJobItemService jobItemService,
            IItemService itemService,
            IImageService imageService,
            IDbContext dbContext
            )
        {
            _gumtreeFeedService = gumtreeFeedService;
            _permissionService = permissionService;
            _logService = logService;
            _jobItemService = jobItemService;
            _itemService = itemService;
            _imageService = imageService;
            _dbContext = dbContext;
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


        public ActionResult DeveloperOperation()
        {

            if (!_permissionService.Authorize(ThirdStorePermission.DeveloperOperation.ToName()))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/");
            }

            return View();
        }



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



        [HttpPost]
        public ActionResult ImportCostwayData()
        {
            try
            {
                var costwayFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\ThirdStoreDSZData\\CostwayProductData20250116.csv";
                var csvContext = new CsvContext();
                var inputFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true };
                //var importData = csvContext.Read<JobItemImport>(@"C:\Users\gdutj\OneDrive\Document\Code\3rdStore\TODOList\File2\JobItem20190801.csv", inputFileDescription);
                var importData = csvContext.Read<CostwayItem>(costwayFilePath, inputFileDescription);
                var allItems = _itemService.GetAllItems();
                var costwayItems = allItems.Where(i => i.SupplierID.Equals(ThirdStoreSupplier.CW.ToValue())).ToList();
                foreach (var costwayItem in importData)
                {
                    if (costwayItems.FirstOrDefault(cw => cw.SKU.Trim().ToUpper().Equals(costwayItem.SKU.Trim().ToUpper())) != null)
                        continue;
                    var newItem = new D_Item();
                    newItem.SKU = costwayItem.SKU;
                    newItem.Name = costwayItem.Name;
                    newItem.Length = Math.Round( Convert.ToDecimal( costwayItem.Length)/100,2);
                    newItem.Width = Math.Round(Convert.ToDecimal(costwayItem.Width) / 100, 2);
                    newItem.Height = Math.Round(Convert.ToDecimal(costwayItem.Height) / 100, 2);
                    newItem.GrossWeight = Math.Round(Convert.ToDecimal(costwayItem.Weight)/1000, 2);
                    newItem.Ref1 = costwayItem.ReferenceSKU;
                    newItem.Ref4 = costwayItem.ProductLink;
                    newItem.Ref6 = costwayItem.Note;
                    newItem.SupplierID = 7;
                    newItem.Type = 2;
                    newItem.IsActive=true;

                    newItem.CreateTime = DateTime.Now;
                    newItem.CreateBy = Constants.SystemUser;
                    newItem.EditTime = DateTime.Now;
                    newItem.EditBy = Constants.SystemUser;

                    var imagesURL = new List<string>();
                    if (!string.IsNullOrEmpty(costwayItem.Image1))
                        imagesURL.Add(costwayItem.Image1);
                    if (!string.IsNullOrEmpty(costwayItem.Image2))
                        imagesURL.Add(costwayItem.Image2);
                    if (!string.IsNullOrEmpty(costwayItem.Image3))
                        imagesURL.Add(costwayItem.Image3);
                    if (!string.IsNullOrEmpty(costwayItem.Image4))
                        imagesURL.Add(costwayItem.Image4);
                    if (!string.IsNullOrEmpty(costwayItem.Image5))
                        imagesURL.Add(costwayItem.Image5);
                    if (!string.IsNullOrEmpty(costwayItem.Image6))
                        imagesURL.Add(costwayItem.Image6);
                    if (!string.IsNullOrEmpty(costwayItem.Image7))
                        imagesURL.Add(costwayItem.Image7);
                    if (!string.IsNullOrEmpty(costwayItem.Image8))
                        imagesURL.Add(costwayItem.Image8);

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
                                    var fileName = newItem.SKU + "-" + newItem.CreateTime.ToString("ddMM") + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                                    var imgObj = _imageService.SaveImage(stream, fileName);
                                    newItem.ItemImages.Add(new M_ItemImage()
                                    {
                                        Image = imgObj,
                                        DisplayOrder = i,
                                        StatusID = 0,//TODO Get item active status id
                                        CreateTime = newItem.CreateTime,
                                         CreateBy = Constants.SystemUser,
                                        EditTime = newItem.EditTime,
                                         EditBy= Constants.SystemUser
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


                //foreach (var grp in grpImportData)
                //{

                //    var headerLine = grp.FirstOrDefault(l => !string.IsNullOrEmpty(l.JobItemCreateTime) && !string.IsNullOrEmpty(l.ConditionID));
                //    if (headerLine != null)
                //    {
                //        try
                //        {
                //            var checkStr = (headerLine.OriginalReference.Trim().IndexOf("/") != -1 ? headerLine.OriginalReference.Trim().Substring(0, headerLine.OriginalReference.Trim().IndexOf("/")) : headerLine.OriginalReference.Trim()) + "-" + headerLine.SKU.Trim();
                //            if (existingJobItem.FirstOrDefault(eji => eji.Ref5.Equals(checkStr)) != null)
                //                continue;
                //            //throw new Exception($"Job Item Reference: {headerLine.OriginalReference.Trim()} SKU {headerLine.SKU.Trim()} already exist");
                //            var soldItem = soldData.FirstOrDefault(si => si.Reference.Trim().Equals(headerLine.OriginalReference.Trim()) && si.SKU.ToLower().Equals(headerLine.SKU.ToLower()));
                //            if (soldItem != null)
                //            {
                //                LogManager.Instance.Error($"SKU {soldItem.SKU} Reference {soldItem.Reference} has been sold.");
                //                continue;
                //            }
                //            var childLines = grp.Where(l => !l.Equals(headerLine));
                //            var item = items.FirstOrDefault(itm => itm.SKU.ToLower().Equals(headerLine.SKU.ToLower()));
                //            var netoProduct = netoProducts.FirstOrDefault(np => np.SKU.ToLower().Equals(headerLine.SKU.ToLower()));
                //            if (item != null)
                //            {
                //                var isNew = headerLine.ConditionID.ToUpper().Equals(ThirdStoreJobItemCondition.NEW.ToName());
                //                var newItem = new D_JobItem();
                //                //newItem.JobItemCreateTime = DateTime.ParseExact(headerLine.JobItemCreateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //                newItem.JobItemCreateTime = Convert.ToDateTime(headerLine.JobItemCreateTime);
                //                newItem.Type = ThirdStoreJobItemType.SELFSTORED.ToValue();
                //                newItem.StatusID = ThirdStoreJobItemStatus.PENDING.ToValue();
                //                newItem.ConditionID = headerLine.ConditionID.ToUpper().ToEnumValue<ThirdStoreJobItemCondition>();
                //                newItem.ItemName = (netoProduct != null ? netoProduct.Name : item.Name);
                //                newItem.Note = headerLine.ItemDetail;
                //                newItem.ItemPrice = Convert.ToDecimal((netoProduct != null ? netoProduct.DefaultPrice : headerLine.ItemPrice));
                //                newItem.Location = headerLine.Location;
                //                newItem.DesignatedSKU = (childLines != null && childLines.Count() > 0 ? headerLine.SKU : string.Empty);
                //                newItem.Ref1 = headerLine.Reference.Trim();
                //                newItem.Ref5 = (headerLine.OriginalReference.Trim().IndexOf("/") != -1 ? headerLine.OriginalReference.Trim().Substring(0, headerLine.OriginalReference.Trim().IndexOf("/")) : headerLine.OriginalReference.Trim()) + "-" + headerLine.SKU.Trim();
                //                newItem.CreateTime = newItem.JobItemCreateTime;
                //                newItem.EditTime = newItem.JobItemCreateTime;

                //                if (childLines != null && childLines.Count() > 0)
                //                {
                //                    foreach (var line in childLines)
                //                    {
                //                        var lineItem = items.FirstOrDefault(itm => itm.SKU.ToLower().Equals(line.SKU.ToLower()));
                //                        if (lineItem != null)
                //                        {
                //                            var newItemLine = new D_JobItemLine();
                //                            newItemLine.SKU = line.SKU;
                //                            newItemLine.ItemID = lineItem.ID;
                //                            newItemLine.Qty = 1;
                //                            newItemLine.Length = (string.IsNullOrEmpty(line.Length) ? lineItem.Length : Convert.ToDecimal(line.Length));
                //                            newItemLine.Width = (string.IsNullOrEmpty(line.Width) ? lineItem.Width : Convert.ToDecimal(line.Width));
                //                            newItemLine.Height = (string.IsNullOrEmpty(line.Height) ? lineItem.Height : Convert.ToDecimal(line.Height));
                //                            newItemLine.Weight = (string.IsNullOrEmpty(line.Weight) ? lineItem.GrossWeight : Convert.ToDecimal(line.Weight));
                //                            newItemLine.CreateTime = newItem.JobItemCreateTime;
                //                            newItemLine.EditTime = newItem.JobItemCreateTime;

                //                            newItem.JobItemLines.Add(newItemLine);
                //                        }
                //                        else
                //                        {
                //                            throw new Exception(line.SKU + " Job item line item info missed");
                //                        }
                //                    }
                //                }
                //                else
                //                {
                //                    var newItemLine = new D_JobItemLine();
                //                    newItemLine.SKU = headerLine.SKU;
                //                    newItemLine.ItemID = _itemService.GetItemBySKU(newItemLine.SKU).ID;
                //                    newItemLine.Qty = 1;
                //                    newItemLine.Length = Convert.ToDecimal((string.IsNullOrEmpty(headerLine.Length) ? item.Length.ToString() : headerLine.Length));
                //                    newItemLine.Width = Convert.ToDecimal((string.IsNullOrEmpty(headerLine.Width) ? item.Width.ToString() : headerLine.Width));
                //                    newItemLine.Height = Convert.ToDecimal((string.IsNullOrEmpty(headerLine.Height) ? item.Height.ToString() : headerLine.Height));
                //                    newItemLine.Weight = Convert.ToDecimal((string.IsNullOrEmpty(headerLine.Weight) ? item.GrossWeight.ToString() : headerLine.Weight));
                //                    newItemLine.CreateTime = newItem.JobItemCreateTime;
                //                    newItemLine.EditTime = newItem.JobItemCreateTime;

                //                    newItem.JobItemLines.Add(newItemLine);
                //                }

                //                if (!string.IsNullOrEmpty(headerLine.ImagePath))
                //                {
                //                    if (Directory.Exists(imageDI + "\\" + headerLine.ImagePath))
                //                    {
                //                        var imageFiles = Directory.GetFiles(imageDI + "\\" + headerLine.ImagePath, "*", SearchOption.AllDirectories);
                //                        int j = 0;
                //                        foreach (var imgFile in imageFiles)
                //                        {
                //                            //Image img = Image.FromFile(imgFile);

                //                            using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(imgFile)))
                //                            {
                //                                var fileName = headerLine.SKU + "-" + newItem.JobItemCreateTime.ToString("ddMM") + headerLine.Reference + "-" + j.ToString().PadLeft(2, '0') + ".jpg";
                //                                var imgObj = _imageService.SaveImage(stream, fileName);
                //                                newItem.JobItemImages.Add(new M_JobItemImage()
                //                                {
                //                                    Image = imgObj,
                //                                    DisplayOrder = j,
                //                                    StatusID = 0,//TODO Get item active status id
                //                                    CreateTime = newItem.CreateTime,
                //                                    EditTime = newItem.EditTime
                //                                });
                //                            }
                //                            j++;
                //                        }
                //                    }
                //                    else
                //                    {
                //                        throw new Exception(headerLine.SKU + " Image file path not exists");
                //                    }
                //                }
                //                else if (!string.IsNullOrEmpty(headerLine.Image1))
                //                {
                //                    var imagesURL = new List<string>();
                //                    if (!string.IsNullOrEmpty(headerLine.Image1))
                //                        imagesURL.Add(headerLine.Image1);
                //                    if (!string.IsNullOrEmpty(headerLine.Image2))
                //                        imagesURL.Add(headerLine.Image2);
                //                    if (!string.IsNullOrEmpty(headerLine.Image3))
                //                        imagesURL.Add(headerLine.Image3);
                //                    if (!string.IsNullOrEmpty(headerLine.Image4))
                //                        imagesURL.Add(headerLine.Image4);
                //                    if (!string.IsNullOrEmpty(headerLine.Image5))
                //                        imagesURL.Add(headerLine.Image5);
                //                    if (!string.IsNullOrEmpty(headerLine.Image6))
                //                        imagesURL.Add(headerLine.Image6);

                //                    int i = 0;
                //                    using (var wc = new WebClient())
                //                    {
                //                        foreach (var imageURL in imagesURL)
                //                        {
                //                            try
                //                            {
                //                                var imgBytes = wc.DownloadData(imageURL);
                //                                using (var stream = new MemoryStream(imgBytes))
                //                                {
                //                                    var fileName = headerLine.SKU + "-" + newItem.JobItemCreateTime.ToString("ddMM") + headerLine.Reference + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                //                                    var imgObj = _imageService.SaveImage(stream, fileName);
                //                                    newItem.JobItemImages.Add(new M_JobItemImage()
                //                                    {
                //                                        Image = imgObj,
                //                                        DisplayOrder = i,
                //                                        StatusID = 0,//TODO Get item active status id
                //                                        CreateTime = newItem.CreateTime,
                //                                        EditTime = newItem.EditTime
                //                                    });
                //                                }

                //                            }
                //                            catch (Exception ex)
                //                            {
                //                                LogManager.Instance.Error(imageURL + " download failed. " + ex.Message);
                //                            }

                //                            i++;
                //                        }
                //                    }
                //                }

                //                _jobItemService.InsertJobItem(newItem);
                //                LogManager.Instance.Info($"Reference {headerLine.OriginalReference} SKU {headerLine.SKU} Supplier {headerLine.Supplier} import successfully.");
                //            }
                //            else
                //            {
                //                throw new Exception("Item or Neto Product Info Missed.");
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            LogManager.Instance.Error($"Reference {headerLine.OriginalReference} SKU {headerLine.SKU} Supplier {headerLine.Supplier} import failed. " + ex.Message);
                //        }

                //    }


                //}
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
                var type2 = System.Type.GetType("ThirdStoreBusiness.ScheduleTask.UpdateDSDataAndSync, ThirdStoreBusiness");
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


        #region update Sello Image

        [HttpPost]
        public ActionResult UpdateSelloImage()
        {
            try 
            {
                var selloFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\ThirdStoreDSZData\\SelloImages.csv";
                var csvContext = new CsvContext();
                var inputFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true };
                var importData = csvContext.Read<SelloImageLine>(selloFilePath, inputFileDescription);
                var allItems = _itemService.GetAllItems();
                var selloItems = allItems.Where(i => i.SupplierID.Equals(ThirdStoreSupplier.S.ToValue()));
                foreach(var si in selloItems)
                {
                    var fi = importData.FirstOrDefault(d=>d.SKU.ToLower().Equals(si.SKU.ToLower()));
                    if(fi!=null)
                    {
                        var imagesURL = new List<string>();
                        if (!string.IsNullOrEmpty(fi.Image1))
                            imagesURL.Add(fi.Image1);
                        if (!string.IsNullOrEmpty(fi.Image2))
                            imagesURL.Add(fi.Image2);
                        if (!string.IsNullOrEmpty(fi.Image3))
                            imagesURL.Add(fi.Image3);
                        if (!string.IsNullOrEmpty(fi.Image4))
                            imagesURL.Add(fi.Image4);
                        if (!string.IsNullOrEmpty(fi.Image5))
                            imagesURL.Add(fi.Image5);
                        if (!string.IsNullOrEmpty(fi.Image6))
                            imagesURL.Add(fi.Image6);
                        if (!string.IsNullOrEmpty(fi.Image7))
                            imagesURL.Add(fi.Image7);
                        if (!string.IsNullOrEmpty(fi.Image8))
                            imagesURL.Add(fi.Image8);

                        if (imagesURL.Count == 0)
                            continue;

                        var createTime = DateTime.Now;
                        var createBy = "System";
                        if (si.ItemImages.Count > 0)
                        {
                            var itemImages = _itemService.GetItemImagesByItemID(si.ID);
                            var keepItemImages = itemImages.Where(ii => ii.StatusID == 1).ToList();
                            itemImages = itemImages.Where(ii => ii.StatusID == 0).ToList();
                            foreach (var existPic in itemImages)
                                _itemService.DeleteItemImage(existPic);

                            var keepButNotValidItemImages = new List<M_ItemImage>();
                            foreach (var kii in keepItemImages)
                            {
                                var imgURL = _imageService.GetImageURL(kii.ImageID);
                                if (!CommonFunc.DoesImageExistRemotely(imgURL))
                                    keepButNotValidItemImages.Add(kii);
                            }
                            foreach (var kbii in keepButNotValidItemImages)
                                _itemService.DeleteItemImage(kbii);
                        }

                        int i = 0;
                        using (var wc = new ThirdStoreWebClient())
                        {
                            foreach (var existPic in si.ItemImages)
                            {
                                existPic.DisplayOrder = i;
                                i++;
                            }

                            foreach (var imageURL in imagesURL)
                            {
                                try
                                {
                                    var imgBytes = wc.DownloadData(imageURL);
                                    using (var stream = new MemoryStream(imgBytes))
                                    {
                                        var fileName = si.SKU + "-" + i.ToString().PadLeft(2, '0') + ".jpg";
                                        var imgObj = _imageService.SaveImage(stream, fileName);
                                        si.ItemImages.Add(new M_ItemImage()
                                        {
                                            Image = imgObj,
                                            DisplayOrder = i,
                                            StatusID = 0,//TODO Get item active status id
                                            CreateTime = createTime,
                                            CreateBy = createBy,
                                            EditTime = createTime,
                                            EditBy = createBy
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

        public string JobItemCreateTime { get; set; }

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

    public class CostwayItem
    {
        [CsvColumn(Name = "SKU")]
        public string SKU { get; set; }
        [CsvColumn(Name = "Length")]
        public decimal Length { get; set; }
        [CsvColumn(Name = "Width")]
        public decimal Width { get; set; }
        [CsvColumn(Name = "Height")]
        public decimal Height { get; set; }
        [CsvColumn(Name = "Weight")]
        public decimal Weight { get; set; }
        [CsvColumn(Name = "Reference SKU")]
        public string ReferenceSKU { get; set; }
        [CsvColumn(Name = "Note")]
        public string Note { get; set; }
        [CsvColumn(Name = "Name")]
        public string Name { get; set; }
        [CsvColumn(Name = "Product Link")]
        public string ProductLink { get; set; }
        [CsvColumn(Name = "Image1")]
        public string Image1 { get; set; }
        [CsvColumn(Name = "Image2")]
        public string Image2 { get; set; }
        [CsvColumn(Name = "Image3")]
        public string Image3 { get; set; }
        [CsvColumn(Name = "Image4")]
        public string Image4 { get; set; }
        [CsvColumn(Name = "Image5")]
        public string Image5 { get; set; }
        [CsvColumn(Name = "Image6")]
        public string Image6 { get; set; }
        [CsvColumn(Name = "Image7")]
        public string Image7 { get; set; }
        [CsvColumn(Name = "Image8")]
        public string Image8 { get; set; }

    }

    public class SelloImageLine
    {
        [CsvColumn(Name = "Item No.")]
        public string SKU { get; set; }
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

    #endregion
}