using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreData;
using ThirdStoreCommon;
using ThirdStoreCommon.Models;
using System.Data.SqlClient;
using System.Data;
using ThirdStoreCommon.Models.Report;

namespace ThirdStoreBusiness.Report
{
    public class ReportService:IReportService
    {
        private IDbContext _dbContext;

        public ReportService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        



        public IPagedList<KPIReport> GetKPIReport(
            DateTime? createTimeFrom = null,
            DateTime? createTimeTo = null,
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            var sqlStr = new StringBuilder();
            sqlStr.Append("select * from fn_GetKPIReport(@CreateTimeFrom,@CreateTimeTo)");

            var paraCreateTimeFrom = new SqlParameter();
            paraCreateTimeFrom.ParameterName = "CreateTimeFrom";
            paraCreateTimeFrom.DbType = DbType.Date;
            paraCreateTimeFrom.Value = (createTimeFrom.HasValue ? createTimeFrom.Value.Date : Convert.ToDateTime(Constants.MinDate).Date);

            var paraCreateTimeTo = new SqlParameter();
            paraCreateTimeTo.ParameterName = "CreateTimeTo";
            paraCreateTimeTo.DbType = DbType.Date;
            paraCreateTimeTo.Value = (createTimeTo.HasValue ? createTimeTo.Value.Date : Convert.ToDateTime(Constants.MaxDate).Date);

            var query = _dbContext.SqlQuery<KPIReport>(sqlStr.ToString(), paraCreateTimeFrom, paraCreateTimeTo).OrderBy(m => m.CreateDate);

            return new PagedList<KPIReport>(query.ToList(), pageIndex, pageSize);
        }
    }
}
