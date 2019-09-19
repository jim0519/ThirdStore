using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ThirdStore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "ValidateSKUExist",
            //    routeTemplate: "api/{controller}/{action}/{sku}"
            //);
        }
    }
}
