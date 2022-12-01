using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IQuarztJobBO
    {
        IEnumerable<QuarztJob> getallJob(ConditionSearchQuartzJob condition);
        QUARZTJOB findByName(string name);
        bool updateStatus(long id, bool status);
        ResultCode UpdateCron(QuarztJob model);

        IEnumerable<WorkingCalendarDAO> GetWorkingCalendar(int year);
        void UpdateProcessByName(string name, bool processing);
    }
}
