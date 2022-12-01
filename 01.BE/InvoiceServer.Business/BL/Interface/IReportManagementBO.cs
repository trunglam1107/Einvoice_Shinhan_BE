using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.BL
{
    public interface IReportManagementBO
    {
        ExportFileInfo PrintViewReportSelfPrintedInvoice(ConditionReportUse codition, CompanyInfo company);

        ExportFileInfo PrintViewNoticeCancelInvoice(ConditionReportCancelInvoice codition, CompanyInfo company);

        ExportFileInfo PrintViewNoticeUseInvoice(ConditionReportCancelInvoice codition, CompanyInfo company);

        ExportFileInfo PrintViewMonthlyInvoiceBoard(ConditionInvoiceBoard codition, CompanyInfo company);
        ReporMonthlyBoard PrintViewMonthlyInvoiceBoardView(ConditionInvoiceBoard codition, CompanyInfo company);

        ExportFileInfo PrintViewMonthlyInvoiceBoardXML(ConditionInvoiceBoard codition, CompanyInfo company);
    }
}
