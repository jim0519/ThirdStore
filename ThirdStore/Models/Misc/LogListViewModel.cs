using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ThirdStore.Models.Misc
{
    public class LogListViewModel : BaseViewModel
    {
        public LogListViewModel()
        {
            
        }

        public string SearchMessage { get; set; }

        [UIHint("DateNullable"), Display(Name = "Log Time From")]
        public DateTime? LogTimeFrom { get; set; }

        [Display(Name = "Log Time To"), UIHint("DateNullable")]
        public DateTime? LogTimeTo { get; set; }

    }
}