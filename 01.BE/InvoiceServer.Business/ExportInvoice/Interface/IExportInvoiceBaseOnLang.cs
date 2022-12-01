using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.ExportInvoice.Interface
{
    interface IExportInvoiceBaseOnLang
    {
        ExportFileInfo ExportFile(string lang);
    }
}
