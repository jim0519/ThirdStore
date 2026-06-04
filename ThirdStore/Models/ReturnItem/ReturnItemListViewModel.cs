using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ThirdStore.Models.ReturnItem
{
    public class ReturnItemListViewModel : BaseViewModel
    {
        public ReturnItemListViewModel()
        {
            this.ReturnItemTypes = new List<SelectListItem>();
        }

        //public int SearchStatus { get; set; }

        public string SearchTrackingNumber { get; set; }

        public string SearchSKU { get; set; }

        public IList<SelectListItem> ReturnItemTypes { get; set; }

    }
}