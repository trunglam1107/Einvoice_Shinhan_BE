using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IInvoiceSampleBO
    {
        IEnumerable<InvoiceSampleInfo> GetList();
        InvoiceSampleInfo GetInvoiceSampleById(long invoiceSampleId);

        InvoiceSampleInfo CreateInvoiceSample(InvoiceSampleInfo invoiceSampleInfo);

        InvoiceSampleInfo UpdateInvoiceSample(InvoiceSampleInfo invoiceSampleInfo);

        InvoiceSampleInfo Update(InvoiceSampleInfo info);

        ResultCode Update(long id, InvoiceSampleInfo invoiceSampleInfo);

        InvoiceSampleInfo GetByCode(string code);

        InvoiceSampleInfo GetById(long id);
        List<Data.DBAccessor.INVOICESAMPLE> GetAll();

        ResultCode Delete(string code);

        ResultCode DeleteById(long id);

        IEnumerable<InvoiceSampleInfo> Filter(ConditionSearchInvoiceSample condition);

        long Count(ConditionSearchInvoiceSample condition);

        IEnumerable<InvoiceSampleInfo> GetByInvoiceTypeId(long invoiceTypeId);
        bool MyInvoiceSampleUsing(long invoiceID);
    }
}

