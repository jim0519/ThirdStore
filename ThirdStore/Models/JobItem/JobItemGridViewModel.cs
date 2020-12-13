using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Models.JobItem
{
    public class JobItemGridViewModel : BaseEntityViewModel
    {
        public DateTime JobItemCreateTime { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Condition { get; set; }
        public string Supplier { get; set; }
        public decimal ItemPrice { get; set; }
        public DateTime? ShipTime { get; set; }
        public string TrackingNumber { get; set; }
        public string SKUs { get; set; }
        public string Reference { get; set; }
        public string Location { get; set; }
        public string ItemDetail { get; set; }
        public string Ref2 { get; set; }
        public DateTime? StocktakeTime { get; set; }
        public decimal CBM { get; set; }
        public System.DateTime EditTime { get; set; }
        public string EditBy { get; set; }
    }
}