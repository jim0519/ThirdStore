using ThirdStoreFramework.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

namespace ThirdStore.Models.Misc
{
    public class GumtreeFeedViewModel : BaseViewModel
    {
        public GumtreeFeedViewModel()
        {
            
        }

        public string SKU { get; set; }

        
    }
}
