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
using ThirdStoreCommon.Models.AccessControl;

namespace ThirdStore.Controllers
{
    public class AccessControlController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        public AccessControlController(IUserService userService,
            IWorkContext workContext,
            IRoleService roleService,
            IPermissionService permissionService)
        {
            _userService = userService;
            _workContext = workContext;
            _roleService = roleService;
            _permissionService = permissionService;
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

        public ActionResult RoleList()
        {

            var model = new RoleListViewModel();

            model.YesOrNo = YesNo.Y.ToSelectList(false).ToList();
            model.YesOrNo.Insert(0, new SelectListItem { Text = "", Value = "-1", Selected = true });

            var allowAccessUserList = new int[] { 1 };
            if (!allowAccessUserList.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult RoleList(DataSourceRequest command)
        {
            var roles = _roleService.SearchRoles(
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);

            var roleGridViewList = roles.Select(i => i.ToModel());

            var gridModel = new DataSourceResult() { Data = roleGridViewList, Total = roles.TotalCount };
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

        public ActionResult EditUser(int userID)
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
        public ActionResult EditUser(UserViewModel model)
        {

            var allowEditNewUserUserIDs = new int[] { 1 };
            if (!allowEditNewUserUserIDs.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }
            FillDropDownDS(model);
            //if (!ModelState.IsValid)
            //{
            //    var errMsg = ModelState.Values.SelectMany(v => v.Errors.Select(er => er.ErrorMessage)).Aggregate((current, next) => current + Environment.NewLine + next);
            //    ErrorNotification(errMsg);
            //    return View(model);
            //}

            var currentTime = DateTime.Now;
            var currentUser = Constants.SystemUser;
            if (_workContext.CurrentUser != null)
                currentUser = _workContext.CurrentUser.Email;

            var editEntityModel = _userService.GetUserByID(model.ID);
            editEntityModel = model.ToCreateNewEntity(editEntityModel).FillOutNull();
            editEntityModel.EditBy = currentUser;
            editEntityModel.EditTime = currentTime;

            //foreach(var roleID in model.Roles)
            //{
            //    //add role if not exists
            //    if(!editEntityModel.UserRoles.Any(r=>r.ID.Equals(roleID)))
            //    {
            //        editEntityModel.UserRoles.Add(_roleService.GetRoleByID(roleID));
            //    }

            //    //delete role if remove

            //}

            //rolesToAdd
            //var roleIDsToAdd = model.Roles.Select(r=>Convert.ToInt32(r)).Except(editEntityModel.UserRoles.Select(ur => ur.RoleID)); 
            //var roleIDsToDelete = editEntityModel.UserRoles.Select(ur => ur.RoleID).Except(model.Roles.Select(r => Convert.ToInt32(r)));
            //var rolesToDelete = editEntityModel.UserRoles.Where(ur => roleIDsToDelete.Contains(ur.RoleID));
            if (model.Roles == null)
                model.Roles = new List<int>();
            var roleIDsToAdd = model.Roles.Except(editEntityModel.UserRoles.Select(ur => ur.RoleID)).ToList();
            var roleIDsToDelete = editEntityModel.UserRoles.Select(ur => ur.RoleID).Except(model.Roles).ToList();
            foreach (var roleIDToAdd in roleIDsToAdd)
            {
                editEntityModel.UserRoles.Add(new M_UserRole() { UserID = model.ID, RoleID = roleIDToAdd, CreateBy = currentUser, CreateTime = currentTime, EditBy = currentUser, EditTime = currentTime });
            }

            foreach (var roleIDToDelete in roleIDsToDelete)
            {
                _userService.RemoveUserRole(editEntityModel.UserRoles.FirstOrDefault(ur => ur.RoleID == roleIDToDelete));
            }

            _userService.UpdateUser(editEntityModel);

            return RedirectToAction("UserList");


            //return RedirectToAction("Edit", new { userID = editEntityModel.ID });
        }


        [HttpPost]
        public ActionResult ChangePassword(UserListViewModel.ChangePasswordViewModel changePasswordModel,string userID)
        {
            try
            {
                var allowEditNewUserUserIDs = new int[] { 1 };
                if (!allowEditNewUserUserIDs.Contains(_workContext.CurrentUser.ID))
                {
                    ErrorNotification("You do not have permission to process this page.");
                    return Redirect("~/"); ;
                }

                //if (!ModelState.IsValid)
                //{
                //    var errMsg = ModelState.Values.SelectMany(v => v.Errors.Select(er => er.ErrorMessage)).Aggregate((current, next) => current + Environment.NewLine + next);
                //    ErrorNotification(errMsg);
                //    return View(model);
                //}

                var currentTime = DateTime.Now;
                var currentUser = Constants.SystemUser;
                if (_workContext.CurrentUser != null)
                    currentUser = _workContext.CurrentUser.Email;

                var userEntityModel = _userService.GetUserByID(Convert.ToInt32(userID));
                userEntityModel.Password = changePasswordModel.Password;
                _userService.EncryptPassword(userEntityModel);
                userEntityModel.EditBy = currentUser;
                userEntityModel.EditTime = currentTime;
                _userService.UpdateUser(userEntityModel);

                SuccessNotification("Change Password Success.");
                return RedirectToAction("UserList");
            }
            catch (Exception exc)
            {
                LogManager.Instance.Error(exc.Message);
                ErrorNotification("Change Password Failed." + exc.Message);
                return RedirectToAction("UserList");
            }

            //return RedirectToAction("Edit", new { userID = editEntityModel.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            _userService.SignOut();

            return RedirectToAction("Login", "AccessControl");
        }


 
        public ActionResult CreateRole()
        {
            var allowCreateNewUserUserIDs = new int[] { 1 };
            if (!allowCreateNewUserUserIDs.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }

            var newRoleViewModel = new RoleViewModel() { IsActive = 1 };

            FillDropDownDSForRole(newRoleViewModel);

            return View(newRoleViewModel);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRole(RoleViewModel model)
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
                _roleService.InsertRole(newEntityModel);

                return RedirectToAction("RoleList");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult EditRole(int roleID)
        {
            var allowEditNewUserUserIDs = new int[] { 1 };
            if (!allowEditNewUserUserIDs.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }

            var editRoleViewModel = new RoleViewModel();
            var role = _roleService.GetRoleByID(roleID);
            if (role != null)
            {
                editRoleViewModel = role.ToCreateNewModel();
            }

            FillDropDownDSForRole(editRoleViewModel);
            return View(editRoleViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRole(RoleViewModel model)
        {

            var allowEditNewUserUserIDs = new int[] { 1 };
            if (!allowEditNewUserUserIDs.Contains(_workContext.CurrentUser.ID))
            {
                ErrorNotification("You do not have permission to process this page.");
                return Redirect("~/"); ;
            }
            FillDropDownDSForRole(model);
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

            var editEntityModel = _roleService.GetRoleByID(model.ID);
            editEntityModel = model.ToCreateNewEntity(editEntityModel).FillOutNull();
            editEntityModel.EditBy = currentUser;
            editEntityModel.EditTime = currentTime;

            if (model.Permissions == null)
                model.Permissions = new List<int>();
            var permissionIDsToAdd = model.Permissions.Except(editEntityModel.RolePermissions.Select(ur => ur.PermissionID)).ToList();
            var permissionIDsToDelete = editEntityModel.RolePermissions.Select(rp => rp.PermissionID).Except(model.Permissions).ToList();
            foreach (var permissionIDToAdd in permissionIDsToAdd)
            {
                editEntityModel.RolePermissions.Add(new M_RolePermission() { RoleID = model.ID, PermissionID = permissionIDToAdd, CreateBy = currentUser, CreateTime = currentTime, EditBy = currentUser, EditTime = currentTime });
            }

            foreach (var permissionIDToDelete in permissionIDsToDelete)
            {
                _roleService.RemoveRolePermission(editEntityModel.RolePermissions.FirstOrDefault(ur => ur.PermissionID == permissionIDToDelete));
            }

            _roleService.UpdateRole(editEntityModel);

            return RedirectToAction("RoleList");
        }


        private void FillDropDownDS(UserViewModel model)
        {
            model.YesOrNo = YesNo.Y.ToSelectList(false).ToList();

            //model.AllRoles = _roleService.GetAllRoles().ToSelectListByList().ToList();
            model.AllRoles = new MultiSelectList(_roleService.GetAllRoles().Select(r => new { ID = r.ID, Name = r.Name }), "ID", "Name", model.Roles).ToList();
        }

        private void FillDropDownDSForRole(RoleViewModel model)
        {
            model.YesOrNo = YesNo.Y.ToSelectList(false).ToList();

            model.AllPermissions = new MultiSelectList(_permissionService.GetAllPermissions().Select(r => new { ID = r.ID, Name = r.Name }), "ID", "Name", model.Permissions).ToList();
        }
    }
}