using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.InvoiceTemplate;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IInvoiceTemplateSampleBO
    {
        IEnumerable<InvoiceTemplateInfo> GetList();

        IEnumerable<InvoiceTemplateInfo> GetListByInvoiceTypeId(long invoiceTypeId);

        InvoiceTemplateInfo GetById(long id);

        InvoiceTemplateViewModel GetViewModelById(long id);

        InvoiceTemplateSampleName GetTemplateSampleNameById(long id);


    }
}

