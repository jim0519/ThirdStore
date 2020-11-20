using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ThirdStore.Models.AccessControl
{
    public class RoleListViewModel : BaseViewModel
    {
        public RoleListViewModel()
        {
            this.YesOrNo = new List<SelectListItem>();
        }


        public IList<SelectListItem> YesOrNo { get; set; }

        
    }
}