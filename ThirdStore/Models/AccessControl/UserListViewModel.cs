using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThirdStoreFramework.MVC;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

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

        public ChangePasswordViewModel ChangePasswordModel { get; set; }

        public class ChangePasswordViewModel
        {

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

        }
    }
}