﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ThirdStoreFramework.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public class AuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly bool _dontValidate;


        public AuthorizeAttribute()
            : this(false)
        {
        }

        public AuthorizeAttribute(bool dontValidate)
        {
            this._dontValidate = dontValidate;
        }

        private void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }

        //private IEnumerable<AuthorizeAttribute> GetAdminAuthorizeAttributes(ActionDescriptor descriptor)
        //{
        //    return descriptor.GetCustomAttributes(typeof(AdminAuthorizeAttribute), true)
        //        .Concat(descriptor.ControllerDescriptor.GetCustomAttributes(typeof(AdminAuthorizeAttribute), true))
        //        .OfType<AdminAuthorizeAttribute>();
        //}

        //private bool IsAdminPageRequested(AuthorizationContext filterContext)
        //{
        //    var adminAttributes = GetAdminAuthorizeAttributes(filterContext.ActionDescriptor);
        //    if (adminAttributes != null && adminAttributes.Any())
        //        return true;
        //    return false;
        //}

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (_dontValidate)
                return;

            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                throw new InvalidOperationException("You cannot use [AdminAuthorize] attribute when a child action cache is active");

            //if()
            //if (IsAdminPageRequested(filterContext))
            //{
            //    if (!this.HasAdminAccess(filterContext))
            //        this.HandleUnauthorizedRequest(filterContext);
            //}
        }

        public virtual bool HasAdminAccess(AuthorizationContext filterContext)
        {
            //var permissionService = DropshipCommon.Infrastructure.DropshipWebContext.Instance.Resolve<IPermissionService>();
            //bool result = permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel);
            //return result;

            
            bool skipAuthorization = filterContext.ActionDescriptor.IsDefined(
                typeof(AllowAnonymousAttribute), inherit: true)
            || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(
                typeof(AllowAnonymousAttribute), inherit: true);
            if (filterContext.HttpContext.Request.IsAuthenticated || skipAuthorization)
                return true;
            else
                return false;
        }
    }
}
