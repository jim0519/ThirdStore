using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ThirdStoreFramework.Controllers;
using ThirdStoreCommon;


namespace ThirdStore.Controllers
{
    public class ReturnController : BaseController
    {

        public ReturnController()
        {

        }

        public ActionResult List()
        {


            return View();
        }
    }
}