using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness'
    public class EmailActiveBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness'
    {
        #region Fields, Properties

        private readonly IEmailActiveBO emailActiveBO;
        private readonly Logger logger = new Logger();

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness.EmailActiveBusiness(IBOFactory)'
        public EmailActiveBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness.EmailActiveBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            this.emailActiveBO = boFactory.GetBO<IEmailActiveBO>(printConfig, this.CurrentUser);
        }

        #endregion Contructor

        #region Methods

        public IEnumerable<EmailActiveInfo> Filter(ConditionSearchEmailActive condition)
        {
            condition.TotalRecords = this.emailActiveBO.CountFilter(condition);
            var emailActives = this.emailActiveBO.Filter(condition).ToList();
            return emailActives;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness.GetEmailActive(long)'
        public EmailActiveInfo GetEmailActive(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness.GetEmailActive(long)'
        {
            long companyId = this.CurrentUser.Company.Id ?? 0;
            return this.emailActiveBO.GetEmailActive(companyId, id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness.SendEmailActive(long, EmailActiveInfo)'
        public ResultCode SendEmailActive(long id, EmailActiveInfo emailActive)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness.SendEmailActive(long, EmailActiveInfo)'
        {
            if (emailActive == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.emailActiveBO.SendEmail(id, emailActive, this.CurrentUser.EmailServer);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness.SendEmailActives(SendMultiMail)'
        public ResultCode SendEmailActives(SendMultiMail emailActives)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveBusiness.SendEmailActives(SendMultiMail)'
        {
            if (emailActives == null || emailActives.ids == null || emailActives.ids.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.emailActiveBO.SendEmails(emailActives, this.CurrentUser.EmailServer);
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder, fullPathFileInvoice);
            try
            {
                if (this.CurrentUser.Company != null)
                {
                    config.BuildAssetByCompany(this.CurrentUser.Company);
                }
            }
            catch (Exception ex)
            {
                long userId = CurrentUser?.Id ?? 0;
                logger.Error(userId.ToString(), ex);
            }
            return config;
        }

        #endregion

    }
}