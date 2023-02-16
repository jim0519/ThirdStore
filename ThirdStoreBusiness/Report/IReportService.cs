using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon.Models;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Report;

namespace ThirdStoreBusiness.Report
{
    public interface IReportService
    {

        IPagedList<KPIReport> GetKPIReport(
            DateTime? createTimeFrom = null,
            DateTime? createTimeTo = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);


        IPagedList<LocationRanking> GetLocationRanking(
            int pageIndex = 0,
            int pageSize = int.MaxValue);

    }
}
