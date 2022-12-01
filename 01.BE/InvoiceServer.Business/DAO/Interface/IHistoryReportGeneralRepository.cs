using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.HistoryReportGeneral;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.DAO
{
    public interface IHistoryReportGeneralRepository : IRepository<HISTORYREPORTGENERAL>
    {        
        HISTORYREPORTGENERAL GetById(long id);

        IEnumerable<HISTORYREPORTGENERAL> Filter(FilterHistoryReportGeneralCondition condition);
        ResultSignReportgeneral CheckResultSendTwan(long id);

        IEnumerable<HistoryReportGeneralInfo> GetAll(FilterHistoryReportGeneralCondition condition);

        long Count(FilterHistoryReportGeneralCondition condition);

        long GetSBTHDLIEU(long? companyId);
        //bool IsExistFirstTime_Success(decimal? isMonth, int? year, int? month, int? quarter, long? companyId);

        IEnumerable<HistoryReportGeneralInfo> GetAllForExcel(FilterHistoryReportGeneralCondition condition);

        IEnumerable<HistoryReportGeneralInfo> GetHistoryReportsList(ConditionReportDetailUse condition);
    }
}
