using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThirdStoreBusiness.API.Dropshipzone;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.Order;
using ThirdStoreBusiness.Setting;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Item;
using ThirdStoreCommon.Models.Order;

namespace ThirdStoreBusiness.DSChannel
{
    public interface IDSChannel
    {
        int Order { get; }

        int DSChannel { get; }

        //should create a new middle class(data contract) for importing item to current system, but we use ref5 as the images path at the moment
        IList<D_Item> GetDSData();

        IList<string> GetNeedSyncSKUs();

        Stream ExportBatchOrderFile(IList<D_Order_Line> orderLines);

        IList<Tuple<string, IList<string>>> GetImagesPathsBySKUs(IList<string> skus);

        IList<Tuple<string, int,bool>> GetInventoryQtyBySKUs(IList<string> skus);
    }

    public class DSZChannel : IDSChannel
    {
        private readonly CsvContext _csvContext;
        private readonly CsvFileDescription _csvFileDescription;
        private readonly ISettingService _settingService;
        private readonly CommonSettings _commonSetting;
        private readonly IDropshipzoneAPICallManager _dropshipzoneAPIManager;
        public DSZChannel(
            IDropshipzoneAPICallManager dropshipzoneAPIManager,
            CsvContext csvContext,
            CsvFileDescription csvFileDescription,
            ISettingService settingService)
        {
            _dropshipzoneAPIManager = dropshipzoneAPIManager;
            _csvContext = csvContext;
            _csvFileDescription = csvFileDescription;
            _settingService = settingService;
            _commonSetting = _settingService.LoadSetting<CommonSettings>();
        }

        public int DSChannel =>ThirdStoreSupplier.P.ToValue();

        public int Order => 0;

