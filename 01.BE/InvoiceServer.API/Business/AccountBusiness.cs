using InvoiceServer.Business.BL;
using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Account;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness'
    public class AccountBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness'
    {
        #region Fields, Properties

        private readonly ILoginUserBO loginUseBO;
        private readonly ISessionManagerBO sessionManagerBO;
        private readonly EmailConfig emailConfig;
        private readonly IClientBO clientUseBO;
        private readonly IEmailActiveBO emailActiveBO;
        private readonly ILoginUserBO clientAccountBO;
        #endregion Fields, Properties

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.AccountBusiness(IBOFactory)'
        public AccountBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.AccountBusiness(IBOFactory)'
        {
            var config = new SessionConfig()
            {
                LoginSecretKey = Config.ApplicationSetting.Instance.LoginSecretKey,
                SessionTimeout = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.SessionTimeout),
                CreateTokenRandom = Config.ApplicationSetting.Instance.EnableCreateTokenRandom,
                SessionResetPasswordExpire = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.ResetPasswordTimeOut),

            };

            emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            PrintConfig printConfig = GetPrintConfig();
            this.loginUseBO = boFactory.GetBO<ILoginUserBO>(config);
            this.sessionManagerBO = boFactory.GetBO<ISessionManagerBO>(config);
            this.clientUseBO = boFactory.GetBO<IClientBO>(printConfig, emailConfig);
            this.emailActiveBO = boFactory.GetBO<IEmailActiveBO>(printConfig);
            this.clientAccountBO = boFactory.GetBO<ILoginUserBO>(config, printConfig);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.FillterUser(ConditionSearchUser)'
        public List<AccountDetail> FillterUser(ConditionSearchUser condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.FillterUser(ConditionSearchUser)'
        {
            condition.TotalRecords = this.loginUseBO.CountFillterUser(condition);
            return this.loginUseBO.FillterUser(condition).ToList();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.FillterUserOutRole(ConditionSearchUser)'
        public List<AccountDetailLogin> FillterUserOutRole(ConditionSearchUser condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.FillterUserOutRole(ConditionSearchUser)'
        {
            var res = this.loginUseBO.FillterUserOutRole(condition);
            condition.TotalRecords = res.Count();
            return res.ToList();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.GetAccoutInfo(long)'
        public AccountInfo GetAccoutInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.GetAccoutInfo(long)'
        {
            return this.loginUseBO.GetAccountInfo(id, this.CurrentUser.Company.Id, this.CurrentUser.RoleUser.Level);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ChangePassword(long, PasswordInfo)'
        public ResultCode ChangePassword(long id, PasswordInfo passwordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ChangePassword(long, PasswordInfo)'
        {
            bool sameUser = id == this.CurrentUser.Id;

            if (!passwordInfo.IsReset && !sameUser)
            {
                throw new BusinessLogicException(ResultCode.IdNotMatch,
                                string.Format("Change password faild because parameter Id not match with current Id = [{0}].", id));
            }

            return this.loginUseBO.ChangePassword(id, passwordInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ChangeLanguage(string)'
        public ResultCode ChangeLanguage(string language)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ChangeLanguage(string)'
        {
            if (language == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long companyId = this.CurrentUser.Id;

            return this.loginUseBO.ChangeLanguage(companyId, language);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.CreateUser(AccountInfo)'
        public ResultCode CreateUser(AccountInfo userInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.CreateUser(AccountInfo)'
        {
            if (userInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            userInfo.CompanyId = this.CurrentUser.Company.Id;
            return this.loginUseBO.Create(userInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.UpdateUser(long, AccountInfo)'
        public ResultCode UpdateUser(long id, AccountInfo userInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.UpdateUser(long, AccountInfo)'
        {
            if (userInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.loginUseBO.Update(id, userInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.DeleteUser(long)'
        public ResultCode DeleteUser(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.DeleteUser(long)'
        {
            string roleOfCurrentUser = this.CurrentUser.RoleUser.Level;
            long? companyId = this.CurrentUser.Company.Id;
            return this.loginUseBO.Delete(id, companyId, roleOfCurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.GetListAccount(ConditionSearchUser)'
        public IEnumerable<AccountViewModel> GetListAccount(ConditionSearchUser condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.GetListAccount(ConditionSearchUser)'
        {
            condition.TotalRecords = this.loginUseBO.CountListAccount(condition);
            var rs = this.loginUseBO.GetListAccount(condition);
            return rs;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.DownloadDataAccount(ConditionSearchUser)'
        public List<AccountViewModel> DownloadDataAccount(ConditionSearchUser condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.DownloadDataAccount(ConditionSearchUser)'
        {
            var rs = this.loginUseBO.DownloadDataAccount(condition);
            return rs;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.GetAccountById(long)'
        public AccountViewModel GetAccountById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.GetAccountById(long)'
        {
            return this.loginUseBO.GetCustomerById(id, null);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ResetPassword(ResetPassword)'
        public async Task<bool> ResetPassword(ResetPassword resetPasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ResetPassword(ResetPassword)'
        {
            if (resetPasswordInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var userSessionInfo = this.loginUseBO.ResetPassword(resetPasswordInfo);
            var receiverInfo = new ReceiverInfo(userSessionInfo, Resources.ResetPassword);
            receiverInfo.UrlResetPassword = string.Format("{0}{1}/", Config.ApplicationSetting.Instance.UrlResetPassword, userSessionInfo.Token);

            EmailInfo email = EmailTemplate.GetEmail(emailConfig, Resources.ResetPassword, receiverInfo);
            ProcessEmail processEmail = new ProcessEmail();
            return await Task.Factory.StartNew(() =>
                //processEmail.SendEmail(new SendGmail(email, GetSmtpClientOfCompany(userSessionInfo.EmailServer)))
                processEmail.SendEmail(email, this.CurrentUser.EmailServer)
            ).ContinueWith(x => true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ResetClientPassword(ResetPassword)'
        public ResultCode ResetClientPassword(ResetPassword resetPasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ResetClientPassword(ResetPassword)'
        {
            ResultCode resultCode;
            if (resetPasswordInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            resultCode = this.sessionManagerBO.ResetClientPassword(resetPasswordInfo);
            if (resultCode == ResultCode.NoError)
            {
                var emailActiveId = this.clientUseBO.CreateEmailActiveByClientId(resetPasswordInfo.ClientId,null);
                var clientAddInfo = new ClientAddInfo()
                {
                    Id = resetPasswordInfo.ClientId,
                    EmailActiveId = emailActiveId
                };
                resultCode = this.emailActiveBO.SendEmailProvideAccount(clientAddInfo);

                if (resultCode == ResultCode.NoError)
                {
                    //encrypt password after send email account infomation
                    resultCode = this.clientUseBO.EncryptUserPassword(resetPasswordInfo.ClientId);
                }
            }

            return resultCode;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.UpdatePasswordStatus(long)'
        public ResultCode UpdatePasswordStatus(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.UpdatePasswordStatus(long)'
        {
            return this.loginUseBO.UpdatePasswordStatus(id);

        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ImportData(UploadInvoice)'
        public ResultImportSheet ImportData(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ImportData(UploadInvoice)'
        {
            byte[] data = Convert.FromBase64String((fileImport.Data.IsNullOrEmpty() ? string.Empty : fileImport.Data));
            var sizeOfFile = data.Length / 1024;
            if (sizeOfFile > Config.ApplicationSetting.Instance.ImportFileMaxSize)
            {
                throw new BusinessLogicException(ResultCode.ImportDataSizeOfFileTooLarge, "Size of the selected file is too large");
            }
            return null;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ImportFile(string)'
        public ResultImportSheet ImportFile(string fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.ImportFile(string)'
        {
            long companyId = GetCompanyIdOfUser();
            string fullPathfile = SaveFileImportLoginUser(fileImport);
            return this.loginUseBO.ImportData(fullPathfile, companyId, this.CurrentUser);
        }
        private static string SaveFileImportLoginUser(string fileImport)
        {
            string FolderPath = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.UploadFolderImport);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            string fileName = string.Format(@"{0}\Invoice{1}.xlsx", FolderPath, DateTime.Now.ToString("ddMMyyyy"));
            try
            {
                File.WriteAllBytes(fileName, Convert.FromBase64String(fileImport));
            }
            catch (Exception ex)
            {

                throw new BusinessLogicException(ResultCode.FileNotFound, ex.Message);
            }

            return fileName;
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.DownloadClientAccount(ConditionSearchUser)'
        public ExportFileInfo DownloadClientAccount(ConditionSearchUser condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AccountBusiness.DownloadClientAccount(ConditionSearchUser)'
        {
            ExportFileInfo fileInfo = this.clientAccountBO.DownloadClientAccount(condition);
            return fileInfo;
        }
        #endregion
    }
}