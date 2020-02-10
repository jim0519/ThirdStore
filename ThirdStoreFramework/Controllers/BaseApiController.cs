using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;

namespace ThirdStoreFramework.Controllers
{
    /// <summary>
    /// Base controller
    /// </summary>
    [BasicAuthenticate]
    public abstract class BaseApiController : ApiController
    {
        //private readonly IWorkContext _workContext;
        public BaseApiController()
        {

        }

        

    }
}
