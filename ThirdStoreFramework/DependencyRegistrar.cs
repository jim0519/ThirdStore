using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Core;
using Autofac.Integration.WebApi;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using ThirdStoreData;
using System.Data.Entity.Core.Metadata.Edm;
using System.Web;
using ThirdStoreBusiness.AccessControl;
using ThirdStoreBusiness.ScheduleTask;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.JobItem;
using ThirdStoreBusiness.API.Neto;
using ThirdStoreBusiness.Image;
using ThirdStoreBusiness.API.eBay;
using ThirdStoreBusiness.Order;
using ThirdStoreBusiness.ReportPrint;
using LINQtoCSV;
using ThirdStoreFramework.Controllers;
using ThirdStoreBusiness.DSChannel;
using ThirdStoreBusiness.Report;
using ThirdStoreBusiness.Setting;
using ThirdStoreBusiness.Misc;
using ThirdStoreBusiness.API.Dropshipzone;

namespace ThirdStoreFramework
{
    public class DependencyRegistrarData : IDependencyRegistrar
    {
        #region IDependencyRegistrar Members

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.Register(c =>
                //register FakeHttpContext when HttpContext is not available
                HttpContext.Current != null ?
                (new HttpContextWrapper(HttpContext.Current) as HttpContextBase) :
                (new FakeHttpContext("~/") as HttpContextBase))
                .As<HttpContextBase>()
                .InstancePerLifetimeScope();
            //builder.Register(c =>
            //    //register FakeHttpContext when HttpContext is not available

            //    (new HttpContextWrapper(HttpContext.Current) as HttpContextBase))
            //    .As<HttpContextBase>()
            //    .InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Request)
                .As<HttpRequestBase>()
                .InstancePerLifetimeScope();

            builder.RegisterType<CsvContext>().InstancePerLifetimeScope();
            builder.Register(c=>new CsvFileDescription() { SeparatorChar = ',', FirstLineHasColumnNames = true, IgnoreUnknownColumns = true, TextEncoding = Encoding.UTF8 }).InstancePerLifetimeScope();
            var assemblies = typeFinder.GetAssemblies().ToArray();
            //PerHttpRequest

            //do not change the order of the registration because autofac will always resolve the one having the latest registration, by default it should resolve the default db. 
            builder.RegisterType<ThirdStoreDBContext>().As<IDbContext>().Keyed<IDbContext>(Constants.ThirdStoreDBKey).InstancePerLifetimeScope();
            builder.RegisterType<WebWorkContext>().As<IWorkContext>().InstancePerLifetimeScope();

            builder.RegisterGeneric(typeof(ThirdStoreRepository<>)).As(typeof(IRepository<>)).WithParameter(
                new ResolvedParameter((pi, c) => pi.ParameterType == typeof(IDbContext), (pi, c) => c.ResolveKeyed<IDbContext>(SelectDB(pi.Member.DeclaringType.GetGenericArguments()[0])))).InstancePerLifetimeScope();


            //controllers
            builder.RegisterControllers(typeFinder.GetAssemblies().ToArray());
            builder.RegisterApiControllers(typeFinder.GetAssemblies().ToArray());

            //Sigleton
            builder.RegisterType<CacheManager>().As<ICacheManager>().SingleInstance();


            //Item
            builder.RegisterType<ItemService>().As<IItemService>().InstancePerLifetimeScope();

            //JobItem
            builder.RegisterType<JobItemService>().As<IJobItemService>().InstancePerLifetimeScope();

            //Image
            builder.RegisterType<ImageService>().As<IImageService>().InstancePerLifetimeScope();

            //Order
            builder.RegisterType<OrderService>().As<IOrderService>().InstancePerLifetimeScope();

            //Report Print
            builder.RegisterType<ReportPrintService>().As<IReportPrintService>().InstancePerLifetimeScope();
            builder.RegisterType<LocalReportPrinting>().As<ILocalReportPrinting>().InstancePerLifetimeScope();

            //API
            builder.RegisterType<NetoAPICredentialProvider>().As<INetoCredentialProvider>().InstancePerLifetimeScope();
            builder.RegisterType<NetoAPICallManager>().As<INetoAPICallManager>().InstancePerLifetimeScope();
            builder.RegisterType<eBayAPICredentialProvider>().As<IeBayApiContextProvider>().InstancePerLifetimeScope();
            builder.RegisterType<eBayAPICallManager>().As<IeBayAPICallManager>().InstancePerLifetimeScope();
            builder.RegisterType<DropshipzoneAPICredentialProvider>().As<IDropshipzoneCredentialProvider>().InstancePerLifetimeScope();
            builder.RegisterType<DropshipzoneAPICallManager>().As<IDropshipzoneAPICallManager>().InstancePerLifetimeScope();

            //Access Control
            builder.RegisterType<EncryptionService>().As<IEncryptionService>().InstancePerLifetimeScope();
            builder.RegisterType<PermissionService>().As<IPermissionService>().InstancePerLifetimeScope();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
            builder.RegisterType<RoleService>().As<IRoleService>().InstancePerLifetimeScope();
            builder.RegisterType<AuthorizationHelper>().As<IAuthorizationHelper>().InstancePerLifetimeScope();

            //Schedule Task
            builder.RegisterType<ScheduleTaskService>().As<IScheduleTaskService>().InstancePerLifetimeScope();

            //Schedule Rule
            builder.RegisterType<ScheduleRuleService>().As<IScheduleRuleService>().InstancePerLifetimeScope();

            //Dropship Channels
            builder.RegisterAssemblyTypes(assemblies).Where(t => typeof(IDSChannel).IsAssignableFrom(t)).InstancePerLifetimeScope().AsImplementedInterfaces();

            //Gumtree Feed
            builder.RegisterType<GumtreeFeedService>().As<IGumtreeFeedService>().InstancePerLifetimeScope();

            //Report
            builder.RegisterType<ReportService>().As<IReportService>().InstancePerLifetimeScope();

            //Setting
            builder.RegisterType<SettingService>().As<ISettingService>().InstancePerLifetimeScope();

            //Log
            builder.RegisterType<LogService>().As<ILogService>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 0; }
        }

        private string SelectDB(Type type)
        {
            var dbcontext = ThirdStoreWebContext.Instance.Resolve<IDbContext>(Constants.ThirdStoreDBKey);
            if (ExistsInDBContext(type, dbcontext))
            {
                return Constants.ThirdStoreDBKey;
            }

            //dbcontext=ThirdStoreWebContext.Instance.Resolve<IDbContext>(Constants.DeliveryManagementSystemDBKey);
            //if (ExistsInDBContext(type, dbcontext))
            //{
            //    return Constants.DeliveryManagementSystemDBKey;
            //}

            //dbcontext = ThirdStoreWebContext.Instance.Resolve<IDbContext>(Constants.WMSDBKey);
            //if (ExistsInDBContext(type, dbcontext))
            //{
            //    return Constants.WMSDBKey;
            //}

            //dbcontext = ThirdStoreWebContext.Instance.Resolve<IDbContext>(Constants.APIClientDBKey);
            //if (ExistsInDBContext(type, dbcontext))
            //{
            //    return Constants.APIClientDBKey;
            //}

            return Constants.ThirdStoreDBKey;
        }

        private bool ExistsInDBContext(Type type,IDbContext dbcontext)
        {
            string entityName = type.Name;
            var objContext = ((IObjectContextAdapter)dbcontext).ObjectContext;
            MetadataWorkspace workspace = objContext.MetadataWorkspace;
            return workspace.GetItems<EntityType>(DataSpace.CSpace).Any(e => e.Name == entityName);
        }

        #endregion
    }
}
