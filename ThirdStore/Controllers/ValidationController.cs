using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ThirdStoreBusiness.Item;

namespace ThirdStore.Controllers
{
    [RoutePrefix("api/validation")]
    public class ValidationController : ApiController
    {
        private readonly IItemService _itemService;

        public ValidationController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [Route("isskuexists")]
        [HttpGet]
        public IHttpActionResult ValidateSKUExist(string sku)
        {
            bool validationResult = true;
            //var sku = "ggg";
            if (!String.IsNullOrEmpty(sku))
            {
                var item =  _itemService.GetItemBySKU(sku);

                validationResult = (item != null);
            }

            return Ok(validationResult);
        }
    }
}
