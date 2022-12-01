using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO.Interface
{
    public interface ICurrencyRepository : IRepository<CURRENCY>
    {
        IEnumerable<CURRENCY> GetList();
        IEnumerable<CURRENCY> Filter(ConditionSearchInvoiceType condition, int skip = 0, int take = int.MaxValue);
        bool AddList(List<CURRENCY> currencyLists);
        bool Delete(string code);
        bool Update(string code, CURRENCY currencyList);
        bool ContainCode(string code, bool create);
        CURRENCY GetByCode(string code);
        bool MyCurrencyUsing(long currencyId);


    }
}

