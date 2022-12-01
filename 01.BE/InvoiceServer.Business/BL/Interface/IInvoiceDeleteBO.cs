using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.BL
{
    public interface IInvoiceDeleteBO
    {
        void InvoiceDelete(CancellingInvoiceMaster deleteInfo);
        void MultipleInvoiceDelete(CancellingInvoiceMaster deleteInfo);

        void MultipleInvoiceCancel(CancellingInvoiceMaster deleteInfo);
        InvoiceInfo GetInvoiceInfo(long id,bool isInvoice = true);
    }
}
