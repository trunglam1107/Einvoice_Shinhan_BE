using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IEmployeeBO
    {
        IEnumerable<EmployeeInfo> FillterEmployee(ConditionSearchEmployee condition, int skip = int.MinValue, int take = int.MaxValue);

        int CountFillterEmployee(ConditionSearchEmployee condition);

        EmployeeInfo GetEmployeeInfo(int id, int? companyId, string level);

        ResultCode Create(EmployeeInfo userInfo);

        ResultCode Update(int id, EmployeeInfo userInfo);

        ResultCode Delete(int id, int? companyId, string roleOfUserDelete);
    }
}
