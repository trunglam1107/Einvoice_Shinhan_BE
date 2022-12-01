using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IInvoiceTypeBO
    {
        IEnumerable<InvoiceTypeViewModel> GetList(long CompanyId);

        IEnumerable<InvoiceTypeViewModel> Filter(ConditionSearchInvoiceType condition);

        InvoiceTypeViewModel GetByCode(string code);

        long Count(ConditionSearchInvoiceType condition);

        ResultCode Create(InvoiceTypeViewModel invoiceTypeViewModel);

        ResultCode Update(string code, InvoiceTypeViewModel invoiceTypeViewModel);

        ResultCode Delete(string code);
        bool MyInvoiceTypeUsing(long invoiceTypeId);

    }
}


