using InvoiceServer.Business.Cache;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Net.Mail;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness'
    public abstract class BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness'
    {
        private UserSessionInfo _currentUser;

        private SmtpClient smtpClientOfCompany;

        private static readonly object lockObject = new object();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness.CurrentUser'
        public UserSessionInfo CurrentUser
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness.CurrentUser'
        {
            get
            {
                lock (lockObject)
                {

                    if (this._currentUser == null)
                    {
                        string token = HttpUtils.GetRequestHeaderValue(HttpContext.Current.Request, CustomHttpRequestHeader.AuthorizationToken);
                        if (token == null || token == "")
                        {
                            token = "quarzt";
                        }
                        this._currentUser = UserSessionCache.Instance.GetUserSession(token);
                    }

                    return this._currentUser;
                }
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness.SmtpClientOfCompany'
        public SmtpClient SmtpClientOfCompany
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness.SmtpClientOfCompany'
        {
            get
            {
                lock (lockObject)
                {

                    if (this.smtpClientOfCompany == null)
                    {
                        this.smtpClientOfCompany = GetSmtpClientOfCompany(this.CurrentUser.EmailServer);
                    }

                    return this.smtpClientOfCompany;
                }

            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness.GetSmtpClientOfCompany(EmailServerInfo)'
        public SmtpClient GetSmtpClientOfCompany(EmailServerInfo emailServer)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness.GetSmtpClientOfCompany(EmailServerInfo)'
        {
            SmtpClient smtpCOC = new SmtpClient();

            if (emailServer == null)
            {
                return smtpCOC;
            }

            smtpCOC.Host = emailServer.SMTPServer;
            smtpCOC.Port = emailServer.Port;
            smtpCOC.Credentials = new System.Net.NetworkCredential(emailServer.EmailServer, emailServer.Password);
            smtpCOC.EnableSsl = emailServer.MethodSendSSL;
            return smtpCOC;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness.GetCompanyIdOfUser()'
        public long GetCompanyIdOfUser()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseBusiness.GetCompanyIdOfUser()'
        {
            long companyId = 0;
            if (this.CurrentUser.Company.Id.HasValue)
            {
                companyId = this.CurrentUser.Company.Id.Value;
            }

            return companyId;
        }
    }
}