using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.BL
{
    public interface IJobReplacedInvoice
    {
        void ReplaceInvoice();

        bool ReplaceInvoice_new();

        void MultipleInvoiceCancel(CancellingInvoiceMaster deleteInfo);

        ResultImportSheet ImportData(UploadInvoice dataUpload, string fullPathFile, long companyId);

        void MultipleInvoiceAdjusted(CancellingInvoiceMaster deleteInfo);

        ResultImportSheet ImportDataInvoiceAdjusted(UploadInvoice dataUpload, string fullPathFile, long companyId);
    }
}
