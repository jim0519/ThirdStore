using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;

namespace ThirdStore.Models.AccessControl
{
    public class UserGridViewModel : BaseEntityViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int StatusID { get; set; }
        public string Email { get; set; }
    }
}