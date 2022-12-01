using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO.Interface
{
    public interface IQuarztJobRepository : IRepository<QUARZTJOB>
    {
        QUARZTJOB findByID(long id);
        QUARZTJOB findByName(string name);
        IEnumerable<QuarztJob> getAllJob(ConditionSearchQuartzJob condition);

        QUARZTJOB GetWorkingCalendar(long quarztJobId = 0);
    }
}
