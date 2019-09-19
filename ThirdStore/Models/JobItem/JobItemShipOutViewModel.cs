using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Models.JobItem
{
    public class JobItemShipOutViewModel : BaseViewModel
    {
        public string JobItemLineID { get; set; }
        public string JobItemLineReference { get; set; }
        public string TrackingNumber { get; set; }
    }
}