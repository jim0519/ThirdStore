using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;

namespace ThirdStore.Models.Item
{
    public class ItemListViewModel : BaseViewModel
    {
        public ItemListViewModel()
        {
            this.SearchTypes = new List<SelectListItem>();
            this.Suppliers = new List<SelectListItem>();
            this.YesOrNo = new List<SelectListItem>();
            this.BulkUpdate = new BulkUpdateItemModel();
        }

        public string SearchSKU { get; set; }
        public int SearchType { get; set; }
        public string SearchName { get; set; }
        public int SearchSupplier { get; set; }
        public int SearchReadyForList { get; set; }
        public bool ShowSyncInventory { get; set; }

        public IList<SelectListItem> Suppliers { get; set; }
        public IList<SelectListItem> SearchTypes { get; set; }
        public IList<SelectListItem> YesOrNo { get; set; }

        public BulkUpdateItemModel BulkUpdate { get; set; }
        public class BulkUpdateItemModel
        {
            public int IsActive { get; set; }
            public int IsReadyForList { get; set; }
        }
    }
}