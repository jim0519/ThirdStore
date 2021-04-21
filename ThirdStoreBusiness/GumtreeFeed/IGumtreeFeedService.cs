using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon;
using ThirdStoreBusiness.JobItem;

namespace ThirdStoreBusiness.GumtreeFeed
{
    public interface IGumtreeFeedService
    {
        IPagedList<JobItem.GumtreeFeed> SearchGumtreeFeeds(string sku = null,
             
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        Stream ExportImages(IList<JobItem.GumtreeFeed> feeds);

        Stream RedownloadGumtreeFeed();
    }
}
