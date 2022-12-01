using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.HistoryReportGeneral;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IHistoryReportGeneralBO
    {
        IEnumerable<HistoryReportGeneralInfo> Filter(int monthOrQuarterValue, bool isMonth, int year,
           int additionalTimes);
        HISTORYREPORTGENERAL CreateHistoryGeneral(ConditionReportDetailUse condition,string fileName);
        HISTORYREPORTGENERAL GetById(long id);

        void updateFileName(ReportHistoryXml reportHistoryXml);
        ResultSignReportgeneral CheckResultSendTwan(long id);
        IEnumerable<HistoryReportGeneralInfo> GetAll(FilterHistoryReportGeneralCondition condition);

        long Count(FilterHistoryReportGeneralCondition condition);

        ExportFileInfo DownloadXMLBCTH(long id,long companyId);

        ExportFileInfo DownloadExcelHRG(FilterHistoryReportGeneralCondition condition);

        long GetAdditionTimesNext(ConditionReportDetailUse condition);
    }
}
