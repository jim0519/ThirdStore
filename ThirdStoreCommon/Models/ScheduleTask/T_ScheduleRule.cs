using System;
using System.Collections.Generic;

namespace ThirdStoreCommon.Models.ScheduleTask
{
    public class T_ScheduleRule : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int IntervalDay { get; set; }
        public DateTime LastSuccessTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime EditTime { get; set; }
        public string EditBy { get; set; }


        public virtual ICollection<T_ScheduleRuleLine> ScheduleRuleLines { get; set; }
    }
}
