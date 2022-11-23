using Autofac.Integration.WebApi;
using FluentValidation.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ThirdStore.Validators.JobItem;
using ThirdStoreBusiness.ScheduleTask;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreFramework.Validators;

namespace ThirdStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
       | SecurityProtocolType.Tls11
       | SecurityProtocolType.Tls12
       | SecurityProtocolType.Ssl3;
            ThirdStoreWebContext.Instance.Initialize();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            //ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(new ThirdStoreValidatorFactory()));
            FluentValidationModelValidatorProvider.Configure(provider=> {
                provider.ValidatorFactory = new ThirdStoreValidatorFactory();
                provider.AddImplicitRequiredValidator = false;

                //provider.Add(typeof(DesignatedSKUExistValidator), (metadata, context, description, validator) => new DesignatedSKUExistPropertyValidator(metadata, context, description, validator));
            });

            //run schedule task
            TaskManager.Instance.Initialize();
            TaskManager.Instance.Start();
        }

        //protected void Application_EndRequest()
        //{   //here breakpoint
        //    // under debug mode you can find the exceptions at code: this.Context.AllErrors
        //    var errors = this.Context.AllErrors;
        //}
    }
}
