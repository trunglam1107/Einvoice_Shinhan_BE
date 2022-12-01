using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Account;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface ILoginUserBO
    {
        IEnumerable<AccountDetail> FillterUser(ConditionSearchUser condition);

        long CountFillterUser(ConditionSearchUser condition);

        IEnumerable<AccountDetailLogin> FillterUserOutRole(ConditionSearchUser condition);

        AccountInfo GetAccountInfo(long id, long? companyId, string level);

        ResultCode Create(AccountInfo userInfo);

        ResultCode Update(long id, AccountInfo userInfo);

        ResultCode Delete(long id, long? companyId, string roleOfUserDelete);

        ResultCode UpdateCurrentUser(LOGINUSER loginUser);

        ResultCode ChangePassword(long id, PasswordInfo passowrdInfo);

        ResultCode ChangeLanguage(long id, string language);

        LOGINUSER GetById(long id, long? companyId);

        AccountViewModel GetCustomerById(long id, long? companyId);

        IEnumerable<AccountViewModel> GetListAccount(ConditionSearchUser condition);

        List<AccountViewModel> DownloadDataAccount(ConditionSearchUser condition);
        long CountListAccount(ConditionSearchUser condition);
        UserSessionInfo ResetPassword(ResetPassword resetPasswordInfo);
        EmailServerInfo GetEmailServer();
        ResultCode UpdatePasswordStatus(long id);
        ResultImportSheet ImportData(string fullPathFile, long companyId, UserSessionInfo userSessionInfo);
        ExportFileInfo DownloadClientAccount(ConditionSearchUser condition);
    }
}
