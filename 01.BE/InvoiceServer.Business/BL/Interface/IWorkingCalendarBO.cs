using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.BL.Interface
{
    public interface IWorkingCalendarBO
    {
        IEnumerable<WorkingCalendarDAO> GetWorkingCalendar(int year, long quarztJobId = 0);
        FullYearWorkingCalendarDAO GetYears(long quarztJobId = 0);
        ResultCode UpdateWorkingCalendar(WorkingCalendarDAO calendarDAO, long quarztJobId = 0);
        FullYearWorkingCalendarDAO AddYearWorkingCalendar(long quarztJobId = 0, int year = 0);

        List<WorkingCalendarDTO> GetWorkingCalendarForPostingGL(int year, long quarztJobId = 0);        
    }
}
