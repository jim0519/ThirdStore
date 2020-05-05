using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ThirdStore.Models.JobItem
{
    public class JobItemListViewModel : BaseViewModel
    {
        public JobItemListViewModel()
        {
            this.JobItemTypes = new List<SelectListItem>();
            this.Suppliers = new List<SelectListItem>();
            this.JobItemConditions = new List<SelectListItem>();
            this.JobItemStatuses = new List<SelectListItem>();
            this.YesOrNo = new List<SelectListItem>();
        }

        [UIHint("DateNullable"), Display(Name = "Affect Time From")]
        public DateTime? AffectTimeFrom { get; set; }
        [Display(Name = "Affect Time To"), UIHint("DateNullable")]
        public DateTime? AffectTimeTo { get; set; }
        [UIHint("DateNullable"), Display(Name = "Create Time From")]
        public DateTime? CreateTimeFrom { get; set; }
        [Display(Name = "Create Time To"), UIHint("DateNullable")]
        public DateTime? CreateTimeTo { get; set; }
        public string SearchID { get; set; }
        public string SearchSKU { get; set; }
        public int SearchType { get; set; }
        public int SearchCondition { get; set; }
        public int SearchSupplier { get; set; }
        public int SearchStatus { get; set; }
        [Display(Name = "Exclude Shipped")]
        public bool IsExcludeShippedStatus { get; set; }
        public string SearchLocation { get; set; }
        public string SearchTrackingNumber { get; set; }
        public string SearchReference { get; set; }
        [UIHint("MultiSelectString")]
        public List<string> SearchInspector { get; set; }
        public int HasStocktakeTime { get; set; }
        public bool ShowSyncInventory { get; set; }

        public IList<SelectListItem> Suppliers { get; set; }
        public IList<SelectListItem> JobItemTypes { get; set; }
        public IList<SelectListItem> JobItemConditions { get; set; }
        public IList<SelectListItem> JobItemStatuses { get; set; }
        public IList<SelectListItem> InspectorList { get; set; }
        public IList<SelectListItem> YesOrNo { get; set; }

        public BulkUpdateJobItemModel BulkUpdate { get; set; }
        public class BulkUpdateJobItemModel
        {
            public int StatusID { get; set; }
            public string Location { get; set; }
            public decimal ItemPrice { get; set; }
            [RegularExpression(@"^[0-1]\.\d{1,2}$", ErrorMessage = "Percentage only can be decimal and 2 decimal places")]
            [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
            public decimal PricePercentage { get; set; } = 0;
        }
    }
}