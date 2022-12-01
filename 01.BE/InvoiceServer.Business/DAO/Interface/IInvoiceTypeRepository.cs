using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceTypeRepository : IRepository<INVOICETYPE>
    {
        IEnumerable<INVOICETYPE> GetList();
        IEnumerable<INVOICETYPE> Filter(ConditionSearchInvoiceType condition, int skip = 0, int take = int.MaxValue);
        INVOICETYPE GetByCode(string code);
        bool Update(string code, INVOICETYPE invoiceType);
        bool DeleteInvoiceType(string code);

        bool ContainCode(string code, string name, string denominator, bool create);

        INVOICETYPE GetById(long id);
        bool MyInvoiceTypeUsing(long invoiceTypeId);
    }
}
