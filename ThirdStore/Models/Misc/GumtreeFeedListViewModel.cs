using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;

namespace ThirdStore.Models.Misc
{
    public class GumtreeFeedListViewModel : BaseViewModel
    {
        public GumtreeFeedListViewModel()
        {
            
        }

        public string SearchSKU { get; set; }
        
    }
}