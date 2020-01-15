using ThirdStoreCommon.Models.ScheduleTask;
using System.Collections.Generic;
using ThirdStoreData;
using System;
using System.Linq;

namespace ThirdStoreBusiness.ScheduleTask
{
    public interface IScheduleRuleService
    {
        T_ScheduleRule GetScheduleRuleByName(string ruleName);

        bool IsTaskCanBeRunNow(string ruleName);

        bool IsTaskCanBeRunNow(T_ScheduleRule rule);

        void RunSuccess(T_ScheduleRule rule);
    }

    public class ScheduleRuleService : IScheduleRuleService
    {
        private readonly IRepository<T_ScheduleRule> _scheduleRuleRepository;

        public ScheduleRuleService(IRepository<T_ScheduleRule> scheduleRule)
        {
            _scheduleRuleRepository = scheduleRule;
        }

        public bool IsTaskCanBeRunNow(string ruleName)
        {
            var scheduleRule = GetScheduleRuleByName(ruleName);
            if (scheduleRule == null)
                return false;

            return IsTaskCanBeRunNow(scheduleRule);

        }

        public T_ScheduleRule GetScheduleRuleByName(string ruleName)
        {
            return _scheduleRuleRepository.Table.FirstOrDefault(r=>r.Name.ToLower().Equals(ruleName.ToLower())&&r.IsActive);
        }

        public bool IsTaskCanBeRunNow(T_ScheduleRule rule)
        {
            var nowTime = DateTime.Now;

            var elaspDays = (nowTime.Date - rule.LastSuccessTime.Date).TotalDays;
            var nowTimeTime = nowTime.TimeOfDay;
            T_ScheduleRuleLine ruleLine = null;
            //Predicate<ScheduleRuleLine, TimeSpan, double> matchScheduleLine = (l, nowtime, elaspDays) => { };


            //see if nowTime is meet the interval day+LastSuccessTime
            if ((elaspDays % rule.IntervalDay == 0) && ((elaspDays > 0 && rule.IntervalDay >= 1) || elaspDays == 0 && rule.IntervalDay == 1))
            {

                //get the rule line whose time range the nowTime can sit in
                //returnRuleLine = rule.ScheduleRuleLines.FirstOrDefault(l => nowTimeTime >= l.TimeRangeFrom.TimeOfDay
                //    && nowTimeTime < l.TimeRangeTo.TimeOfDay
                //    && l.ScheduleRuleObj.LastSuccessTime.TimeOfDay < l.TimeRangeFrom.TimeOfDay.Add(TimeSpan.FromDays(elaspDays)));
                ruleLine = rule.ScheduleRuleLines.FirstOrDefault(l => ScheduleRuleLineFilter(l, nowTimeTime, elaspDays));
            }

            return ruleLine != null;
        }

        public void RunSuccess(T_ScheduleRule rule)
        {
            var now = DateTime.Now;
            rule.LastSuccessTime = now;
            _scheduleRuleRepository.Update(rule);
        }

        private Func<T_ScheduleRuleLine, TimeSpan, double, bool> ScheduleRuleLineFilter = (l, nowtime, elaspDays) =>
        {
            if (nowtime >= l.TimeRangeFrom.TimeOfDay
                     && nowtime < l.TimeRangeTo.TimeOfDay
                     && l.ScheduleRule.LastSuccessTime.TimeOfDay < l.TimeRangeFrom.TimeOfDay.Add(TimeSpan.FromDays(elaspDays)))
                return true;
            else
                return false;
        };

    }
}
