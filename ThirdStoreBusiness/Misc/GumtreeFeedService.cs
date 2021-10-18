
using System;
using System.IO;
using System.Drawing;
using ThirdStoreData;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;
using System.Linq;
using LINQtoCSV;
using System.Text;
using ThirdStoreBusiness.JobItem;
using System.Collections.Generic;
using Ionic.Zip;
using ThirdStoreBusiness.Image;

namespace ThirdStoreBusiness.Misc
{
    public class GumtreeFeedService : IGumtreeFeedService
    {
        private readonly CsvContext _csvContext;
        private readonly IWorkContext _workContext;
        private readonly IImageService _imageService;

        public GumtreeFeedService(
            IWorkContext workContext,
            CsvContext csvContext,
            IImageService imageService)
        {            
            _workContext = workContext;
            _csvContext = csvContext;
            _imageService = imageService;
        }

        public Stream ExportImages(IList<JobItem.GumtreeFeed> feeds)
        {
            try
            {
                MemoryStream zipStream = new MemoryStream();

                using (var zip = new ZipFile())
                {
                    foreach(var feed in feeds)
                    {
                        MemoryStream skuZipStream = new MemoryStream();
                        using (var skuZip = new ZipFile())
                        {
                            if (!string.IsNullOrWhiteSpace(feed.Image1))
                            {
                                skuZip.AddEntry(feed.SKU + "_01.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image1)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image2))
                            {
                                skuZip.AddEntry(feed.SKU + "_02.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image2)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image3))
                            {
                                skuZip.AddEntry(feed.SKU + "_03.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image3)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image4))
                            {
                                skuZip.AddEntry(feed.SKU + "_04.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image4)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image5))
                            {
                                skuZip.AddEntry(feed.SKU + "_05.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image5)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image6))
                            {
                                skuZip.AddEntry(feed.SKU + "_06.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image6)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image7))
                            {
                                skuZip.AddEntry(feed.SKU + "_07.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image7)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image8))
                            {
                                skuZip.AddEntry(feed.SKU + "_08.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image8)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image9))
                            {
                                skuZip.AddEntry(feed.SKU + "_09.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image9)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image10))
                            {
                                skuZip.AddEntry(feed.SKU + "_10.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image10)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image11))
                            {
                                skuZip.AddEntry(feed.SKU + "_11.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image11)));
                            }

                            if (!string.IsNullOrWhiteSpace(feed.Image12))
                            {
                                skuZip.AddEntry(feed.SKU + "_12.jpg", File.ReadAllBytes(_imageService.GetLocalImagePathByURL(feed.Image12)));
                            }

                            skuZip.Save(skuZipStream);
                            skuZipStream.Position = 0;

                            zip.AddEntry(feed.SKU + "_" + feed.Condition + ".zip", skuZipStream);
                        }
                    }

                    zip.Save(zipStream);
                    zipStream.Position = 0;
                }
                return zipStream;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                throw ex;
            }
        }

        public Stream RedownloadGumtreeFeed()
        {
            throw new NotImplementedException();
        }

        public IPagedList<JobItem.GumtreeFeed> SearchGumtreeFeeds(string sku = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var csvFileDescription = new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true, TextEncoding = Encoding.Default };
            var di = new DirectoryInfo(ThirdStoreConfig.Instance.GumtreeFeedPath);
            if (!di.Exists)
                return default(IPagedList<JobItem.GumtreeFeed>);

            var lastestFile = di.GetFiles().OrderByDescending(fi => fi.CreationTime).FirstOrDefault();

            var gumtreeFeeds = _csvContext.Read<JobItem.GumtreeFeed>(lastestFile.FullName,csvFileDescription);

            if (sku != null)
                gumtreeFeeds = gumtreeFeeds.Where(fd=>fd.SKU.ToLower().Equals(sku.ToLower()));

            return new PagedList<JobItem.GumtreeFeed>(gumtreeFeeds, pageIndex, pageSize, gumtreeFeeds.Count());
        }
    }
}
