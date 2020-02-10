using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using ThirdStoreBusiness.AccessControl;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;

namespace ThirdStoreFramework.Controllers
{
    public interface IAuthorizationHelper
    {
        bool UserExistsAndActive(string userName,string password);
    }

    public class AuthorizationHelper : IAuthorizationHelper
    {
        public bool UserExistsAndActive(string userName, string password)
        {
            var userService = ThirdStoreCommon.Infrastructure.ThirdStoreWebContext.Instance.Resolve<IUserService>();

            if (userService.ValidateUser(userName, password))
            {
                return true;
            }

            return false;
        }

        
    }
}
