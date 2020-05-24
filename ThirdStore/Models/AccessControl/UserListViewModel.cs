using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;

namespace ThirdStore.Models.AccessControl
{
    public class UserListViewModel : BaseViewModel
    {
        public UserListViewModel()
        {
            this.YesOrNo = new List<SelectListItem>();
        }

        public string SearchName { get; set; }
        public string SearchDescription { get; set; }
        public string SearchEmail { get; set; }
        public int SearchStatus { get; set; }

        public IList<SelectListItem> YesOrNo { get; set; }

    }
}