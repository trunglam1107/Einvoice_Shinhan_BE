using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.BL
{
    public interface ISignDetailBO
    {
        SIGNDETAIL GetByInvoiceId(int invoiceId, bool isClientSign = false);

        SIGNDETAIL GetByAnnouncementId(int announcementId, bool isClientSign = false);

        ResultCode Create(SignDetailInfo signDetailInfo);
    }
}
