using ThirdStoreFramework.MVC;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ThirdStore.Models.AccessControl
{
    public class UserViewModel : BaseEntityViewModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        public int StatusID { get; set; }

        public IList<SelectListItem> YesOrNo { get; set; }
    }
    
}
