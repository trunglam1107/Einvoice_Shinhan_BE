using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IUserBO
    {
        IEnumerable<AccountDetail> FillterUser(ConditionSearchUser condition, int skip = int.MinValue, int take = int.MaxValue);

        int CountFillterUser(ConditionSearchUser condition);

        AccountInfo GetAccountInfo(int id, int? companyId, string level);



        ResultCode Create(AccountInfo userInfo);

        ResultCode Update(int id, AccountInfo userInfo);

        ResultCode Delete(int id, int? companyId, string roleOfUserDelete);

        ResultCode UpdateCurrentUser(LOGINUSER loginUser);

        ResultCode ChangePassword(int id, PasswordInfo passowrdInfo);
    }
}
