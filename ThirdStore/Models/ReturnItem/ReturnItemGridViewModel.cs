using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Models.ReturnItem
{
    public class ReturnItemGridViewModel : BaseEntityViewModel
    {
        public string Status { get; set; }
        public string TrackingNumber { get; set; }
        public string Note { get; set; }
        public string SKUs { get; set; }
        public DateTime CreateTime { get; set; }
    }
}