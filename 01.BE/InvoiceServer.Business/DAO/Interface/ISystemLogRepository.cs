using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ISystemLogRepository : IRepository<SYSTEMLOG>
    {
        IEnumerable<SystemLogInfo> FilterSystemLog(ConditionSearchSystemLog condition);
        int Count(ConditionSearchSystemLog condition);
    }
}
