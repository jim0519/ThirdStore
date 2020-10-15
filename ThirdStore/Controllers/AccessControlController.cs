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
using ThirdStoreFramework.Kendoui;

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

        public ActionResult UserList()
        {

            var model = new UserListViewModel();

            model.YesOrNo = YesNo.Y.ToSelectList(false).ToList();
            model.YesOrNo.Insert(0, new SelectListItem { Text = "", Value = "-1", Selected = true });
            model.SearchStatus = -1;

            var allowAccessUserList = new int[] { 1 };
            if (!allowAccessUserList.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult UserList(DataSourceRequest command, UserListViewModel model)
        {
            var users = _userService.SearchUsers(
                name:model.SearchName,
                description:model.SearchDescription,
                email:model.SearchEmail,
                status:model.SearchStatus,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var userGridViewList = users.Select(i => i.ToModel());

            var gridModel = new DataSourceResult() { Data = userGridViewList, Total = users.TotalCount };
            //return View();
            return new JsonResult
            {
                Data = gridModel
            };
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

            var newItemViewModel = new UserViewModel() { StatusID = 1 };

            FillDropDownDS(newItemViewModel);

            return View(newItemViewModel);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentTime = DateTime.Now;
                var currentUser = Constants.SystemUser;
                if (_workContext.CurrentUser != null)
                    currentUser = _workContext.CurrentUser.Email;
                
                var newEntityModel = model.ToCreateNewEntity().FillOutNull();
                newEntityModel.CreateTime = currentTime;
                newEntityModel.CreateBy = currentUser;
                newEntityModel.EditBy = currentUser;
                newEntityModel.EditTime = currentTime;
                _userService.RegisterUser(newEntityModel);

                _userService.SignIn(model.Email, model.Password);
                return Redirect("~/");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Edit(int userID)
        {
            var allowEditNewUserUserIDs = new int[] { 1 };
            if (!allowEditNewUserUserIDs.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }

            var editUserViewModel = new UserViewModel();
            var user = _userService.GetUserByID(userID);
            if (user != null)
            {
                editUserViewModel = user.ToCreateNewModel();
            }


            FillDropDownDS(editUserViewModel);

            return View(editUserViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserViewModel model)
        {
            var allowEditNewUserUserIDs = new int[] { 1 };
            if (!allowEditNewUserUserIDs.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }
            FillDropDownDS(model);

            if (!ModelState.IsValid)
            {
                var errMsg = ModelState.Values.SelectMany(v => v.Errors.Select(er => er.ErrorMessage)).Aggregate((current, next) => current + Environment.NewLine + next);
                ErrorNotification(errMsg);
                return View(model);
            }

            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
                currentUser = _workContext.CurrentUser.Email;
            
            var editEntityModel = _userService.GetUserByID(model.ID);
            editEntityModel = model.ToCreateNewEntity(editEntityModel).FillOutNull();
            editEntityModel.EditBy = currentUser;
            editEntityModel.EditTime = currentTime;
            _userService.UpdateUser(editEntityModel);

            return RedirectToAction("List");

            //return RedirectToAction("Edit", new { userID = editEntityModel.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _userService.SignOut();

            return RedirectToAction("Login", "AccessControl");
        }


        private void FillDropDownDS(UserViewModel model)
        {
            model.YesOrNo = YesNo.Y.ToSelectList(false).ToList();
        }
    }
}