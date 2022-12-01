using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.ExportInvoice
{
    public interface IXssf
    {
        ExportFileInfo ExportFile();
    }
}
