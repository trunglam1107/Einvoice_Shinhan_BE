using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IUnitListRepository : IRepository<UNITLIST>
    {
        IEnumerable<UNITLIST> GetList();
        IEnumerable<UNITLIST> Filter(ConditionSearchUnitList condition, int skip = 0, int take = int.MaxValue);
        UNITLIST GetByCode(string code, long companySID);
        bool Update(string code, UNITLIST unitList);
        bool DeleteTypeTax(string code, long companySID);
        bool ContainCode(string code, string name, bool create, long companySID);

        bool AddList(List<UNITLIST> unitLists);

        UNITLIST GetById(long id);
        bool MyUnitUsing(long id);

        IEnumerable<UNITLIST> GetByName(string name, long companyId);
        IEnumerable<UNITLIST> GetByNameOrCode(string code, string name, long companyId);
        UNITLIST GetByName(string name);
    }
}
