using InvoiceServer.Business.Models;

namespace InvoiceServer.Business.BL
{
    public interface ICancellingInvoiceBO
    {
        ReleaseInvoiceMaster Create(CancellingInvoiceMaster cancellingInfo);
        ReleaseInvoiceMaster CancellingInvoice(CancellingInvoiceMaster cancellingInvoiceinfo, bool isAutoSino = false);
    }
}
