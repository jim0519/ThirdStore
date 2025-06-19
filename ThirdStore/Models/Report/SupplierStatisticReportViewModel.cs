using ThirdStoreFramework.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace ThirdStore.Models.Report
{
    public class SupplierStatisticReportViewModel : BaseViewModel
    {
        public SupplierStatisticReportViewModel()
        {

        }

        public int SupplierID { get; set; }

        [UIHint("DateNullable"), Display(Name = "Create Time From")]
        public DateTime? CreateTimeFrom { get; set; }

        [Display(Name = "Create Time To"), UIHint("DateNullable")]
        public DateTime? CreateTimeTo { get; set; }

        public IList<SelectListItem> Suppliers { get; set; }


    }
}
