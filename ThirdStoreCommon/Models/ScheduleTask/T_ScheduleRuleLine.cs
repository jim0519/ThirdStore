using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.ScheduleTask
{
    public class T_ScheduleRuleLine : BaseEntity
    {
        public int ScheduleRuleID { get; set; }
        public DateTime TimeRangeFrom { get; set; }
        public DateTime TimeRangeTo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime EditTime { get; set; }
        public string EditBy { get; set; }

        public virtual T_ScheduleRule ScheduleRule { get; set; }
    }
}