        protected string DSDataPath => ThirdStoreConfig.Instance.ThirdStoreDSZData + "\\DSZData";
        protected string DSDataURL => "http://dropshipzone.com.au/sample/Standard/sku_list_zone_rates.csv";
        //protected string DSDataURL => "https://www.dropshipzone.com.au/rsdropship/download/downloadSkuList/";
        public Stream ExportBatchOrderFile(IList<D_Order_Line> orderLines)
        {
            try
            {
                Stream retStream = new MemoryStream();
                var listDSZImportLine = new List<DSZImportLine>();

                foreach (var line in orderLines)
                {
                    if (string.IsNullOrWhiteSpace(line.Ref5))//item has been allocated and not local stock item
                    {
                        var o = line.Order;
                        var importLine = new DSZImportLine();
                        importLine.serial_number = line.Ref1;
                        var firstName = o.ConsigneeName.Substring(0, o.ConsigneeName.IndexOf(" "));
                        var lastName = o.ConsigneeName.Substring(o.ConsigneeName.IndexOf(" ") + 1);
                        importLine.first_name = firstName;
                        importLine.last_name = lastName;

                        importLine.address1 = o.ShippingAddress1;
                        importLine.address2 = o.ShippingAddress2;
                        importLine.suburb = o.ShippingSuburb;
                        importLine.state = o.ShippingState;
                        importLine.country = o.ShippingCountry;
                        importLine.postcode = o.ShippingPostcode;
                        importLine.telephone = o.ConsigneePhoneNo;
                        importLine.sku = line.SKU;
                        importLine.price = 10;
                        importLine.postage = 0;
                        importLine.qty = line.Qty;
                        importLine.comment = o.BuyerNote;

                        listDSZImportLine.Add(importLine);
                    }
                }


                var stWriter = new StreamWriter(retStream);
                _csvContext.Write(listDSZImportLine, stWriter, _csvFileDescription);
                stWriter.Flush();
                retStream.Position = 0;

                return retStream;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public IList<D_Item> GetDSData()
        {
            var retItems = new List<D_Item>();
            try
            {
                IList<DSZModel> dsDatas = null;
                //using (var webClient = new ThirdStoreWebClient())
                //{
                var dszProducts = _dropshipzoneAPIManager.GetAllProducts();
                if (dszProducts != null && dszProducts.Count > 0)
                {
                    dsDatas = ConvertToDSZModel(dszProducts);


                    //if (!Directory.Exists(this.DSDataPath))
                    //    Directory.CreateDirectory(this.DSDataPath);
                    //var di = new DirectoryInfo(this.DSDataPath);
                    //FileInfo[] files = di.GetFiles().ToArray();
                    //if (files.Count() > 0)
                    //{
                    //var fileName = this.DSDataPath+"\\" + CommonFunc.ToCSVFileName("DSZData");

                    //webClient.DownloadFile(DSDataURL, fileName);
                    //var topDataFile = files.OrderByDescending(fi => fi.CreationTime).FirstOrDefault();
                    //dsDatas = _csvContext.Read<DSZModel>(topDataFile.FullName, _csvFileDescription).ToList();

                    if (dsDatas != null)
                    {
                        if (!Directory.Exists(this.DSDataPath))
                            Directory.CreateDirectory(this.DSDataPath);
                        var fileName = this.DSDataPath + "\\" + CommonFunc.ToCSVFileName("DSZData");
                        dsDatas = dsDatas.Where(d=>!string.IsNullOrWhiteSpace(d.SKU)).ToList();
                        _csvContext.Write(dsDatas, fileName, _csvFileDescription);

                        dsDatas = dsDatas.Where(s => !s.SKU.Contains("*") && !Regex.IsMatch(s.SKU, @"^V\d+")).ToList();

                        foreach (var dsData in dsDatas)
                        {
                            var newItem = new D_Item();
                            if (dsData.SKU.Length > 23)
                                LogManager.Instance.Info($"SKU {dsData.SKU} length is larger than 23.");
                            newItem.SKU = dsData.SKU;
                            newItem.Name = dsData.Title;
                            newItem.Description = dsData.Description;
                            newItem.Cost =(dsData.SpecialPrice!=0?dsData.SpecialPrice: dsData.Price);

                            var postage = new List<decimal>() {
                            dsData.ACT.IsNumeric()? Convert.ToDecimal(dsData.ACT):0,
                            dsData.NSW_M.IsNumeric()? Convert.ToDecimal(dsData.NSW_M):0,
                            dsData.NSW_R.IsNumeric()? Convert.ToDecimal(dsData.NSW_R):0,
                            dsData.NT_M.IsNumeric()? Convert.ToDecimal(dsData.NT_M):0,
                            dsData.NT_R.IsNumeric()? Convert.ToDecimal(dsData.NT_R):0,
                            dsData.QLD_M.IsNumeric()? Convert.ToDecimal(dsData.QLD_M):0,
                            dsData.QLD_R.IsNumeric()? Convert.ToDecimal(dsData.QLD_R):0,
                            dsData.REMOTE.IsNumeric()? Convert.ToDecimal(dsData.REMOTE):0,
                            dsData.SA_M.IsNumeric()? Convert.ToDecimal(dsData.SA_M):0,
                            dsData.SA_R.IsNumeric()? Convert.ToDecimal(dsData.SA_R):0,
                            dsData.TAS_M.IsNumeric()? Convert.ToDecimal(dsData.TAS_M):0,
                            dsData.TAS_R.IsNumeric()? Convert.ToDecimal(dsData.TAS_R):0,
                            dsData.VIC_M.IsNumeric()? Convert.ToDecimal(dsData.VIC_M):0,
                            dsData.VIC_R.IsNumeric()? Convert.ToDecimal(dsData.VIC_R):0,
                            dsData.WA_M.IsNumeric()? Convert.ToDecimal(dsData.WA_M):0,
                            dsData.WA_R.IsNumeric()? Convert.ToDecimal(dsData.WA_R):0
                            }.Max();

                            //newItem.Price = (newItem.Cost + postage) * _commonSetting.DropshipMarkupRate;
                            newItem.Price = newItem.Cost * _commonSetting.DropshipMarkupRate;
                            newItem.Type = ThirdStoreItemType.SINGLE.ToValue();
                            newItem.SupplierID = ThirdStoreSupplier.P.ToValue();
                            newItem.GrossWeight = (!string.IsNullOrWhiteSpace(dsData.Weight) ? Convert.ToDecimal(dsData.Weight) : 0);
                            newItem.NetWeight = (!string.IsNullOrWhiteSpace(dsData.Weight) ? Convert.ToDecimal(dsData.Weight) : 0);
                            newItem.Length = (!string.IsNullOrWhiteSpace(dsData.Length) ? Convert.ToDecimal(dsData.Length) / 100 : 0);
                            newItem.Width = (!string.IsNullOrWhiteSpace(dsData.Width) ? Convert.ToDecimal(dsData.Width) / 100 : 0);
                            newItem.Height = (!string.IsNullOrWhiteSpace(dsData.Height) ? Convert.ToDecimal(dsData.Height) / 100 : 0);
                            newItem.Ref3 =!string.IsNullOrWhiteSpace( dsData.EANCode)? dsData.EANCode.Trim():string.Empty;
                            newItem.IsReadyForList = (dsData.SKU.Length <= 23 && dsData.Price <= Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceBelow) ? true : false);
                            newItem.IsActive = true;

                            var imagesURL = GetImageURLList(dsData);
                            if (imagesURL.Count > 0)
                                newItem.Ref5 = imagesURL.Aggregate((current, next) => current + ";" + next);

                            retItems.Add(newItem);
                        }
                    }
                    //}
                }

                return retItems;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        private IList<DSZModel> ConvertToDSZModel(IList<DropshipzoneProduct> dszProducts)
        {
            var dsDatas = new List<DSZModel>();
            foreach(var dszProduct in dszProducts)
            {
                var dsData = new DSZModel();
                dsData.SKU = dszProduct.sku;
                dsData.Category = dszProduct.Category;
                dsData.Title = dszProduct.title;
                dsData.InventoryQty =Convert.ToDecimal( dszProduct.stock_qty);
                dsData.Status = dszProduct.status;
                dsData.Price = Convert.ToDecimal(dszProduct.price);
                dsData.RrpPrice = dszProduct.RrpPrice;
                if(dszProduct.zone_rates!=null&&dszProduct.zone_rates.Count>0)
                {
                    var zoneRate = dszProduct.zone_rates.FirstOrDefault();
                    dsData.ACT = zoneRate.act;
                    dsData.NSW_M = zoneRate.nsw_m;
                    dsData.NSW_R = zoneRate.nsw_r;
                    dsData.NT_M = zoneRate.nt_m;
                    dsData.NT_R = zoneRate.nt_r;
                    dsData.QLD_M = zoneRate.qld_m;
                    dsData.QLD_R = zoneRate.qld_r;
                    dsData.REMOTE = zoneRate.remote;
                    dsData.SA_M = zoneRate.sa_m;
                    dsData.SA_R = zoneRate.sa_r;
                    dsData.TAS_M = zoneRate.tas_m;
                    dsData.TAS_R = zoneRate.tas_r;
                    dsData.VIC_M = zoneRate.vic_m;
                    dsData.VIC_R = zoneRate.vic_r;
                    dsData.WA_M = zoneRate.wa_m;
                    dsData.WA_R = zoneRate.wa_r;
                }
                dsData.IsBulkyItem = dszProduct.bulky_item;
                dsData.Discontinued = dszProduct.discontinued;
                dsData.EANCode = dszProduct.eancode;
                dsData.Brand = dszProduct.brand;
                //dsData.MPN=dszProduct.mpn
                dsData.Weight = dszProduct.weight;
                dsData.Length = dszProduct.length;
                dsData.Width = dszProduct.width;
                dsData.Height = dszProduct.height;
                dsData.Description = dszProduct.desc;
                dsData.Color = dszProduct.colour;
                if(!string.IsNullOrWhiteSpace(dszProduct.special_price))
                    dsData.SpecialPrice = Convert.ToDecimal(dszProduct.special_price);
                if (!string.IsNullOrWhiteSpace(dszProduct.special_price_end_date))
                    dsData.SpecialPriceEndDate = Convert.ToDateTime(dszProduct.special_price_end_date);
                if (dszProduct.gallery!=null&&dszProduct.gallery.Count>0)
                {
                    var imgIndex = 1;
                    foreach(var img in dszProduct.gallery)
                    {
                        switch (imgIndex)
                        {
                            case 1:
                                dsData.Image1 = img;
                                break;
                            case 2:
                                dsData.Image2 = img;
                                break;
                            case 3:
                                dsData.Image3 = img;
                                break;
                            case 4:
                                dsData.Image4 = img;
                                break;
                            case 5:
                                dsData.Image5 = img;
                                break;
                            case 6:
                                dsData.Image6 = img;
                                break;
                            case 7:
                                dsData.Image7 = img;
                                break;
                            case 8:
                                dsData.Image8 = img;
                                break;
                            case 9:
                                dsData.Image9 = img;
                                break;
                            case 10:
                                dsData.Image10 = img;
                                break;
                            case 11:
                                dsData.Image11 = img;
                                break;
                            case 12:
                                dsData.Image12 = img;
                                break;
                            case 13:
                                dsData.Image13 = img;
                                break;
                            case 14:
                                dsData.Image14 = img;
                                break;
                            case 15:
                                dsData.Image15 = img;
                                break;
                            default:
                                break;
                        }

                        imgIndex++;   
                    }
                }


                dsDatas.Add(dsData);
            }
            return dsDatas;
        }

        public IList<Tuple<string, IList<string>>> GetImagesPathsBySKUs(IList<string> skus)
        {
            try
            {
                var retList = new List<Tuple<string, IList<string>>>();
                if (skus == null || skus.Count == 0)
                    return retList;
                var di = new DirectoryInfo(DSDataPath);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles().ToArray();
                    if (files.Count() > 0)
                    {
                        var topDataFile = files.OrderByDescending(fi => fi.CreationTime).FirstOrDefault();
                        var dsDatas = _csvContext.Read<DSZModel>(topDataFile.FullName, _csvFileDescription);

                        foreach (var sku in skus)
                        {
                            var dsData = dsDatas.FirstOrDefault(d => d.SKU.ToLower().Equals(sku.ToLower()));
                            if (dsData != null)
                            {
                                var imagesURL = GetImageURLList(dsData);
                                retList.Add(new Tuple<string, IList<string>>(dsData.SKU, imagesURL));
                            }
                        }
                    }
                }

                return retList;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public IList<Tuple<string, int,bool>> GetInventoryQtyBySKUs(IList<string> skus)
        {
            try
            {
                var retList = new List<Tuple<string, int,bool>>();
                var di = new DirectoryInfo(DSDataPath);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles().ToArray();
                    if (files.Count() > 0)
                    {
                        var dszDataFile = files.OrderByDescending(fi => fi.CreationTime).FirstOrDefault();
                        var dsDatas = _csvContext.Read<DSZModel>(dszDataFile.FullName, _csvFileDescription);
                        var dsDatasBySKUs = dsDatas.Where(d =>!string.IsNullOrWhiteSpace( d.SKU)&& skus.Select(s => s.ToLower()).Contains(d.SKU.ToLower()));
                        var dsInventoryThredshold = Convert.ToInt32(ThirdStoreConfig.Instance.DSInventoryThreshold);
                        //var bulkyItemDropshipSKUs = new List<string>();
                        foreach(var data in dsDatasBySKUs)
                        {
                            var invQty = 0;
                            var postage = new List<decimal>() {
                                    data.ACT.IsNumeric()? Convert.ToDecimal(data.ACT):0,
                                    data.NSW_M.IsNumeric()? Convert.ToDecimal(data.NSW_M):0,
                                    data.NSW_R.IsNumeric()? Convert.ToDecimal(data.NSW_R):0,
                                    data.NT_M.IsNumeric()? Convert.ToDecimal(data.NT_M):0,
                                    data.NT_R.IsNumeric()? Convert.ToDecimal(data.NT_R):0,
                                    data.QLD_M.IsNumeric()? Convert.ToDecimal(data.QLD_M):0,
                                    data.QLD_R.IsNumeric()? Convert.ToDecimal(data.QLD_R):0,
                                    data.REMOTE.IsNumeric()? Convert.ToDecimal(data.REMOTE):0,
                                    data.SA_M.IsNumeric()? Convert.ToDecimal(data.SA_M):0,
                                    data.SA_R.IsNumeric()? Convert.ToDecimal(data.SA_R):0,
                                    data.TAS_M.IsNumeric()? Convert.ToDecimal(data.TAS_M):0,
                                    data.TAS_R.IsNumeric()? Convert.ToDecimal(data.TAS_R):0,
                                    data.VIC_M.IsNumeric()? Convert.ToDecimal(data.VIC_M):0,
                                    data.VIC_R.IsNumeric()? Convert.ToDecimal(data.VIC_R):0,
                                    data.WA_M.IsNumeric()? Convert.ToDecimal(data.WA_M):0,
                                    data.WA_R.IsNumeric()? Convert.ToDecimal(data.WA_R):0
                                    }.Max();
                            var bulkyItemPostage = new List<decimal>() {
                                    //data.ACT.IsNumeric()? Convert.ToDecimal(data.ACT):0,
                                    data.NSW_M.IsNumeric()? Convert.ToDecimal(data.NSW_M):0,
                                    //data.NSW_R.IsNumeric()? Convert.ToDecimal(data.NSW_R):0,
                                    //data.NT_M.IsNumeric()? Convert.ToDecimal(data.NT_M):0,
                                    //data.NT_R.IsNumeric()? Convert.ToDecimal(data.NT_R):0,
                                    data.QLD_M.IsNumeric()? Convert.ToDecimal(data.QLD_M):0,
                                    //data.QLD_R.IsNumeric()? Convert.ToDecimal(data.QLD_R):0,
                                    //data.REMOTE.IsNumeric()? Convert.ToDecimal(data.REMOTE):0,
                                    data.SA_M.IsNumeric()? Convert.ToDecimal(data.SA_M):0,
                                    //data.SA_R.IsNumeric()? Convert.ToDecimal(data.SA_R):0,
                                    //data.TAS_M.IsNumeric()? Convert.ToDecimal(data.TAS_M):0,
                                    //data.TAS_R.IsNumeric()? Convert.ToDecimal(data.TAS_R):0,
                                    data.VIC_M.IsNumeric()? Convert.ToDecimal(data.VIC_M):0,
                                    //data.VIC_R.IsNumeric()? Convert.ToDecimal(data.VIC_R):0,
                                    data.WA_M.IsNumeric()? Convert.ToDecimal(data.WA_M):0,
                                    //data.WA_R.IsNumeric()? Convert.ToDecimal(data.WA_R):0
                                    }.Max();
                            var needToLoginDB = false;
                            if (data.InventoryQty >= dsInventoryThredshold && data.Price <= Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceBelow)&& bulkyItemPostage == 0)
                            {
                                invQty = dsInventoryThredshold;
                                if (postage > 0 && bulkyItemPostage == 0)
                                    needToLoginDB = true;
                            }
                            retList.Add(new Tuple<string, int,bool>(data.SKU, invQty, needToLoginDB));
                        }
                        //if(bulkyItemDropshipSKUs.Count>0)
                        //    LogManager.DBLogInstance.Info(bulkyItemDropshipSKUs.Aggregate((current, next) => current + "," + next));
                    }
                }
                return retList;
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public IList<string> GetNeedSyncSKUs()
        {
            try
            {
                List<string> syncSKUs = new List<string>();
                var di = new DirectoryInfo(this.DSDataPath);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles().ToArray();
                    if (files.Count() > 0)
                    {
                        var top2DataFile = files.OrderByDescending(fi => fi.CreationTime).Take(2);
                        var syncDSPriceBelow = Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceBelow);
                        var dsInventoryThredshold = Convert.ToInt32(ThirdStoreConfig.Instance.DSInventoryThreshold);
                        if (top2DataFile.Count() == 2)
                        {
                            var latestDataFile = top2DataFile.First();
                            var secondLatestDataFile = top2DataFile.Last();

                            var latestData = _csvContext.Read<DSZModel>(latestDataFile.FullName, _csvFileDescription);
                            var secondLatestData = _csvContext.Read<DSZModel>(secondLatestDataFile.FullName, _csvFileDescription);

                            var leftJoinResult = from ld in latestData
                                                 join sld in secondLatestData on ld.SKU equals sld.SKU into leftJoin
                                                 from lj in leftJoin.DefaultIfEmpty()
                                                 where lj == null
                                                 ||
                                                 (
                                                 (
                                                    (ld.InventoryQty >= dsInventoryThredshold && lj.InventoryQty < dsInventoryThredshold) || (ld.InventoryQty < dsInventoryThredshold && lj.InventoryQty >= dsInventoryThredshold)|| (ld.Price != lj.Price)||(ld.SpecialPrice!=lj.SpecialPrice)
                                                 ) && (ld.Price <= syncDSPriceBelow || lj.Price <= syncDSPriceBelow))
                                                 select ld.SKU;

                            var rightJoinResult = from sld in secondLatestData
                                                  join ld in latestData on sld.SKU equals ld.SKU into rightJoin
                                                  from rj in rightJoin.DefaultIfEmpty()
                                                  where rj == null
                                                  ||
                                                  (((sld.InventoryQty >= dsInventoryThredshold && rj.InventoryQty < dsInventoryThredshold) || (sld.InventoryQty < dsInventoryThredshold && rj.InventoryQty >= dsInventoryThredshold)
                                                  || (sld.Price != rj.Price)||(sld.SpecialPrice!=rj.SpecialPrice)) && (sld.Price <= syncDSPriceBelow || rj.Price <= syncDSPriceBelow))
                                                  select sld.SKU;

                            syncSKUs = (from sku in leftJoinResult.Union(rightJoinResult).Distinct()
                                           select sku).ToList();

                        }
                        else if (top2DataFile.Count() == 1)
                        {
                            var latestDataFile = top2DataFile.First();
                            var latestData = _csvContext.Read<DSZModel>(latestDataFile.FullName, _csvFileDescription);

                            syncSKUs = (from ld in latestData
                                           where ld.Price <= syncDSPriceBelow
                                           select ld.SKU).ToList();
                        }
                    }
                }

                return syncSKUs;
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        private List<string> GetImageURLList(DSZModel dsData)
        {
            var imagesURL = new List<string>();
            if (!string.IsNullOrEmpty(dsData.Image1))
                imagesURL.Add(dsData.Image1);
            if (!string.IsNullOrEmpty(dsData.Image2))
                imagesURL.Add(dsData.Image2);
            if (!string.IsNullOrEmpty(dsData.Image3))
                imagesURL.Add(dsData.Image3);
            if (!string.IsNullOrEmpty(dsData.Image4))
                imagesURL.Add(dsData.Image4);
            if (!string.IsNullOrEmpty(dsData.Image5))
                imagesURL.Add(dsData.Image5);
            if (!string.IsNullOrEmpty(dsData.Image6))
                imagesURL.Add(dsData.Image6);
            if (!string.IsNullOrEmpty(dsData.Image7))
                imagesURL.Add(dsData.Image7);
            if (!string.IsNullOrEmpty(dsData.Image8))
                imagesURL.Add(dsData.Image8);
            if (!string.IsNullOrEmpty(dsData.Image9))
                imagesURL.Add(dsData.Image9);
            if (!string.IsNullOrEmpty(dsData.Image10))
                imagesURL.Add(dsData.Image10);
            if (!string.IsNullOrEmpty(dsData.Image11))
                imagesURL.Add(dsData.Image11);
            if (!string.IsNullOrEmpty(dsData.Image12))
                imagesURL.Add(dsData.Image12);
            if (!string.IsNullOrEmpty(dsData.Image13))
                imagesURL.Add(dsData.Image13);
            if (!string.IsNullOrEmpty(dsData.Image14))
                imagesURL.Add(dsData.Image14);
            if (!string.IsNullOrEmpty(dsData.Image15))
                imagesURL.Add(dsData.Image15);
            return imagesURL;
        }
    }

    public class SelloDSChannel :IDSChannel
    {
        private readonly CsvContext _csvContext;
        private readonly CsvFileDescription _csvFileDescription;
        private readonly ISettingService _settingService;
        private readonly CommonSettings _commonSetting;
        public SelloDSChannel(
            CsvContext csvContext,
            CsvFileDescription csvFileDescription,
            ISettingService settingService)
        {
            _csvContext = csvContext;
            _csvFileDescription = csvFileDescription;
            _settingService = settingService;
            _commonSetting = _settingService.LoadSetting<CommonSettings>();
        }

        public int DSChannel => ThirdStoreSupplier.S.ToValue();

        protected string DSDataPath => ThirdStoreConfig.Instance.ThirdStoreDSZData + "\\SelloDSData";
        //protected string DSDataURL => "https://idropship.com.au/pub/media/export_product_all_generic.csv";
        protected string DSDataURL => "https://idropship.com.au/pub/media/export_product_all_generic_1.csv";

        public int Order => 1;

        public Stream ExportBatchOrderFile(IList<D_Order_Line> orderLines)
        {
            try
            {
                Stream retStream = new MemoryStream();

                var listSelloImportLine = new List<SelloImportLine>();

                foreach (var line in orderLines)
                {
                    if (string.IsNullOrWhiteSpace(line.Ref5))//item has been allocated and not local stock item
                    {
                        var o = line.Order;
                        var importLine = new SelloImportLine();
                        importLine.serial_number = line.Ref1;
                        var firstName = o.ConsigneeName.Substring(0, o.ConsigneeName.IndexOf(" "));
                        var lastName = o.ConsigneeName.Substring(o.ConsigneeName.IndexOf(" ") + 1);
                        importLine.first_name = firstName;
                        importLine.last_name = lastName;

                        importLine.address1 = o.ShippingAddress1;
                        importLine.address2 = o.ShippingAddress2;
                        importLine.suburb = o.ShippingSuburb;
                        importLine.state = o.ShippingState;
                        importLine.country = o.ShippingCountry;
                        importLine.postcode = o.ShippingPostcode;
                        importLine.telephone = o.ConsigneePhoneNo;
                        importLine.sku = line.SKU;
                        importLine.qty = line.Qty;
                        importLine.comment = o.BuyerNote;

                        listSelloImportLine.Add(importLine);
                    }
                }


                var stWriter = new StreamWriter(retStream);
                _csvContext.Write(listSelloImportLine, stWriter, _csvFileDescription);
                stWriter.Flush();
                retStream.Position = 0;

                return retStream;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public IList<D_Item> GetDSData()
        {
            var retItems = new List<D_Item>();

            try
            {
                IList<SelloDSModel> dsDatas = null;
                using (var webClient = new ThirdStoreWebClient())
                {
                    if (!Directory.Exists(this.DSDataPath))
                        Directory.CreateDirectory(this.DSDataPath);
                    var fileName = this.DSDataPath + "\\" + CommonFunc.ToCSVFileName("SelloDSData");

                    webClient.DownloadFile(DSDataURL, fileName);
                    dsDatas = _csvContext.Read<SelloDSModel>(fileName, _csvFileDescription).ToList();
                    if (dsDatas != null)
                    {
                        foreach (var dsData in dsDatas)
                        {
                            var newItem = new D_Item();
                            if (dsData.sku.Length > 23)
                                LogManager.Instance.Info($"SKU {dsData.sku} length is larger than 23.");

                            newItem.SKU = dsData.sku;
                            newItem.Name = dsData.name;
                            newItem.Description = dsData.description;
                            newItem.Cost = dsData.special_price;//+(!string.IsNullOrEmpty(dsData.NSW_R)?Convert.ToDecimal(dsData.NSW_R):0);
                            newItem.Price = (newItem.Cost) * _commonSetting.DropshipMarkupRate;
                            newItem.Type= ThirdStoreItemType.SINGLE.ToValue();
                            newItem.SupplierID = ThirdStoreSupplier.S.ToValue();
                            newItem.GrossWeight = (!string.IsNullOrWhiteSpace(dsData.weight) ? Convert.ToDecimal(dsData.weight) : 0);
                            newItem.NetWeight = (!string.IsNullOrWhiteSpace(dsData.weight) ? Convert.ToDecimal(dsData.weight) : 0);
                            newItem.Length = (!string.IsNullOrWhiteSpace(dsData.length) ? Convert.ToDecimal(dsData.length) / 100 : 0);
                            newItem.Width = (!string.IsNullOrWhiteSpace(dsData.width) ? Convert.ToDecimal(dsData.width) / 100 : 0);
                            newItem.Height = (!string.IsNullOrWhiteSpace(dsData.height) ? Convert.ToDecimal(dsData.height) / 100 : 0);
                            newItem.Ref3 = dsData.UPC?.Trim();
                            newItem.IsReadyForList = (dsData.sku.Length <= 23 && dsData.special_price > Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceAbove) ? true : false);
                            newItem.IsActive = true;

                            var imagesURL = GetImageURLList(dsData);
                            if(imagesURL.Count>0)
                                newItem.Ref5=imagesURL.Aggregate((current, next) => current + ";" + next);

                            retItems.Add(newItem);
                        }
                    }
                }

                return retItems;
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        private List<string> GetImageURLList(SelloDSModel dsData)
        {
            var imagesURL = new List<string>();
            if(!string.IsNullOrWhiteSpace(dsData.main_image))
                imagesURL.Add(dsData.main_image);
            if(!string.IsNullOrWhiteSpace(dsData.additional_images))
            {
                var additional = dsData.additional_images.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                imagesURL.AddRange(additional);
            }
            return imagesURL;
        }

        public IList<Tuple<string, IList<string>>> GetImagesPathsBySKUs(IList<string> skus)
        {
            try
            {
                var retList = new List<Tuple<string, IList<string>>>();
                if (skus == null || skus.Count == 0)
                    return retList;
                var di = new DirectoryInfo(DSDataPath);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles().ToArray();
                    if (files.Count() > 0)
                    {
                        var topDataFile = files.OrderByDescending(fi => fi.CreationTime).FirstOrDefault();
                        var dsDatas = _csvContext.Read<SelloDSModel>(topDataFile.FullName, _csvFileDescription);
                        var matchedDSDatas = dsDatas.Where(d=>skus.Contains(d.sku));
                        foreach (var dsData in matchedDSDatas)
                        {
                            if (dsData != null)
                            {
                                var imagesURL = GetImageURLList(dsData);
                                retList.Add(new Tuple<string, IList<string>>(dsData.sku, imagesURL));
                            }
                        }
                    }
                }

                return retList;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public IList<string> GetNeedSyncSKUs()
        {
            try
            {
                List<string> syncSKUs = new List<string>();
                var di = new DirectoryInfo(this.DSDataPath);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles().ToArray();
                    if (files.Count() > 0)
                    {
                        var top2DataFile = files.OrderByDescending(fi => fi.CreationTime).Take(2);
                        var syncDSPriceAbove = Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceAbove);
                        var dsInventoryThredshold = Convert.ToInt32(ThirdStoreConfig.Instance.DSInventoryThreshold);
                        if (top2DataFile.Count() == 2)
                        {
                            var latestDataFile = top2DataFile.First();
                            var secondLatestDataFile = top2DataFile.Last();

                            var latestData = _csvContext.Read<SelloDSModel>(latestDataFile.FullName, _csvFileDescription);
                            var secondLatestData = _csvContext.Read<SelloDSModel>(secondLatestDataFile.FullName, _csvFileDescription);

                            var leftJoinResult = from ld in latestData
                                                 join sld in secondLatestData on ld.sku equals sld.sku into leftJoin
                                                 from lj in leftJoin.DefaultIfEmpty()
                                                 where lj == null
                                                 ||
                                                 ((((ld.qty >= dsInventoryThredshold && lj.qty < dsInventoryThredshold) || (ld.qty < dsInventoryThredshold && lj.qty >= dsInventoryThredshold))
                                                 || (ld.special_price != lj.special_price)) && (ld.special_price > syncDSPriceAbove || lj.special_price > syncDSPriceAbove))
                                                 select ld.sku;

                            var rightJoinResult = from sld in secondLatestData
                                                  join ld in latestData on sld.sku equals ld.sku into rightJoin
                                                  from rj in rightJoin.DefaultIfEmpty()
                                                  where rj == null
                                                  ||
                                                  ((((sld.qty >= dsInventoryThredshold && rj.qty < dsInventoryThredshold) || (sld.qty < dsInventoryThredshold && rj.qty >= dsInventoryThredshold))
                                                  || (sld.special_price != rj.special_price)) && (sld.special_price > syncDSPriceAbove || rj.special_price > syncDSPriceAbove))
                                                  select sld.sku;

                            syncSKUs = (from sku in leftJoinResult.Union(rightJoinResult).Distinct()
                                        select sku).ToList();
                        }
                        else if (top2DataFile.Count() == 1)
                        {
                            var latestDataFile = top2DataFile.First();
                            var latestData = _csvContext.Read<SelloDSModel>(latestDataFile.FullName, _csvFileDescription);

                            syncSKUs = (from ld in latestData
                                        where ld.special_price > syncDSPriceAbove
                                        select ld.sku).ToList();
                        }
                    }
                }
                return syncSKUs;
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public IList<Tuple<string, int,bool>> GetInventoryQtyBySKUs(IList<string> skus)
        {
            try
            {
                var retList = new List<Tuple<string, int,bool>>();
                var di = new DirectoryInfo(DSDataPath);
                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles().ToArray();
                    if (files.Count() > 0)
                    {
                        var dszDataFile = files.OrderByDescending(fi => fi.CreationTime).FirstOrDefault();
                        var dsDatas = _csvContext.Read<SelloDSModel>(dszDataFile.FullName, _csvFileDescription);
                        var dsDatasBySKUs = dsDatas.Where(d => skus.Select(s => s.ToLower()).Contains(d.sku.ToLower()));
                        var dsInventoryThredshold = Convert.ToInt32(ThirdStoreConfig.Instance.DSInventoryThreshold);
                        foreach (var data in dsDatasBySKUs)
                        {
                            var invQty = 0;
                            var shippingClassLevel = (!string.IsNullOrWhiteSpace(data.ShippingClass) ? Convert.ToInt32(Regex.Match(data.ShippingClass, @"\d+").Value) : 999);
                            if (data.qty >= dsInventoryThredshold && data.special_price >Convert.ToDecimal(ThirdStoreConfig.Instance.SyncDSPriceAbove)
                                && shippingClassLevel<=3
                                //&&!string.IsNullOrWhiteSpace(data.shipping_operation)&& data.shipping_operation== "0"
                                )
                            {
                                invQty = dsInventoryThredshold;
                            }
                            retList.Add(new Tuple<string, int,bool>(data.sku, invQty,false));
                        }
                    }
                }
                return retList;
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }
    }

}
