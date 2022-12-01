using InvoiceServer.Business.Models;
using InvoiceServer.Common;

namespace InvoiceServer.Business.BL
{
    public interface ISessionManagerBO
    {
        UserSessionInfo Login(LoginInfo loginInfo, string keyOfPackage = null);
        UserSessionInfo Logout(string token);
        UserSessionInfo GetUserSession(string token);
        UserSessionInfo ResetPassword(ResetPassword resetPasswordInfo);
        ResultCode UpdatePassword(long userId, ChangePassword updatePasswordInfo);
        ResultCode UpdateClientPassword(long userId, ChangePassword updatePasswordInfo);
        ResultCode ResetClientPassword(ResetPassword updatePasswordInfo);
        long getNumberOfPackage(string keyOfPackage);
        UserSessionInfo JobLogin(string keyOfPackage = null);
        UserSessionInfo ClientLogin(LoginInfo loginInfo);

        EmailServerInfo GetEmailServer();

    }
}
