using InvoiceServer.Business.BL;
using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness'
    public class EmailServerBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness'
    {
        #region Fields, Properties

        private readonly IConfigEmailServerBO emailServerBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness.EmailServerBusiness(IBOFactory)'
        public EmailServerBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness.EmailServerBusiness(IBOFactory)'
        {
            this.emailServerBO = boFactory.GetBO<IConfigEmailServerBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness.GetEmailServer()'
        public EmailServerInfo GetEmailServer()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness.GetEmailServer()'
        {
            long companyId = GetCompanyIdOfUser();
            return this.emailServerBO.GetByCompanyId(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness.Create(EmailServerInfo)'
        public ResultCode Create(EmailServerInfo emailServerInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness.Create(EmailServerInfo)'
        {
            if (emailServerInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long companyId = GetCompanyIdOfUser();
            emailServerInfo.CompanyId = companyId;
            ResultCode result = this.emailServerBO.Create(emailServerInfo);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness.SendEmailTest(SendMailTest)'
        public ResultCode SendEmailTest(SendMailTest sendMailTest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerBusiness.SendEmailTest(SendMailTest)'
        {
            if (sendMailTest == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long companyId = GetCompanyIdOfUser();
            sendMailTest.EmailServer.CompanyId = companyId;

            var email = new EmailInfo()
            {
                EmailTo = sendMailTest.EmailTo,
                Subject = "Test",
                Content = sendMailTest.Content,
            };

            ProcessEmail processEmail = new ProcessEmail();
            try
            {
                processEmail.Test(email, sendMailTest.EmailServer);
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.SendMailError, "Send mail test error!", ex);
            }

            return ResultCode.NoError;
        }
        #endregion
    }
}