using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.InvoiceTemplate;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IInvoiceTemplateBO
    {
        IEnumerable<InvoiceTemplateInfo> GetList(long companySID);

        ExportFileInfo PrintViewTemplate(long templateId, long companyId);
        IEnumerable<InvoiceTemplateInfo> Filter(long invoiceSampleId, long companySID);
        long CreateInvoiceTemplateByInvoiceSample(InvoiceTemplateViewModel invoiceTemplateViewModel, long companyId, long sampleId);

        long GetByInvoiceSampleCode(string invoiceSampleCode, long companyId);

        InvoiceTemplateInfo GetByRegisterTemplateId(long registerTemplateId);
        List<INVOICETEMPLATE> GetAll();
    }
}

