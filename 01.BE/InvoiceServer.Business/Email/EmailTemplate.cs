using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InvoiceServer.Business.Email
{
    public static class EmailTemplate
    {
        private static readonly Logger logger = new Logger();
        // get config data in Web.config file
        private static readonly Func<string, string> GetConfig = key => CommonUtil.GetConfig(key);
        public static EmailInfo GetEmail(EmailConfig emailConfig, int typeEmail, ReceiverInfo receiverInfo)
        {
            string templateEmail = GetTemplateFile(typeEmail);
            EmailInfo emailInfo = GetEmailInfo(emailConfig.FolderEmailTemplate, templateEmail);
            StandardizedContentEmail(emailInfo, receiverInfo);
            return emailInfo;
        }

        public static EmailInfo GetEmail(EmailConfig emailConfig, int typeEmail, ReceiverAccountActiveInfo receiverInfo)
        {
            string templateEmail = GetTemplateFile(typeEmail);
            EmailInfo emailInfo = GetEmailInfo(emailConfig.FolderEmailTemplate, templateEmail);
            StandardizedContentEmail(emailInfo, receiverInfo);
            return emailInfo;
        }

        public static EmailInfo GetEmail(EmailConfig emailConfig, int typeEmail, SendEmailVerificationCode receiverInfo)
        {
            string templateEmail = GetTemplateFile(typeEmail);
            EmailInfo emailInfo = GetEmailInfo(emailConfig.FolderEmailTemplate, templateEmail);
            emailInfo.RefIds.Add(receiverInfo.InvoiceId);    // Task #30077
            StandardizedContentEmail(emailInfo, receiverInfo);
            return emailInfo;
        }

        public static EmailInfo GetEmail(EmailConfig emailConfig, int typeEmail, SendEmailVerificationCodeAnnoun receiverInfo)
        {
            string templateEmail = GetTemplateFile(typeEmail);
            EmailInfo emailInfo = GetEmailInfo(emailConfig.FolderEmailTemplate, templateEmail);
            StandardizedContentEmail(emailInfo, receiverInfo);
            return emailInfo;
        }

        public static EmailInfo GetEmail(EmailConfig emailConfig, string typeEmail, ReceiverInfo receiverInfo)
        {
            EmailInfo emailInfo = GetEmailInfo(emailConfig.FolderEmailTemplate, string.Format("{0}.txt", typeEmail));
            StandardizedContentEmail(emailInfo, receiverInfo);
            return emailInfo;
        }

        public static EmailInfo GetEmail(EmailConfig emailConfig, int typeEmail, SendEmailRelease receiverInfo)
        {
            string templateEmail = GetTemplateFile(typeEmail);
            EmailInfo emailInfo = GetEmailInfo(emailConfig.FolderEmailTemplate, templateEmail);
            StandardizedContentEmail(emailInfo, receiverInfo);
            return emailInfo;
        }
        public static EmailInfo GetEmail(EmailConfig emailConfig, int typeEmail, CLIENT client, List<InvoiceSendByMonth> invoiceSends)
        {
            string templateEmail = GetTemplateFile(typeEmail);
            EmailInfo emailInfo = GetEmailInfo(emailConfig.FolderEmailTemplate, templateEmail);

            // Task #30077
            List<long> refIds = invoiceSends.Select(x => (long)x.InvoiceId).ToList();
            emailInfo.RefIds = refIds;

            StandardizedContentEmail(emailInfo, client, invoiceSends);
            return emailInfo;
        }

        #region Private methods

        private static string GetTemplateFile(int emailType)
        {
            string template = string.Empty;
            switch (emailType)
            {
                case (int)SendEmailType.NoticeAdjustInfomation:
                    template = "Notice_AdjustmentInfomation_Invoice.txt";
                    break;
                case (int)SendEmailType.NoticeAdjustDown:
                    template = "Notice_Down_Invoice.txt";
                    break;
                case (int)SendEmailType.NoticeAdjustUp:
                    template = "Notice_Up_Invoice.txt";
                    break;
                case (int)SendEmailType.NoticeAdjustSubstitute:
                    template = "Notice_Substitute_Invoice.txt";
                    break;
                case (int)SendEmailType.NoticeAccountCustomer:
                    template = "Customer_Active_account.txt";
                    break;
                case (int)SendEmailType.NoticeAccountSeller:
                    template = "Seller_Active_account.txt";
                    break;
                case (int)SendEmailType.SendVerificationCode:
                    template = "Notice_VerificationCode.txt";
                    break;
                case (int)SendEmailType.SendVerificationCodeAnnouncementAdjust:
                    template = "Notice_VerificationCodeAnnouncement.txt";
                    break;
                case (int)SendEmailType.SendVerificationCodeAnnouncementReplace:
                    template = "Notice_VerificationCodeAnnouncement.txt";
                    break;
                case (int)SendEmailType.SendVerificationCodeAnnouncementCancel:
                    template = "Notice_VerificationCodeAnnouncement.txt";
                    break;
                case (int)SendEmailType.SendVerificationCodeSendMonth:
                    template = "Notice_VerificationCodeSendMonth.txt";
                    break;
                case (int)SendEmailType.SendVerificationCodeSino:
                    template = "Sino_Notice_VerificationCode.txt";
                    break;
                default:
                    template = "Notice_Release_Invoice.txt";
                    break;
            }

            return template;
        }

        private static EmailInfo GetEmailInfo(string fullFilePathTempale, string templateEmail)
        {
            try
            {
                var emailContent = new EmailInfo();
                var fullFilePath = GetFullFilePath(fullFilePathTempale, templateEmail);
                if (!File.Exists(fullFilePath))
                {
                    throw new BusinessLogicException(ResultCode.FileNotFound, "File not exist" + fullFilePath);
                }

                string[] contentTemplates = System.IO.File.ReadAllLines(fullFilePath);
                StringBuilder contentEmail = new StringBuilder();
                int numberOfDocument = 0;
                foreach (var line in contentTemplates)
                {
                    if (numberOfDocument == 0)
                    {
                        emailContent.Subject = line;
                    }
                    else
                    {
                        contentEmail.AppendLine(line);
                    }

                    numberOfDocument++;
                }

                if (contentEmail.Length == 0 || emailContent.Subject.IsNullOrEmpty())
                {
                    throw new BusinessLogicException(ResultCode.DataInvalid, "Template file Invalid format");
                }

                emailContent.Content = contentEmail.ToString();

                return emailContent;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message, fullFilePathTempale, templateEmail);
                return null;
            }
        }

        private static string GetFullFilePath(string fullFilePathTempale, string fileName)
        {
            return Path.Combine(fullFilePathTempale, fileName);
        }

        private static void StandardizedContentEmail(EmailInfo emailInfo, ReceiverInfo receiverInfo)
        {
            emailInfo.EmailTo = receiverInfo.Email;
            emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderEmail, receiverInfo.Email)
                .Replace(EmailType.PlaceHolderUrl, receiverInfo.UrlResetPassword)
                .Replace(EmailType.PlaceHolderUserId, receiverInfo.UserId);
        }

        private static void StandardizedContentEmail(EmailInfo emailInfo, SendEmailRelease receiverInfo)
        {
            emailInfo.EmailTo = receiverInfo.Email;
            emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderEmail, receiverInfo.Email)
                .Replace(EmailType.PlaceHolderCompanyName, receiverInfo.CompanyName)
                .Replace(EmailType.PlaceHolderCustomerName, receiverInfo.CustomerName)
                .Replace(EmailType.PlaceHolderInvoiceNo, receiverInfo.InvoiceNo)
                .Replace(EmailType.PlaceHolderInvoicePattern, receiverInfo.InvoicePattern)
                .Replace(EmailType.PlaceHolderInvoiceSeria, receiverInfo.InvoiceSeria)
                .Replace(EmailType.PlaceHolderInvoiceNoOld, receiverInfo.InvoiceNoOld)
                .Replace(EmailType.PlaceHolderInvoicePatternOld, receiverInfo.InvoicePatternOld)
                .Replace(EmailType.PlaceHolderInvoiceLinkDowLoad, string.Empty)
                .Replace(EmailType.PlaceHolderInvoiceSeriaOld, receiverInfo.InvoiceSeriaOld);
        }

        private static void StandardizedContentEmail(EmailInfo emailInfo, ReceiverAccountActiveInfo receiverInfo)
        {
            emailInfo.Subject = emailInfo.Subject.Replace(EmailType.PlaceHolderBankName, GetConfig("BankName"));
            emailInfo.EmailTo = receiverInfo.Email;
            emailInfo.Content = emailInfo.Content.Replace(EmailTypeAccountInfo.PlaceHolderCustomerName, receiverInfo.CompanyName)
                .Replace(EmailType.PlaceHolderBankAddress, GetConfig("BankAddress"))
                .Replace(EmailType.PlaceHolderBankAddressEn, GetConfig("BankAddressEn"))
                .Replace(EmailType.PlaceHolderBankName, GetConfig("BankName"))
                .Replace(EmailType.PlaceHolderBankNameEn, GetConfig("BankNameEn"))
                .Replace(EmailType.PlaceHolderBankTnhh, GetConfig("TnhhMtv"))
                .Replace(EmailType.PlaceHolderBankPhone, GetConfig("BankPhone"))
                .Replace(EmailTypeAccountInfo.PlaceHolderCustomerName, receiverInfo.CustomerName)
                .Replace(EmailTypeAccountInfo.PlaceHolderUserId, receiverInfo.UserId)
                .Replace(Resources.PlaceHolderPassword, receiverInfo.Password)
                .Replace(EmailType.PlaceHolderLinkPortalShow, GetConfig("PortalCompanyLinkText"))
                .Replace(EmailType.PlaceHolderLinkPortal, GetConfig("PortalCompanyLink"))
                .Replace(EmailType.PlaceHolderIsHavePortal, CommonUtil.GetConfig("IsHavePublicPortal", true) ? "inherit" : "none");
            InsertLogoToEmail(emailInfo);
        }

        private static void StandardizedContentEmailSino(EmailInfo emailInfo, SendEmailVerificationCode receiverInfo)
        {
            emailInfo.Subject = emailInfo.Subject.Replace(EmailType.PlaceHolderInvoiceDate, receiverInfo.InvoiceDate.ToString("yyyyMMdd"))
                                                 .Replace(EmailType.PlaceHolderInvoiceNo, receiverInfo.InvoiceNo);
            emailInfo.EmailTo = receiverInfo.EmailTo;
            emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderEmail, receiverInfo.EmailTo)
                .Replace(EmailType.PlaceHolderBankAddress, GetConfig("BankAddress"))
                .Replace(EmailType.PlaceHolderBankAddressEn, GetConfig("BankAddressEn"))
                .Replace(EmailType.PlaceHolderBankPhone, GetConfig("BankPhone"));
            InsertLogoToEmail(emailInfo);
        }
        private static void StandardizedContentEmail(EmailInfo emailInfo, SendEmailVerificationCode receiverInfo)
        {
            emailInfo.Subject = emailInfo.Subject.Replace(EmailType.PlaceHolderCompanyName, receiverInfo.CompanyName)
                                                 .Replace(EmailType.PlaceHolderCustomerName, receiverInfo.CustomerName)
                                                 .Replace(EmailType.PlaceHolderVerificationCode, receiverInfo.VerificationCode)
                                                 .Replace(EmailType.PlaceHolderBankName, GetConfig("BankName"));
            emailInfo.EmailTo = receiverInfo.EmailTo;
            emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderEmail, receiverInfo.EmailTo)
                .Replace(EmailType.PlaceHolderBankAddress, GetConfig("BankAddress"))
                .Replace(EmailType.PlaceHolderBankAddressEn, GetConfig("BankAddressEn"))
                .Replace(EmailType.PlaceHolderBankName, GetConfig("BankName"))
                .Replace(EmailType.PlaceHolderBankNameEn, GetConfig("BankNameEn"))
                .Replace(EmailType.PlaceHolderBankTnhh, GetConfig("TnhhMtv"))
                .Replace(EmailType.PlaceHolderBankPhone, GetConfig("BankPhone"))
                .Replace(EmailType.PlaceHolderCompanyName, receiverInfo.CompanyName)
                .Replace(EmailType.PlaceHolderCustomerName, receiverInfo.CustomerName)
                .Replace(EmailType.PlaceHolderInvoiceNo, receiverInfo.InvoiceNo)
                .Replace(EmailType.PlaceHolderInvoicePattern, receiverInfo.InvoiceCode)
                .Replace(EmailType.PlaceHolderInvoiceSeria, receiverInfo.Symbol)
                .Replace(EmailType.PlaceHolderInvoiceDate, receiverInfo.InvoiceDate.ToString("dd/MM/yyyy"))
                .Replace(EmailType.PlaceHolderInvoiceTotal, String.Format(FormatNumber.GetStringFormat(receiverInfo.SystemSetting.Amount), receiverInfo.Total.ToDecimal()))
                .Replace(EmailType.PlaceHolderInvoiceTotalTax, String.Format(FormatNumber.GetStringFormat(receiverInfo.SystemSetting.Amount), receiverInfo.TotalTax.ToDecimal()))
                .Replace(EmailType.PlaceHolderInvoiceSum, String.Format(FormatNumber.GetStringFormat(receiverInfo.SystemSetting.Amount), receiverInfo.Sum.ToDecimal()))
                .Replace(EmailType.PlaceHolderClientId, receiverInfo.ClientUser)
                .Replace(Resources.PlaceHolderClientPassword, receiverInfo.ClientPassword)
                .Replace(EmailType.PlaceHolderVerificationCode, receiverInfo.VerificationCode)
                .Replace(EmailType.PlaceHolderLinkPortalShow, GetConfig("PortalCompanyLinkText"))
                .Replace(EmailType.PlaceHolderLinkPortal, GetConfig("PortalCompanyLink"))
                .Replace(EmailType.PlaceHolderIsHavePortal, CommonUtil.GetConfig("IsHavePublicPortal", true) ? "inherit" : "none");
            InsertLogoToEmail(emailInfo);

            emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderTextPass, EmailType.TextPass())
                .Replace(EmailType.PlaceHolderTextPassEn1, EmailType.TextPassEn1())
                .Replace(EmailType.PlaceHolderTextPassEn2, EmailType.TextPassEn2())
                .Replace(EmailType.PlaceHolderValuePass, EmailType.ValuePass(receiverInfo.IsOrg))
                .Replace(EmailType.PlaceHolderValuePassEn, EmailType.ValuePassEn(receiverInfo.IsOrg));

        }
        private static void StandardizedContentEmail(EmailInfo emailInfo, SendEmailVerificationCodeAnnoun receiverInfo)
        {
            //Subject
            emailInfo.Subject = emailInfo.Subject.Replace(EmailTypeAnnoun.PlaceHolderMinutesType, receiverInfo.AnnouncementTitle)
                .Replace(EmailTypeAnnoun.PlaceHolderCustomerName, receiverInfo.CustomerName)
                .Replace(EmailTypeAnnoun.PlaceHolderVerificationCode, receiverInfo.VerificationCode)
                .Replace(EmailType.PlaceHolderBankName, GetConfig("BankName"));
            emailInfo.EmailTo = receiverInfo.EmailTo;
            // Content
            emailInfo.Content = emailInfo.Content.Replace(EmailTypeAnnoun.PlaceHolderEmail, receiverInfo.EmailTo)
                .Replace(EmailType.PlaceHolderBankAddress, GetConfig("BankAddress"))
                .Replace(EmailType.PlaceHolderBankAddressEn, GetConfig("BankAddressEn"))
                .Replace(EmailType.PlaceHolderBankName, GetConfig("BankName"))
                .Replace(EmailType.PlaceHolderBankNameEn, GetConfig("BankNameEn"))
                .Replace(EmailType.PlaceHolderBankTnhh, GetConfig("TnhhMtv"))
                .Replace(EmailType.PlaceHolderBankPhone, GetConfig("BankPhone"))
                .Replace(EmailTypeAnnoun.PlaceHolderCompanyName, receiverInfo.CompanyName)
                .Replace(EmailTypeAnnoun.PlaceHolderCustomerName, receiverInfo.CustomerName)
                .Replace(EmailTypeAnnoun.PlaceHolderNo, receiverInfo.MinutesNo)
                .Replace(EmailTypeAnnoun.PlaceHolderMinutesType, receiverInfo.AnnouncementTitle)
                .Replace(EmailTypeAnnoun.PlaceHolderMinutesDate, receiverInfo.InvoiceDate.ToString("dd/MM/yyyy"))
                .Replace(EmailTypeAnnoun.PlaceHolderVerificationCode, receiverInfo.VerificationCode)
                .Replace(EmailType.PlaceHolderLinkPortalShow, GetConfig("PortalCompanyLinkText"))
                .Replace(EmailType.PlaceHolderLinkPortal, GetConfig("PortalCompanyLink"))
                .Replace(EmailType.PlaceHolderIsHavePortal, CommonUtil.GetConfig("IsHavePublicPortal", true) ? "inherit" : "none");
            InsertLogoToEmail(emailInfo);

            emailInfo.Content = emailInfo.Content.Replace(EmailTypeAnnoun.IsVisibleInvoiceChangedNumber, EmailTypeAnnoun.Visible)
                .Replace(EmailTypeAnnoun.AdjustedInvoiceNumber, receiverInfo.InvoiceNo);

        }
        private static void StandardizedContentEmail(EmailInfo emailInfo, CLIENT client, List<InvoiceSendByMonth> invoiceSends)
        {
            emailInfo.Subject = emailInfo.Subject.Replace(EmailType.PlaceHolderBankName, GetConfig("BankName"));
            StringBuilder detailsInvoice = new StringBuilder();
            var index = 1;
            foreach (var item in invoiceSends)
            {
                detailsInvoice = detailsInvoice.Append("<tr><td style = 'border: 1px solid black;' >" + index + "</td>");
                detailsInvoice = detailsInvoice.Append("<td style = 'border: 1px solid black; color:#00529b' >" + item.InvoiceSymbol + "</td>");
                detailsInvoice = detailsInvoice.Append("<td style = 'border: 1px solid black; color:#00529b' >" + item.InvoiceNo + "</td>");
                detailsInvoice = detailsInvoice.Append("<td style = 'border: 1px solid black; color:#00529b' >" + item.InvoiceDate + "</td>");
                detailsInvoice = detailsInvoice.Append("<td style = 'border: 1px solid black; color:#00529b' >" + item.VerificationCode + "</td></tr>");
                index++;
            }

            emailInfo.EmailTo = client.USEREGISTEREMAIL == true ? client.EMAIL : client.RECEIVEDINVOICEEMAIL;
            emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderCustomerName, client.CUSTOMERNAME ?? client.PERSONCONTACT)
                .Replace(EmailType.PlaceHolderBankAddress, GetConfig("BankAddress"))
                .Replace(EmailType.PlaceHolderBankAddressEn, GetConfig("BankAddressEn"))
                .Replace(EmailType.PlaceHolderBankName, GetConfig("BankName"))
                .Replace(EmailType.PlaceHolderBankNameEn, GetConfig("BankNameEn"))
                .Replace(EmailType.PlaceHolderBankTnhh, GetConfig("TnhhMtv"))
                .Replace(EmailType.PlaceHolderBankPhone, GetConfig("BankPhone"))
                .Replace(EmailType.PlaceHolderInvoiceDetails, detailsInvoice.ToString())
                .Replace(EmailType.PlaceHolderLinkPortalShow, GetConfig("PortalCompanyLinkText"))
                .Replace(EmailType.PlaceHolderLinkPortal, GetConfig("PortalCompanyLink"))
                .Replace(EmailType.PlaceHolderIsHavePortal, CommonUtil.GetConfig("IsHavePublicPortal", true) ? "inherit" : "none");
            InsertLogoToEmail(emailInfo);
        }

        /// <summary>
        /// Nếu bật cờ UseBase64MailImage thì sẽ không chèn URL logo vào email, mà chèn Base64. Dùng cho những bank không có Portal nhưng vẫn gửi mail.
        /// 
        /// Nếu bật cờ InsertLogoWhenCreate thì sẽ insert Base64 vào DB, sẽ làm cho DB tốn nhiều bộ nhớ, nhưng an toàn hơn.
        /// Nếu không bật cờ InsertLogoWhenCreate thì khi gửi mail mới chèn Base64 vào email, không an toàn bằng cách trên nhưng DB sẽ nhẹ hơn (1 hình có thể vài trăm KB đến vài MB, 1 email là 1 hình).
        /// </summary>
        /// <param name="emailInfo"></param>
        private static void InsertLogoToEmail(EmailInfo emailInfo)
        {
            if (CommonUtil.GetConfig("UseBase64MailImage", false))
            {
                if (CommonUtil.GetConfig("InsertLogoWhenCreate", false))
                {
                    emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderLogo, $"data:image/jpeg;base64,{CommonUtil.GetBase64StringImage(DefaultFields.LOGO_MAIL_FILE)}");
                }
            }
            else
            {
                emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderLogo, GetConfig("BankLogoPath"));
            }
        }
        #endregion
    }
}
