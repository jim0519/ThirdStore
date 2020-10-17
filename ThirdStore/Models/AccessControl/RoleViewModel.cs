using ThirdStoreFramework.MVC;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ThirdStore.Models.AccessControl
{
    public class RoleViewModel : BaseEntityViewModel
    {
        [Required]
        [Display(Name = "Role Name")]
        public string Name { get; set; }

        public string Description { get; set; }

       

        public int IsActive { get; set; }

        [UIHint("MultiSelect")]
        public List<int> Permissions { get; set; }

        public IList<SelectListItem> YesOrNo { get; set; }

        public IList<SelectListItem> AllPermissions { get; set; }


    }
    
}
