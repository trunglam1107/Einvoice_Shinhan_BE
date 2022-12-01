using InvoiceServer.Business.BL;
using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness'
    public class SessionBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness'
    {
        #region Fields, Properties

        private readonly ISessionManagerBO sessionManagerBO;
        private readonly ISystemLogBO systemLogBO;
        private readonly EmailConfig emailConfig;
        #endregion Fields, Properties

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.SessionBusiness(IBOFactory)'
        public SessionBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.SessionBusiness(IBOFactory)'
        {
            var config = new SessionConfig()
            {
                LoginSecretKey = Config.ApplicationSetting.Instance.LoginSecretKey,
                SessionTimeout = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.SessionTimeout),
                CreateTokenRandom = Config.ApplicationSetting.Instance.EnableCreateTokenRandom,
                SessionResetPasswordExpire = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.ResetPasswordTimeOut)
            };
            if (HttpContext.Current != null)
            {
                emailConfig = new EmailConfig()
                {
                    FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
                };
            }
            else
            {
                emailConfig = null;
            }

            this.sessionManagerBO = boFactory.GetBO<ISessionManagerBO>(config);
            this.systemLogBO = boFactory.GetBO<ISystemLogBO>();
        }

        
#pragma warning disable CS1572 // XML comment has a param tag for 'token', but there is no parameter by that name
/// <summary>
        /// Process user login flow
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public UserSessionInfo Login(LoginInfo loginInfo)
#pragma warning restore CS1572 // XML comment has a param tag for 'token', but there is no parameter by that name
        {
            var keyOfPackage = Config.ApplicationSetting.Instance.NumOfPackage;
            var userSessionInfo = this.sessionManagerBO.Login(loginInfo, keyOfPackage);
            SaveAuditLogInOut("Login", (int)SystemLog_LogType.Success, userSessionInfo.UserId);

            return userSessionInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.QuarzLogin()'
        public UserSessionInfo QuarzLogin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.QuarzLogin()'
        {
            var keyOfPackage = Config.ApplicationSetting.Instance.NumOfPackage;
            //SaveAuditLogInOut("Check User in QuarzLogin", (int)SystemLog_LogType.Success, keyOfPackage);
            var userSessionInfo = this.sessionManagerBO.JobLogin(keyOfPackage);
            return userSessionInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.ClientLogin(LoginInfo)'
        public UserSessionInfo ClientLogin(LoginInfo loginInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.ClientLogin(LoginInfo)'
        {
            var userSessionInfo = this.sessionManagerBO.ClientLogin(loginInfo);
            return userSessionInfo;
        }


        /// <summary>
        /// Process: user logout flow
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ResultCode Logout(string token)
        {
            this.sessionManagerBO.Logout(token);
            return ResultCode.NoError;
        }

        /// <summary>
        /// Process: checking user session is alive
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ResultCode CheckUserSession(string token)
        {
            if (Config.ApplicationSetting.Instance.TokenKeyResearch.Equals(token))
            {
                return ResultCode.SessionAlive;
            }

            var sessionInfo = this.sessionManagerBO.GetUserSession(token);
            return sessionInfo != null ? ResultCode.SessionAlive : ResultCode.SessionEnded;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.GetUserSession(string)'
        public UserSessionInfo GetUserSession(string token)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.GetUserSession(string)'
        {
            if (Config.ApplicationSetting.Instance.TokenKeyResearch.Equals(token))
            {
                return new UserSessionInfo()
                {
                    UserId = "NewInvoice",
                    UserName = "NewInvoice",
                    RoleUser = new RoleLevel() { Permissions = new List<string>() },
                };
            }
            return this.sessionManagerBO.GetUserSession(token);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.ResetPassword(ResetPassword)'
        public async Task<bool> ResetPassword(ResetPassword resetPasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.ResetPassword(ResetPassword)'
        {
            if (resetPasswordInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var userSessionInfo = this.sessionManagerBO.ResetPassword(resetPasswordInfo);
            var receiverInfo = new ReceiverInfo(userSessionInfo, Resources.ResetPassword);
            receiverInfo.UrlResetPassword = string.Format("{0}{1}/", Config.ApplicationSetting.Instance.UrlResetPassword, userSessionInfo.Token);

            EmailInfo email = EmailTemplate.GetEmail(emailConfig, Resources.ResetPassword, receiverInfo);
            ProcessEmail processEmail = new ProcessEmail();
            return await Task.Factory.StartNew(() =>
                processEmail.SendEmail(email, this.CurrentUser.EmailServer)
            ).ContinueWith(x => true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.UpdatePassword(ChangePassword)'
        public ResultCode UpdatePassword(ChangePassword passwordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.UpdatePassword(ChangePassword)'
        {
            if (passwordInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.sessionManagerBO.UpdatePassword(this.CurrentUser.Id, passwordInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.GetAccountInfo(string)'
        public UserSessionInfo GetAccountInfo(string token)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.GetAccountInfo(string)'
        {
            return this.sessionManagerBO.GetUserSession(token);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.GetTypeAuthentication()'
        public string GetTypeAuthentication()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.GetTypeAuthentication()'
        {
            return Config.ApplicationSetting.Instance.IsUseLDAPAuth.ToString();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.SaveAuditLogInOut(string, int, string)'
        public void SaveAuditLogInOut(string log, int flgSuccess = (int)SystemLog_LogType.Success, string userId = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionBusiness.SaveAuditLogInOut(string, int, string)'
        {
            //Save Audit Log (Login) 
            String ip = IP.GetIPAddress();
            string logDetail = null;
            string logSummary = null;

            // Set log detail
            if (log.Equals("Login"))
            {
                logDetail = "Login successful system";
            }
            else
            {
                logDetail = log.Equals("PortalLogin") ? "Portal login successful system" : "Logged out of the system";
            }

            if (flgSuccess != (int)SystemLog_LogType.Success)
            {
                logDetail = log.Equals("Login") ? "System login failed" : "Portal login failed";
            }

            // Set log symmary
            if (log.Equals("Login"))
            {
                logSummary = "Log into the system";
            }
            else
            {
                logSummary = log.Equals("PortalLogin") ? "Portal login into system" : "Log out of the system";
            }
            var systemInfo = new SystemLogInfo
            {
                FunctionCode = log,
                LogSummary = logSummary,
                LogType = flgSuccess,
                LogDetail = logDetail,
                IP = ip,
                UserName = log.Equals("PortalLogin") ? userId : this.CurrentUser?.UserId ?? userId,
            };
            this.systemLogBO.Create(systemInfo);
        }
        #endregion Methods
    }
}