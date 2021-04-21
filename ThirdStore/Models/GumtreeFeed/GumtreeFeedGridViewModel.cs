using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Models.GumtreeFeed
{
    public class GumtreeFeedGridViewModel : BaseEntityViewModel
    {
        public string SKU { get; set; }
        public string JobItemIDs { get; set; }
        public string References { get; set; }
        public string Condition { get; set; }
        public decimal Price { get; set; }
        public DateTime CreateTime { get; set; }
        public string Title { get; set; }

        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string Image4 { get; set; }
    }
}