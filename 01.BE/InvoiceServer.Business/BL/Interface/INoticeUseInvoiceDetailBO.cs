using InvoiceServer.Business.Models;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface INoticeUseInvoiceDetailBO
    {
        UseInvoiceDetailInfo GetNoticeUseInvoiceDetail(long id);
        IEnumerable<string> FilterInvoiceSymbol(long companyId, long invoiceTemplateId, bool? isCreate);

        IEnumerable<RegisterTemplateCancelling> FilterInvoiceSymbolUse(long companyId, long invoiceSampleId, long reportCancellingId);

        decimal GetNumberInvoiceRegisterOfAll();
        decimal GetNumberInvoiceRegisterOfCompany(long companyId);
        bool UsedByInvoices(long NotiId);

    }
}
