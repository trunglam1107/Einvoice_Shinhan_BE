using InvoiceServer.Business.Models;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IReportSummaryBO
    {
        ExportFileInfo PrintViewMonthlyInvoiceBoard(ConditionInvoiceBoard condition, CompanyInfo companyInfo);
        ReporMonthlyBoard PrintViewMonthlyInvoiceBoardView(ConditionInvoiceBoard condition, CompanyInfo companyInfo, int skip = 0, int take = 0);

        ExportFileInfo PrintViewMonthlyInvoiceBoardXML(ConditionInvoiceBoard condition, CompanyInfo companyInfo);

        IEnumerable<ReportCustomer> ViewReportCustomers(ConditionReportSummary condition, CompanyInfo company);

        ExportFileInfo ExportReportCustomers(ConditionReportSummary condition, CompanyInfo company);

    }
}
