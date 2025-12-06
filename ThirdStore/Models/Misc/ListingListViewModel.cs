using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ThirdStore.Models.Misc
{
    public class ListingListViewModel : BaseViewModel
    {
        public ListingListViewModel()
        {
            this.ListingStatuses = new List<SelectListItem>();
        }

        public string SearchSKU { get; set; }
        public string SearchTitle { get; set; }
        public string SearchItemID { get; set; }
        public int SearchIsAuto { get; set; }

        public IList<SelectListItem> YesOrNo { get; set; }
        public IList<SelectListItem> ListingStatuses { get; set; }
    }
}