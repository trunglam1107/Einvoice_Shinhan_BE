using InvoiceServer.Business.BL;
using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace InvoiceServer.API.Business.Portal
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness'
    public class InvoicePortalBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness'
    {
        private readonly IInvoicePortalBO invoicePortalBO;
        private readonly IMyCompanyBO companyBo;
        private readonly ISessionManagerBO sessionManagerBO;
        private readonly ISignatureBO signatureBO;
        private readonly IClientBO clientUseBO;
        private readonly IEmailActiveBO emailActiveBO;
        private readonly IReportCancellingBO reportCancellingBO;
        private readonly ISystemSettingBO systemSettingBO;
        private readonly static Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];
        private readonly static Logger logger = new Logger();
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.InvoicePortalBusiness(IBOFactory)'
        public InvoicePortalBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.InvoicePortalBusiness(IBOFactory)'
        {
            this.invoicePortalBO = boFactory.GetBO<IInvoicePortalBO>();

            var uploadImageConfig = new UpdateloadImageConfig
            {
                MaxSizeImage = Config.ApplicationSetting.Instance.MaxSizeImage,
                RootFolderUpload = DefaultFields.ASSET_FOLDER
            };
            this.companyBo = boFactory.GetBO<IMyCompanyBO>(uploadImageConfig);
            var config = new SessionConfig()
            {
                LoginSecretKey = Config.ApplicationSetting.Instance.LoginSecretKey,
                SessionTimeout = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.SessionTimeout),
                CreateTokenRandom = Config.ApplicationSetting.Instance.EnableCreateTokenRandom,
                SessionResetPasswordExpire = TimeSpan.FromMinutes(Config.ApplicationSetting.Instance.ResetPasswordTimeOut)
            };
            this.sessionManagerBO = boFactory.GetBO<ISessionManagerBO>(config);
            this.signatureBO = boFactory.GetBO<ISignatureBO>();
            PrintConfig printConfig = GetPrintConfig();
            EmailConfig emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.clientUseBO = boFactory.GetBO<IClientBO>(printConfig, emailConfig);
            this.emailActiveBO = boFactory.GetBO<IEmailActiveBO>(printConfig);
            this.reportCancellingBO = boFactory.GetBO<IReportCancellingBO>();
            this.systemSettingBO = boFactory.GetBO<ISystemSettingBO>();

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetInvoiceByVeriCode(string)'
        public IEnumerable GetInvoiceByVeriCode(string VerificationCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetInvoiceByVeriCode(string)'
        {
            return this.invoicePortalBO.GetInvoiceByVeriCode(VerificationCode);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetInvoiceByVeriCodeAndClientId(string, long, string, string)'
        public IEnumerable GetInvoiceByVeriCodeAndClientId(string VerificationCode, long ClientId, string DateFrom, string DateTo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetInvoiceByVeriCodeAndClientId(string, long, string, string)'
        {
            return this.invoicePortalBO.GetInvoiceByVeriCodeAndClientId(VerificationCode, ClientId, DateFrom, DateTo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.SearchInvoiceByInvoiceNo(string, string, string, string)'
        public IEnumerable SearchInvoiceByInvoiceNo(string InvoiceNo, string taxcode, string DateFrom, string DateTo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.SearchInvoiceByInvoiceNo(string, string, string, string)'
        {
            return this.invoicePortalBO.SearchInvoiceByInvoiceNo(InvoiceNo, taxcode, DateFrom, DateTo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.ExcelInvoiceNoClient(string, long, string, string)'
        public ExportFileInfo ExcelInvoiceNoClient(string InvoiceNo, long ClientID, string DateFrom, string DateTo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.ExcelInvoiceNoClient(string, long, string, string)'
        {
            ExportFileInfo fileInfo = this.invoicePortalBO.ExcelInvoiceNoClient(InvoiceNo, ClientID, DateFrom, DateTo);
            return fileInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.ExcelInvoiceByVeriCodeLogin(string, long, string, string)'
        public ExportFileInfo ExcelInvoiceByVeriCodeLogin(string InvoiceNo, long ClientID, string DateFrom, string DateTo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.ExcelInvoiceByVeriCodeLogin(string, long, string, string)'
        {
            ExportFileInfo fileInfo = this.invoicePortalBO.ExcelInvoiceByVeriCodeLogin(InvoiceNo, ClientID, DateFrom, DateTo);
            return fileInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetAnnounByVeriCode(string)'
        public IEnumerable GetAnnounByVeriCode(string VerificationCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetAnnounByVeriCode(string)'
        {
            return this.invoicePortalBO.GetAnnounByVeriCode(VerificationCode);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetAnnounByVeriCodeAndClientId(string, long, string, string)'
        public IEnumerable GetAnnounByVeriCodeAndClientId(string VerificationCode, long clientId, string DateFrom, string DateTo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetAnnounByVeriCodeAndClientId(string, long, string, string)'
        {
            return this.invoicePortalBO.GetAnnounByVeriCodeAndClientId(VerificationCode, clientId, DateFrom, DateTo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetAnnounByInvoiceNo(string, long, string, string)'
        public IEnumerable GetAnnounByInvoiceNo(string InvoiceNo, long ClientID, string DateFrom, string DateTo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetAnnounByInvoiceNo(string, long, string, string)'
        {
            return this.invoicePortalBO.GethAnnounByInvoiceNo(InvoiceNo, ClientID, DateFrom, DateTo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetToken(string, long)'
        public string GetToken(string Code, long companyID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetToken(string, long)'
        {
            return this.invoicePortalBO.GetToken(Code, companyID);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetTokenByInvoiceCode(string, long, long)'
        public string GetTokenByInvoiceCode(string Code, long companyID, long ClientID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetTokenByInvoiceCode(string, long, long)'
        {
            return this.invoicePortalBO.GetTokenByInvoiceCode(Code, companyID, ClientID);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetCompanyIdByInvoiceVerifiCode(string)'
        public long? GetCompanyIdByInvoiceVerifiCode(string VerifiCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetCompanyIdByInvoiceVerifiCode(string)'
        {
            try
            {
                return this.invoicePortalBO.GetCompanyIdByInvoiceVerifiCode(VerifiCode);

            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return 0;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetCompanyIdByAnnounVerifiCode(string)'
        public long? GetCompanyIdByAnnounVerifiCode(string VerifiCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetCompanyIdByAnnounVerifiCode(string)'
        {
            try
            {
                return this.invoicePortalBO.GetCompanyIdByAnnounVerifiCode(VerifiCode);

            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return 0;
            }
           
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetClientInfo(long)'
        public ClientAddInfo GetClientInfo(long ClientId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetClientInfo(long)'
        {
            var client = this.clientUseBO.GetClientInfo(ClientId);
            return client;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.UpdataStatusReleaseDetailInvoice(long)'
        public ResultCode UpdataStatusReleaseDetailInvoice(long Code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.UpdataStatusReleaseDetailInvoice(long)'
        {
            return this.invoicePortalBO.UpdataStatusReleaseDetailInvoice(Code);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.AnnouncementUpdataStatusClientSign(long)'
        public ResultCode AnnouncementUpdataStatusClientSign(long announcementId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.AnnouncementUpdataStatusClientSign(long)'
        {
            return this.invoicePortalBO.AnnouncementUpdataStatusClientSign(announcementId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetMyCompanyInfo(long)'
        public MyCompanyInfo GetMyCompanyInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetMyCompanyInfo(long)'
        {
            var company = this.companyBo.GetCompanyOfUser(id, id);
            company.Signatures = this.signatureBO.GetList(id).ToList();
            return company;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.UpdateClientInfo(long, ClientAddInfo)'
        public ResultCode UpdateClientInfo(long id, ClientAddInfo clientInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.UpdateClientInfo(long, ClientAddInfo)'
        {
            if (clientInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            clientInfo.UserAction = 999999; //this.CurrentUser.Id; truong hop khach hang update
            return this.invoicePortalBO.UpdateClientInfo(id, clientInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.UpdateClientPassword(long, ChangePassword)'
        public ResultCode UpdateClientPassword(long userId, ChangePassword passwordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.UpdateClientPassword(long, ChangePassword)'
        {
            if (passwordInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.sessionManagerBO.UpdateClientPassword(userId, passwordInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.ResetClientPassword(ResetPassword)'
        public ResultCode ResetClientPassword(ResetPassword resetPasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.ResetClientPassword(ResetPassword)'
        {
            ResultCode resultCode;
            resultCode = this.sessionManagerBO.ResetClientPassword(resetPasswordInfo);
            if (resultCode == ResultCode.NoError)
            {
                var emailActiveId = this.clientUseBO.CreateEmailActiveByClientId(resetPasswordInfo.ClientId);
                var clientAddInfo = new ClientAddInfo()
                {
                    Id = resetPasswordInfo.ClientId,
                    EmailActiveId = emailActiveId
                };
                resultCode = this.emailActiveBO.SendEmailResetPassword(clientAddInfo);
                if (resultCode == ResultCode.NoError)
                {
                    //encrypt password after send email account infomation
                    resultCode = this.clientUseBO.EncryptUserPassword(resetPasswordInfo.ClientId);
                }
            }
            return resultCode;
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetFooterInfo()'
        public PTFooterInfo GetFooterInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.GetFooterInfo()'
        {
            // get config data in Web.config file
            var PTFooterInfo = new PTFooterInfo()
            {
                FooterCompanyName = Config.ApplicationSetting.Instance.PTCompanyName,
                FooterCompanyNameEN = Config.ApplicationSetting.Instance.PTCompanyNameEN,
                Email = Config.ApplicationSetting.Instance.Email,
                FooterPhone = Config.ApplicationSetting.Instance.FooterPhone,
                PhoneEmergency = Config.ApplicationSetting.Instance.PhoneEmergency,
                AddressStreet = Config.ApplicationSetting.Instance.AddressStreet,
                AddressStreetEN = Config.ApplicationSetting.Instance.AddressStreetEN,
                AddressProvince = Config.ApplicationSetting.Instance.AddressProvince,
                AddressProvinceEN = Config.ApplicationSetting.Instance.AddressProvinceEN,
                Youtube = Config.ApplicationSetting.Instance.Youtube,
                Facebook = Config.ApplicationSetting.Instance.Facebook,
                LinkAddressTransaction = Config.ApplicationSetting.Instance.LinkAddressTransaction,
                TextCopyright = Config.ApplicationSetting.Instance.TextCopyright,
                LinkPrivacyPolicy = Config.ApplicationSetting.Instance.LinkPrivacyPolicy,
                LinkTermsOfUse = Config.ApplicationSetting.Instance.LinkTermsOfUse,
            };

            return PTFooterInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.SendEmailClientAccountInfo(int)'
        public ResultCode SendEmailClientAccountInfo(int isJob)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.SendEmailClientAccountInfo(int)'
        {
            ResultCode result = ResultCode.NoError;
            var listClient = this.invoicePortalBO.GetListClientNotChangePass();
            logger.QuarztJob(false, $"List client for email count : {listClient.Count()}");
            foreach (var client in listClient)
            {
                //logger.Error("Client receviced invoice current : " + client.ReceivedInvoiceEmail, new Exception(""));
                //logger.Error("Client email current : " + client.Email, new Exception(""));
                if (!string.IsNullOrEmpty(client.ReceivedInvoiceEmail))
                {
                    if (!client.ReceivedInvoiceEmail.Contains(","))
                    {
                        logger.Error("Single mail : " + client.ReceivedInvoiceEmail, new Exception(""));
                        if (!string.IsNullOrEmpty(client.ReceivedInvoiceEmail))
                        {
                            if (client.ReceivedInvoiceEmail.Contains("@"))
                            {
                                var emailAccountId = emailActiveBO.checkSentEmail((long)client.ClientID);

                                // email exists then update
                                //if (emailAccountId.HasValue)
                                //{
                                //    this.clientUseBO.UpdateEmailActive((long)client.ClientID, (long)emailAccountId);
                                //}
                                //else
                                //{

                                //}

                                emailAccountId = this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);

                                ClientAddInfo clientAddInfo = new ClientAddInfo();
                                clientAddInfo.Id = (long)client.ClientID;
                                clientAddInfo.EmailActiveId = (long)emailAccountId;
                                result = this.emailActiveBO.SendEmailProvideAccount(clientAddInfo);
                                if (result == ResultCode.NoError)
                                {
                                    //encrypt password after send email account infomation
                                    result = this.clientUseBO.EncryptUserPassword((long)client.ClientID);
                                }

                            }
                            else
                            {
                                this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);
                            }
                        }
                        else
                        {
                            this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);
                        }
                    }
                    else if (client.ReceivedInvoiceEmail.Contains(","))
                    {
                        logger.Error("Multiple mail contains , : " + client.ReceivedInvoiceEmail, new Exception(""));

                        string[] lstRecevierEmail = client.ReceivedInvoiceEmail.Split(',');
                        
                        if (lstRecevierEmail.Count() >= 0)
                        {
                            foreach (var RecevierEmail in lstRecevierEmail)
                            {
                                if (!string.IsNullOrEmpty(RecevierEmail))
                                {
                                    logger.Error("RecevierEmail  : " + RecevierEmail, new Exception(""));

                                    if (RecevierEmail.Contains("@"))
                                    {
                                        try
                                        {
                                            var emailAccountId = emailActiveBO.checkSentEmail((long)client.ClientID);

                                            // email exists then update
                                            //if (emailAccountId.HasValue)
                                            //{
                                            //    this.clientUseBO.UpdateEmailActive((long)client.ClientID, (long)emailAccountId);
                                            //}
                                            //else
                                            //{
                                                
                                            //}
                                            emailAccountId = this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);
                                            ClientAddInfo clientAddInfo = new ClientAddInfo();
                                            clientAddInfo.Id = (long)client.ClientID;
                                            clientAddInfo.EmailActiveId = (long)emailAccountId;
                                            logger.Error("EmailActiveId : " + clientAddInfo.EmailActiveId.ToString(), new Exception(""));
                                            result = this.emailActiveBO.SendEmailProvideAccount(clientAddInfo);
                                            if (result == ResultCode.NoError)
                                            {
                                                //encrypt password after send email account infomation
                                                result = this.clientUseBO.EncryptUserPassword((long)client.ClientID);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            logger.Error("Multiple mail contains , : " + client.ReceivedInvoiceEmail, new Exception(""));
                                        }
                                    }
                                    else
                                    {
                                        this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);
                                    }
                                }
                                else
                                {
                                    this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);
                                }
                            }
                        }
                        else
                        {
                            this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);
                        }
                    }
                    else
                    {
                        this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);

                        
                    }
                }
                else
                {
                    this.clientUseBO.CreateEmailActiveByClientId((long)client.ClientID, client.CompanyId);
                }
            }

            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.SendMailWarning()'
        public ResultCode SendMailWarning()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalBusiness.SendMailWarning()'
        {
            ResultCode result = ResultCode.NoError;
            //function GetActiveSystemSetting can param [companyID] nhung k su dung -> truyen 1 vao
            var systemSettingInfo = this.systemSettingBO.GetActiveSystemSetting(1);
            if (systemSettingInfo != null && !systemSettingInfo.IsWarningMinimum)
            {
                return result;
            }
            var listContent = new List<NotificationUsedInvoiceDetail>();
            // get tat ca cac mau da dang ky
            var listTemplateUsed = this.invoicePortalBO.GetNotificationUseInvoiceDetails();
            foreach (var templateUsed in listTemplateUsed)
            {
                var condition = new ConditionInvoiceUse((long)templateUsed.CompanyId, templateUsed.RegisterTemplatesId, templateUsed.NotificationUseCode);
                // check da tung gui mail warning chua
                //var isSend = invoicePortalBO.wasSendWarning(condition);
                //if (isSend)
                //{
                //    continue;
                //}
                templateUsed.NumberUse = this.invoicePortalBO.GetNumberInvoiceReleased(condition);
                //templateUsed.NumberCancelling = this.reportCancellingBO.GetNumberInvoiceCancelling(condition);
                var numBalance = templateUsed.NumberRegister - templateUsed.NumberUse;
                if (systemSettingInfo != null && numBalance <= systemSettingInfo.NumberMinimum)
                {
                    listContent.Add(templateUsed);
                }
            }
            var email = systemSettingInfo?.EmailReceiveWarning;
            if (!string.IsNullOrEmpty(email) && listContent.Count > 0)
            {
                result = this.invoicePortalBO.SendEmailWarning(listContent, email, this.CurrentUser.EmailServer);
                //if (result == ResultCode.NoError)
                //{
                //    //update ISRECEIVEDWARNING cua table NOTIFICATIONUSEINVOICEDETAIL
                //    this.invoicePortalBO.UpdateStatusReceiveWarning(listContent);
                //}
            }

            return result;
        }

        /// <summary>
        /// GetConfigHidedSign - Get con fig to hide sign on portal
        /// </summary>
        /// <returns>HidedSignPortal</returns>
        public string GetConfigSignPortal()
        {
            return GetConfig("SignPortal") ?? "false";
        }

    }
}