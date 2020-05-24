using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ThirdStore.Extensions;
using ThirdStore.Models.JobItem;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.JobItem;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreCommon.Models.JobItem;
using ThirdStoreFramework.Controllers;

namespace ThirdStore.Controllers
{
    [RoutePrefix("api/jobitem")]
    public class ApiJobItemController : BaseApiController
    {
        private readonly IJobItemService _jobItemService;
        private readonly IItemService _itemService;

        public ApiJobItemController(IJobItemService jobItemService,
            IItemService itemService)
        {
            _jobItemService = jobItemService;
            _itemService = itemService;
        }

        [Route("syncbyids")]
        [HttpPost]
        public IHttpActionResult SyncByIDs([FromBody] int[] ids)
        {
            if (ids == null || ids.Count() == 0)
                return Ok();

            var lstIDs = ids.ToList();

            var result=_jobItemService.SyncInventory(lstIDs);

            return Ok(result);
        }

        [Route("syncbyskus")]
        [HttpPost]
        public IHttpActionResult SyncByIDs([FromBody] string[] skus)
        {
            if (skus == null || skus.Count() == 0)
                return Ok();

            var lstIDs = _itemService.GetItemsBySKUs(skus.ToList()).Select(itm=>Convert.ToInt32(itm.ID)).ToList();

            var result = _jobItemService.SyncInventory(lstIDs);

            return Ok(result);
        }

        [Route("updatedsz")]
        [HttpPost]
        public IHttpActionResult UpdateDSZData()
        {
            var type2 = System.Type.GetType("ThirdStoreBusiness.ScheduleTask.UpdateDSDataAndSync, ThirdStoreBusiness");
            object instance;
            if (!ThirdStoreWebContext.Instance.TryResolve(type2, ThirdStoreWebContext.Instance.ContainerManager.Scope(), out instance))
            {
                //not resolved
                instance = ThirdStoreWebContext.Instance.ResolveUnregistered(type2, ThirdStoreWebContext.Instance.ContainerManager.Scope());
            }
            ThirdStoreBusiness.ScheduleTask.ITask task = instance as ThirdStoreBusiness.ScheduleTask.ITask;
            task.Execute();

            return Ok();
        }

        [Route("{jobItemID?}")]
        [HttpGet]
        public IHttpActionResult Get(int jobItemID=0,int page=0,int pagesize=10)
        {
            var jobItemList = new List<JobItemViewModel>();
            if(jobItemID!=0)
            {
                var jobItem = _jobItemService.GetJobItemByID(jobItemID);
                jobItemList.Add(jobItem.ToCreateNewModel());
            }
            else
            {
                jobItemList.AddRange(_jobItemService.SearchJobItems( isExcludeShippedStatus:false, pageIndex:page,pageSize:pagesize).Select(ji=>ji.ToCreateNewModel()));
            }

            return Ok(jobItemList);
        }

    }
}
