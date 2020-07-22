using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ThirdStore.Models.Order
{
    public class OrderListViewModel : BaseViewModel
    {
        public OrderListViewModel()
        {
            this.OrderStatuses = new List<SelectListItem>();
        }

        [UIHint("DateTimeNullable"), Display(Name = "Update Time From")]
        public DateTime? UpdateOrderFrom { get; set; }

        [Display(Name = "Update Time To"), UIHint("DateTimeNullable")]
        public DateTime? UpdateOrderTo { get; set; }

        [Display(Name = "Job Item ID")]
        public string SearchJobItemID { get; set; }
        [Display(Name = "Neto Order ID")]
        public string SearchChannelOrderID { get; set; }
        [UIHint("DateTimeNullable"), Display(Name = "Order Time From")]
        public DateTime? OrderTimeFrom { get; set; }

        [Display(Name = "Order Time To"), UIHint("DateTimeNullable")]
        public DateTime? OrderTimeTo { get; set; }

        public string SearchCustomerID { get; set; }
        public string SearchConsigneeName { get; set; }
        public int SearchStatusID { get; set; }



        public IList<SelectListItem> OrderStatuses { get; set; }

        public bool CanEditOrder { get; set; }


    }
}