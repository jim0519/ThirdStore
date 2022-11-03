using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using ThirdStore.Validators.JobItem;

namespace ThirdStore.Models.JobItem
{
    [Validator(typeof(JobItemViewValidator))]
    public class JobItemViewModel : BaseEntityViewModel
    {
        public JobItemViewModel()
        {
            this.JobItemViewLines = new List<JobItemLineViewModel>();
            this.JobItemViewImages = new List<JobItemImageViewModel>();
        }

        //[UIHint("DateTimeNullable")]
        //public DateTime JobItemCreateTime { get; set; }
        public int Type { get; set; }
        public int StatusID { get; set; }
        public int ConditionID { get; set; }
        public string Location { get; set; }
        public string ItemName { get; set; }
        public string ItemDetail { get; set; }
        public decimal ItemPrice { get; set; }
        //[Remote("CheckDesignatedSKU","JobItem", HttpMethod = "POST")]
        public string DesignatedSKU { get; set; }
        [UIHint("DateTimeNullable")]
        public DateTime? ShipTime { get; set; }
        public string TrackingNumber { get; set; }
        
        //public string Ref1 { get; set; }
        [Display(Name = "Inspectors")]
        [UIHint("MultiSelectString")]
        public List<string> Ref2 { get; set; }
        [Display(Name = "Sequence#")]
        public string Reference { get; set; }
        public string Note { get; set; }
        //[RegularExpression(@"^[0-1]\.\d{1,2}$", ErrorMessage = "Percentage only can be decimal and 2 decimal places")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public decimal PricePercentage { get; set; } = 1;
        public string ReviewComments { get; set; }
        public bool NeedReview { get; set; }


        public IList<SelectListItem> JobItemTypes { get; set; }
        public IList<SelectListItem> JobItemConditions { get; set; }
        public IList<SelectListItem> JobItemStatuses { get; set; }
        public IList<SelectListItem> InspectorList { get; set; }
        public IList<JobItemLineViewModel> JobItemViewLines { get; set; }
        public IList<JobItemImageViewModel> JobItemViewImages { get; set; }

        public class JobItemLineViewModel : BaseEntityViewModel
        {
            public int ItemID { get; set; }
            public string SKU { get; set; }
            public int Qty { get; set; }
            public decimal Weight { get; set; }
            public decimal Length { get; set; }
            public decimal Width { get; set; }
            public decimal Height { get; set; }
            public decimal CubicWeight { get; set; }
            public string Ref1 { get; set; }
        }

        public class JobItemImageViewModel : BaseEntityViewModel
        {
            public int ImageID { get; set; }
            public string ImageURL { get; set; }
            public string ImageName { get; set; }
            public int DisplayOrder { get; set; }
            public bool StatusID { get; set; }
        }


    }
}