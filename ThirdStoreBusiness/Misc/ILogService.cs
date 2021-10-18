using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon;
using ThirdStoreCommon.Models.Misc;
using ThirdStoreData;

namespace ThirdStoreBusiness.Misc
{
    public interface ILogService
    {
        IPagedList<T_Log> SearchGumtreeFeeds(
            string message = null,
            DateTime? logTimeFrom = null,
            DateTime? logTimeTo = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

    }

    public class LogService : ILogService
    {
        private readonly IRepository<T_Log> _logRepository;
        public LogService(IRepository<T_Log> logRepository
            )
        {
            _logRepository = logRepository;
        }

        public IPagedList<T_Log> SearchGumtreeFeeds(
            string message = null,
            DateTime? logTimeFrom = null, 
            DateTime? logTimeTo = null, 
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            var query = _logRepository.Table;

            if (!string.IsNullOrWhiteSpace(message))
                query = query.Where(l => l.Message.Contains(message));

            if (logTimeFrom != null)
                query = query.Where(o => o.Date >= logTimeFrom);

            if (logTimeTo != null)
            {
                logTimeTo = logTimeTo.Value.AddDays(1);
                query = query.Where(o => o.Date < logTimeTo);
            }

            query = query.OrderByDescending(i => i.Date);

            return new PagedList<T_Log>(query, pageIndex, pageSize);
        }
    }
}
