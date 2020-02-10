using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.JobItem;
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


    }
}
