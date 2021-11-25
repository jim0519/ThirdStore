using ThirdStoreFramework.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace ThirdStore.Models.ReturnItem
{
    public class ReturnItemViewModel : BaseEntityViewModel
    {
        public ReturnItemViewModel()
        {
            this.ReturnItemViewLines = new List<ReturnItemLineViewModel>();
        }

        public int StatusID { get; set; }
        public string TrackingNumber { get; set; }
        public string Note { get; set; }
        public string DesignatedSKU { get; set; }

        //public System.DateTime CreateTime { get; set; }
        //public string CreateBy { get; set; }
        //public System.DateTime EditTime { get; set; }
        //public string EditBy { get; set; }

        public IList<SelectListItem> ReturnItemStatuses { get; set; }

        public IList<ReturnItemLineViewModel> ReturnItemViewLines { get; set; }
        public class ReturnItemLineViewModel : BaseEntityViewModel
        {
            public int ItemID { get; set; }
            public string SKU { get; set; }
            public int Qty { get; set; }
            public decimal Weight { get; set; }
            public decimal Length { get; set; }
            public decimal Width { get; set; }
            public decimal Height { get; set; }
            public decimal CubicWeight { get; set; }
            public string Location { get; set; }
            public string Ref1 { get; set; }
        }
    }
}
