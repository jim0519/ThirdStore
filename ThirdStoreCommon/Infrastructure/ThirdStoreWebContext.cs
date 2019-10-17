using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreCommon;
using Autofac.Core;
using System.Web.Http;
using Autofac.Integration.WebApi;

namespace ThirdStoreCommon.Infrastructure
{
    public class ThirdStoreWebContext
    {
        private static ThirdStoreWebContext _instance;
        private ContainerManager _containerManager;
        private ICacheManager _cacheManager;
        private ThirdStoreWebContext()
        {

        }

        public static ThirdStoreWebContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ThirdStoreWebContext();
                }
                return _instance;
            }
        }

        public void Initialize()
        {

            //dependency injection
            _containerManager = new ContainerManager(new ContainerBuilder().Build());
            _containerManager.RegisterDependency();

            //set dependency resolver
            DependencyResolver.SetResolver(new AutofacDependencyResolver(_containerManager.Container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(_containerManager.Container); //Set the WebApi DependencyResolver

            InitializeCache();

            //Run startup task
            RunStartupTasks();

        }

        private void InitializeCache()
        {
            _cacheManager = _containerManager.Resolve<ICacheManager>();
            _cacheManager.Get(ThirdStoreCacheKey.ThirdStoreJobItemConditionListCache, CacheFunc.GetThirdStoreJobItemCondition);
            //_cacheManager.Get(ThirdStoreCacheKey.OrderTypeList, CacheFunc.GetOrderTypeList);
            //_cacheManager.Get(ThirdStoreCacheKey.PaymentStatusList, CacheFunc.GetPaymentStatusList);

        }

        private void RunStartupTasks()
        {
            var typeFinder = _containerManager.Resolve<ITypeFinder>();
            var startUpTaskTypes = typeFinder.FindClassesOfType<IStartupTask>();
            var startUpTasks = new List<IStartupTask>();
            foreach (var startUpTaskType in startUpTaskTypes)
                startUpTasks.Add((IStartupTask)Activator.CreateInstance(startUpTaskType));
            //sort
            startUpTasks = startUpTasks.AsQueryable().OrderBy(st => st.Order).ToList();
            foreach (var startUpTask in startUpTasks)
                startUpTask.Execute();
        }

        public ContainerManager ContainerManager
        {
            get { return _containerManager; }
        }

        //public T Resolve<T>() where T : class
        //{
        //    return _containerManager.Resolve<T>();
        //}

        public object Resolve(Type type)
        {
            return _containerManager.Resolve(type);
        }

        public object ResolveOptional(Type serviceType)
        {
            return _containerManager.ResolveOptional(serviceType);
        }

        //public T Resolve<T>(string key = "") where T : class
        //{
        //    try
        //    {
        //        return _containerManager.Resolve<T>(key);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public T Resolve<T>(string key = "", ILifetimeScope scope = null) where T : class
        {
            if (scope == null)
            {
                //no scope specified
                scope = _containerManager.Scope();
            }
            if (string.IsNullOrEmpty(key))
            {
                return scope.Resolve<T>();
            }
            return scope.ResolveKeyed<T>(key);
        }

        public T Resolve<T>(IEnumerable<Parameter> parameters, string key = "", ILifetimeScope scope = null ) where T : class
        {
            if (scope == null)
            {
                //no scope specified
                scope = _containerManager.Scope();
            }
            if (string.IsNullOrEmpty(key))
            {
                if (parameters == null)
                    return scope.Resolve<T>();
                else
                    return scope.Resolve<T>(parameters);
            }
            if (parameters == null)
                return scope.ResolveKeyed<T>(key);
            else
                return scope.ResolveKeyed<T>(key,parameters);
        }

        public T[] ResolveAll<T>(string key = "")
        {
            return _containerManager.Resolve<IEnumerable<T>>(key).ToArray();
        }

        public object Resolve(Type type, ILifetimeScope scope = null)
        {
            if (scope == null)
            {
                //no scope specified
                scope = _containerManager.Scope();
            }
            return scope.Resolve(type);
        }

        public bool TryResolve(Type serviceType, ILifetimeScope scope, out object instance)
        {
            if (scope == null)
            {
                //no scope specified
                scope = _containerManager.Scope();
            }
            return scope.TryResolve(serviceType, out instance);
        }

        public T ResolveUnregistered<T>(ILifetimeScope scope = null) where T : class
        {
            return ResolveUnregistered(typeof(T), scope) as T;
        }

        public object ResolveUnregistered(Type type, ILifetimeScope scope = null)
        {
            if (scope == null)
            {
                //no scope specified
                scope =_containerManager.Scope();
            }
            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                try
                {
                    var parameters = constructor.GetParameters();
                    var parameterInstances = new List<object>();
                    foreach (var parameter in parameters)
                    {
                        var service = Resolve(parameter.ParameterType, scope);
                        if (service == null) throw new Exception("Unkown dependency");
                        parameterInstances.Add(service);
                    }
                    return Activator.CreateInstance(type, parameterInstances.ToArray());
                }
                catch (Exception)
                {

                }
            }
            throw new Exception("No contructor was found that had all the dependencies satisfied.");
        }

        //public ILifetimeScope BeginLifetimeScope()
        //{
        //    return _containerManager.Scope();
        //}
        
    }
}