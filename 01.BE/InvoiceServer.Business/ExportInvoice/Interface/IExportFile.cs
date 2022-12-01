using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.ExportInvoice
{
    public interface IExportFile
    {
        FileInfomation ExportFile();
    }
}
