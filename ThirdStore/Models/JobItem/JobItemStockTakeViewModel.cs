using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Models.JobItem
{
    public class JobItemStockTakeViewModel : BaseViewModel
    {
        public string JobItemLineID { get; set; }
        public string JobItemLineReference { get; set; }
        public string Location { get; set; }
        public string Ref4 { get; set; }
        [UIHint("DateNullable"), Display(Name = "Stocktake Time From")]
        public DateTime? StocktakeTimeFrom { get; set; }
        [Display(Name = "Stocktake Time To"), UIHint("DateNullable")]
        public DateTime? StocktakeTimeTo { get; set; }
    }
}