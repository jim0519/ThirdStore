using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Models.Misc
{
    public class ListingGridViewModel : BaseEntityViewModel
    {
        public string ListingSKU { get; set; }
        public int ItemID { get; set; }
        public string ListingTitle { get; set; }
        public decimal ListingPrice { get; set; }
        public int ListingInventoryQty { get; set; }
        public string ListingStatusID { get; set; }
        public string ListingID { get; set; }
        public string IsAuto { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}