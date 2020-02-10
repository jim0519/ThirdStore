using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using ThirdStoreBusiness.AccessControl;

namespace ThirdStoreFramework.Controllers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public class BasicAuthenticateAttribute : AuthorizationFilterAttribute
    {
        private readonly IAuthorizationHelper _authorizationHelper = ThirdStoreCommon.Infrastructure.ThirdStoreWebContext.Instance.Resolve<IAuthorizationHelper>();

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string authHeader = null;
            var auth = actionContext.Request.Headers.Authorization;
            if (auth != null && auth.Scheme == "Basic")
                authHeader = auth.Parameter;

            if (!string.IsNullOrEmpty(authHeader))
            {
                authHeader = Encoding.Default.GetString(Convert.FromBase64String(authHeader));
                var userNamePwdPair = authHeader.Split(new char[] { ':' }, 2);
                if (userNamePwdPair.Length == 2)
                {
                    if (_authorizationHelper.UserExistsAndActive(userNamePwdPair[0], userNamePwdPair[1]))
                    {
                        Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(userNamePwdPair[0]), null);
                    }
                    else
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                    
            }
            else
            { 
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

        }

    }
}
