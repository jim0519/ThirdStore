using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreBusiness.JobItem;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.ScheduleTask;

namespace ThirdStoreBusiness.ScheduleTask
{
    
    public class SyncInventoryForLastDayTask : ITask
    {
        private readonly IJobItemService _jobItemService;
        private readonly IScheduleRuleService _scheduleRuleService;

        public SyncInventoryForLastDayTask(IJobItemService jobItemService,
            IScheduleRuleService scheduleRuleService)
        {
            _jobItemService = jobItemService;
            _scheduleRuleService = scheduleRuleService;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public void Execute()
        {
            try
            {
                var typeName = this.GetType().Name;
                var scheduleRule = _scheduleRuleService.GetScheduleRuleByName(typeName);
                if(_scheduleRuleService.IsTaskCanBeRunNow(scheduleRule))
                {
                    var fromTime = DateTime.Now.AddDays(-1);
                    var toTime = DateTime.Now.AddDays(-1);
                    _jobItemService.SyncInventory(fromTime,toTime);

                    _scheduleRuleService.RunSuccess(scheduleRule);
                }

            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
            }
        }
    }
}
