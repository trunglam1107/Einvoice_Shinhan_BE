using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IUnitListBO
    {
        IEnumerable<UnitListViewModel> GetList(long CompanyId);

        IEnumerable<UnitListViewModel> Filter(ConditionSearchUnitList condition);

        UnitListViewModel GetByCode(ConditionSearchUnitList condition);
        bool MyUnitUsing(long unitId);

        long Count(ConditionSearchUnitList condition);

        ResultCode Create(UnitListViewModel unitListViewModel);

        ResultCode Update(string code, UnitListViewModel info);

        ResultCode Delete(string code, long companyId);

    }
}


