using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using ThirdStore.Extensions;
using ThirdStore.Models.JobItem;
using ThirdStoreBusiness.Item;
using ThirdStoreBusiness.JobItem;
using ThirdStoreCommon;
using ThirdStoreCommon.Infrastructure;
using ThirdStoreCommon.Models.JobItem;
using ThirdStoreFramework.Controllers;

namespace ThirdStore.Controllers
{
    [RoutePrefix("api/common")]
    public class ApiCommonController : ApiController
    {
       
        public ApiCommonController()
        {

        }

        [Route("ebayaccdelete")]
        [HttpGet]
        public IHttpActionResult eBayAccountDeletionGet(string challenge_code)
        {
            string verificationToken = Convert.ToBase64String(Encoding.ASCII.GetBytes("eBayAccountDeletionTokenjim0519"));
            verificationToken = new string(verificationToken.Where(c => char.IsLetter(c)).ToArray());
            //verificationToken = "ZUJheUFjYbnREZWxldGlvblRvaVuamltMDUxOQ";
            var endpoint = this.Request.RequestUri.AbsoluteUri.Replace(this.Request.RequestUri.Query, "");

            IncrementalHash sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            sha256.AppendData(Encoding.UTF8.GetBytes(challenge_code));
            sha256.AppendData(Encoding.UTF8.GetBytes(verificationToken));
            sha256.AppendData(Encoding.UTF8.GetBytes(endpoint));
            byte[] bytes = sha256.GetHashAndReset();
            var retChallengeResponse = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();


            return Json(new { challengeResponse = retChallengeResponse });

            //return Ok();
        }


        [Route("ebayaccdelete")]
        [HttpPost]
        public IHttpActionResult eBayAccountDeletionPost([FromBody] eBayAccountDeletionObject obj)
        {
            if (obj == null)
            {
                var errMsg = "Account Deletion Object is null";
                LogManager.Instance.Error(errMsg);
                return BadRequest(errMsg);
            }
            var serializedAccDeleteStr = JsonConvert.SerializeObject(obj);
            LogManager.Instance.Info(serializedAccDeleteStr);

            return Ok();
        }

        [Route("test")]
        [HttpGet]
        public IHttpActionResult Test()
        {
            try
            {
                LogManager.DBLogInstance.Info("Test Log in DB");
            }
            catch(Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }

            return Json(new { });

            //return Ok();
        }

    }


    public class eBayAccountDeletionObject
    {

        public Metadata metadata { get; set; }
        public Notification notification { get; set; }


        public class Notification
        {
            public string notificationId { get; set; }
            public DateTime eventDate { get; set; }
            public DateTime publishDate { get; set; }
            public int publishAttemptCount { get; set; }
            public Data data { get; set; }
        }

        public class Metadata
        {
            public string topic { get; set; }
            public string schemaVersion { get; set; }
            public bool deprecated { get; set; }
        }

        public class Data
        {
            public string username { get; set; }
            public string userId { get; set; }
            public string eiasToken { get; set; }
        }
    }
}
