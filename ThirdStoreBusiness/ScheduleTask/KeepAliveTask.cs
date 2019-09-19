using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ThirdStoreBusiness.ScheduleTask
{
    public partial class KeepAliveTask : ITask
    {
        //private readonly IeBayAPIContextProvider _eBayAPIContextProvider;
        public KeepAliveTask()
        {
            //_eBayAPIContextProvider = ThirdStoreCommon.Infrastructure.ThirdStoreWebContext.Instance.Resolve<IeBayAPIContextProvider>();
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            //Thread.Sleep(20000);
            if (HttpContext.Current != null)
            {
                string url = HttpContext.Current.Request.Url.Host+"/Home/Index";
                
                using (var wc = new WebClient())
                {
                    wc.DownloadString(url);
                }
            }
            else
                return;
        }
    }
}
