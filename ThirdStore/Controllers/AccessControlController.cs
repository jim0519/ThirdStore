using ThirdStore.Models.AccessControl;
using ThirdStoreFramework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStoreBusiness.AccessControl;
using ThirdStore.Extensions;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;

namespace ThirdStore.Controllers
{
    public class AccessControlController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;

        public AccessControlController(IUserService userService,
            IWorkContext workContext)
        {
            _userService = userService;
            _workContext = workContext;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            //ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnURL)
        {
            if (ModelState.IsValid)
            {
                _userService.SignIn(model.Email, model.Password);
            }

            if (String.IsNullOrEmpty(returnURL) || !Url.IsLocalUrl(returnURL))
                return Redirect("~/");
            //return RedirectToAction("List", "SalesOrder");

            return Redirect(returnURL);

        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult CreateUser()
        {
            var allowCreateNewUserUserIDs = new int[] { 1 };
            if (!allowCreateNewUserUserIDs.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var createTime = DateTime.Now;
                var createBy = Constants.SystemUser;
                var newEntityModel = model.ToCreateNewEntity().FillOutNull();
                newEntityModel.CreateTime = createTime;
                newEntityModel.CreateBy = createBy;
                newEntityModel.EditTime = createTime;
                newEntityModel.EditBy = createBy;
                _userService.RegisterUser(newEntityModel);

                _userService.SignIn(model.Email, model.Password);
                return Redirect("~/");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _userService.SignOut();

            return RedirectToAction("Login", "AccessControl");
        }
    }
}