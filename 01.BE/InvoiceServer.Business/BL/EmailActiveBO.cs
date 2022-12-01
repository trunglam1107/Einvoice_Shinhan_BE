using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using Ionic.Zip;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace InvoiceServer.Business.BL
{
    public class EmailActiveBO : IEmailActiveBO
    {
        #region Fields, Properties
        private readonly IClientRepository clientRepository;
        private readonly IEmailActiveRepository emailRepository;
        private readonly string linkPortal = WebConfigurationManager.AppSettings["PortalCompanyLink"];
        private readonly string bankNameEn = WebConfigurationManager.AppSettings["BankNameEn"];
        private readonly string sender_email = WebConfigurationManager.AppSettings["SenderEmail"];
        private readonly InvoiceBO invoiceBO;
        private readonly ReleaseAnnouncementBO releaseAnnouncementBO;
        private readonly PrintConfig printConfig;
        private readonly UserSessionInfo userSessionInfo;
        private readonly ILoginUserRepository loginUserRepository;
        private static readonly Logger logger = new Logger();
        private readonly Email email;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly IAnnouncementRepository announcementRepository;
        private const string PassVN = "Mật khẩu";
        private const string PassEN = "Log in password";
        private const string PassWordString = "		<span>{0}: <b style=" + "color: blue" + ">{1}</b></span>";
        #endregion

        #region Contructor
        public EmailActiveBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.emailRepository = repoFactory.GetRepository<IEmailActiveRepository>();
            this.clientRepository = repoFactory.GetRepository<IClientRepository>();
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.email = new Email(repoFactory);
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.announcementRepository = repoFactory.GetRepository<IAnnouncementRepository>();
        }

        public EmailActiveBO(IRepositoryFactory repoFactory, PrintConfig printConfig) : this(repoFactory)
        {
            this.printConfig = printConfig;
        }

        public EmailActiveBO(IRepositoryFactory repoFactory, PrintConfig printConfig, UserSessionInfo userSessionInfo)
            : this(repoFactory, printConfig)
        {
            this.userSessionInfo = userSessionInfo;
            this.releaseAnnouncementBO = new ReleaseAnnouncementBO(repoFactory, printConfig, userSessionInfo);
            this.invoiceBO = new InvoiceBO(repoFactory, printConfig, userSessionInfo);
        }
        #endregion

        #region Methods
        public IEnumerable<EmailActiveInfo> Filter(ConditionSearchEmailActive condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var emailActiveInfos = this.emailRepository.Filter(condition).ToList();
            var emailIdInvoices = new List<long>();
            var emailIdAnnouncements = new List<long>();
            foreach (var emailActiveInfo in emailActiveInfos)
            {
                if (emailActiveInfo.TypeEmail == (int)TypeEmailActive.DailyEmail || emailActiveInfo.TypeEmail == (int)TypeEmailActive.MonthlyEmail)
                {
                    emailIdInvoices.Add(emailActiveInfo.Id);
                }
                if (emailActiveInfo.TypeEmail == (int)TypeEmailActive.AnnouncementEmail)
                {
                    emailIdAnnouncements.Add(emailActiveInfo.Id);
                }
                if (emailActiveInfo.TypeEmail == (int)TypeEmailActive.ProvideAccountEmail)
                {
                    emailIdInvoices.Add(emailActiveInfo.Id);
                }
            }
            var listNoDateInvoice = this.emailRepository.GetListInvoiceNoDateFromInvoice(emailIdInvoices);
            var listNoDateAnnouncement = this.emailRepository.GetListInvoiceNoDateFromAnnouncement(emailIdAnnouncements);

            foreach (var emailActiveInfo in emailActiveInfos)
            {
                if (emailActiveInfo.TypeEmail == (int)TypeEmailActive.DailyEmail || emailActiveInfo.TypeEmail == (int)TypeEmailActive.MonthlyEmail)
                {
                    listNoDateInvoice.TryGetValue(emailActiveInfo.Id, out EmailActiveInfo noDateInvoice);
                    if (noDateInvoice == null) continue;
                    emailActiveInfo.InvoiceNo = noDateInvoice.InvoiceNo;
                    emailActiveInfo.DateInvoice = noDateInvoice.DateInvoice;
                    emailActiveInfo.Teller = noDateInvoice.Teller;
                }
                else if (emailActiveInfo.TypeEmail == (int)TypeEmailActive.AnnouncementEmail)
                {
                    listNoDateAnnouncement.TryGetValue(emailActiveInfo.Id, out EmailActiveInfo noDateAnnouncement);
                    if (noDateAnnouncement == null) continue;
                    emailActiveInfo.InvoiceNo = noDateAnnouncement.InvoiceNo;
                    emailActiveInfo.DateInvoice = noDateAnnouncement.DateInvoice;
                }else if (emailActiveInfo.TypeEmail == (int)TypeEmailActive.ProvideAccountEmail)
                {
                    listNoDateInvoice.TryGetValue(emailActiveInfo.Id, out EmailActiveInfo noDateInvoice);
                    if (noDateInvoice == null) continue;
                    emailActiveInfo.InvoiceNo = noDateInvoice.InvoiceNo;
                    emailActiveInfo.DateInvoice = noDateInvoice.DateInvoice;
                    emailActiveInfo.Teller = noDateInvoice.Teller;
                }
            }

            return emailActiveInfos;
        }

        public long CountFilter(ConditionSearchEmailActive condition, int skip = 0, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.emailRepository.Count(condition);
        }

        public EmailActiveInfo GetEmailActive(long companyId, long id)
        {
            var emailActive = GetById(id);
            var emailActiveInfo = new EmailActiveInfo(emailActive);
            InsertLogoForClientView(emailActiveInfo);
            return emailActiveInfo;
        }

        public ResultCode SendEmails(SendMultiMail emailActives, EmailServerInfo emailServerInfo)
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (var emailActiveId in emailActives.ids)
                    {
                        var emailActive = this.emailRepository.GetById(emailActiveId);
                        var emailActiveInfo = new EmailActiveInfo(emailActive);
                        SendEmail(emailActiveId, emailActiveInfo, emailServerInfo);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(this.userSessionInfo?.UserId, ex);
                    throw;
                }
            });

            return ResultCode.NoError;
        }

        public ResultCode SendEmailProvideAccount(ClientAddInfo clientAddInfo, bool isReSend = false)
        {
            try
            {
                EMAILACTIVE curentEmailActive = GetEmailNotice(clientAddInfo.EmailActiveId);
                List<string> Lists = new List<string>(Regex.Split(curentEmailActive.CONTENTEMAIL, Environment.NewLine));
                ReceiverAccountActiveInfo NoticeAccountInfo = this.clientRepository.GetClientAccountInfo(clientAddInfo.Id);
                if (string.IsNullOrEmpty(curentEmailActive.EMAILTO))
                {
                    return ResultCode.SendMailError;
                }
                if (isReSend)
                {
                    NoticeAccountInfo.Password = CretePassword();
                    var indexVn = Lists.FindIndex(f => f.Contains(PassVN));
                    var indexEn = Lists.FindIndex(f => f.Contains(PassEN));

                    Lists[indexVn] = string.Format(PassWordString, PassVN, NoticeAccountInfo.Password);
                    Lists[indexEn] = string.Format(PassWordString, PassEN, NoticeAccountInfo.Password);
                    curentEmailActive.CONTENTEMAIL = string.Join(Environment.NewLine, Lists.ToArray());

                    this.emailRepository.Update(curentEmailActive);
                }
                bool isSuccess = SendEmailAccountInfo(curentEmailActive, NoticeAccountInfo);
                if (isSuccess && isReSend)
                {
                    LOGINUSER loginUser = this.loginUserRepository.GetByClientId(clientAddInfo.Id);
                    loginUser.PASSWORD = EncryptPassword(NoticeAccountInfo.Password);
                    this.loginUserRepository.Update(loginUser);
                }
                this.emailRepository.Update(curentEmailActive);
                return isSuccess ? ResultCode.NoError : ResultCode.SendMailError;
            }
            catch(Exception ex)
            {
                logger.Error("Error : "+ ResultCode.SendMailError, ex);
                return ResultCode.SendMailError;
            }
            
        }
        private string CretePassword()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
        }
        private string EncryptPassword(string password)
        {
            //encrypt password
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(password));
            byte[] resultHash = md5.Hash;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < resultHash.Length; i++)
            {
                //change it into 2 hexadecimal digits for each byte  
                strBuilder.Append(resultHash[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }
        public ResultCode SendEmailDailySino(long emailActiveId, SendEmailVerificationCode emailVerificationCodeInfos)
        {
            bool isSuccess = ProcessFileSinoDaily(emailActiveId, emailVerificationCodeInfos);
            if (isSuccess)
            {
                this.email.UpdateStatusEmail(emailActiveId);
            }
            return isSuccess ? ResultCode.NoError : ResultCode.SendMailError;
        }
        private bool ProcessFileSinoDaily(long emailActiveId, SendEmailVerificationCode emailVeriCodes)
        {
            try
            {
                string folderSource = Path.Combine(this.printConfig.PathInvoiceFile, emailVeriCodes.CompanyId.ToString(), "FileSendMail");
                if (!Directory.Exists(folderSource))
                {
                    Directory.CreateDirectory(folderSource);
                }
                string folderInvoice = String.Empty;
                EMAILACTIVE curentEmailActive = GetEmailNotice(emailActiveId);
                string fileNameInvoice = emailVeriCodes.InvoiceDate.ToString("yyyyMMdd") + "_" + emailVeriCodes.InvoiceNo;
                folderInvoice = Path.Combine(folderSource, fileNameInvoice);
                if (!Directory.Exists(folderInvoice))
                {
                    Directory.CreateDirectory(folderInvoice);
                }
                List<string> listFile = new List<string>();
                string nameKey = DateTime.Today.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("hhMMssFFF");
                //tạo file txt
                List<string> emails = emailVeriCodes.EmailTo.Split(',').ToList();
                string contentTextFile = String.Empty;
                var index = 1;
                foreach (var item in emails)
                {
                    if (index == emails.Count)
                    {
                        contentTextFile += fileNameInvoice + ".zip|" + item.Trim() + "|" + emailVeriCodes.InvoiceDate.ToString("ddMMyyyy") + "|" + emailVeriCodes.InvoiceNo + "|" + emailVeriCodes.CustomerCode;
                    }
                    else
                    {
                        contentTextFile += fileNameInvoice + ".zip|" + item.Trim() + "|" + emailVeriCodes.InvoiceDate.ToString("ddMMyyyy") + "|" + emailVeriCodes.InvoiceNo + "|" + emailVeriCodes.CustomerCode + @"
";
                    }
                    index++;
                }
                string pathFileText = Path.Combine(folderSource, nameKey + ".txt");
                CreateTextFile(pathFileText, contentTextFile);
                listFile.Add(pathFileText);

                //tạo file .zip
                List<FileInformation> Files = GetFileAttach(curentEmailActive);
                listFile.Add(CreateZipFile(folderInvoice, emailVeriCodes.CustomerCode, Files));

                //Tạo file .ok
                string pathFileOk = Path.Combine(folderSource, nameKey + ".ok");
                string contentOkFile = nameKey + ".txt," + emails.Count.ToString();
                CreateTextFile(pathFileOk, contentOkFile);
                listFile.Add(pathFileOk);

                //copy file to bill hunter
                CopyFileToFTP(listFile);
            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Invoice", ex);
                return false;
            }
            return true;
        }
        public ResultCode SendEmailSino(List<SendEmailVerificationCode> emailVerificationCodeInfos)
        {
            bool isSuccess = ProcessFileSino(emailVerificationCodeInfos);
            return isSuccess ? ResultCode.NoError : ResultCode.SendMailError;
        }
        private bool ProcessFileSino(List<SendEmailVerificationCode> emailVeriCodes)
        {
            try
            {
                string folderSource = Path.Combine(this.printConfig.PathInvoiceFile, emailVeriCodes[0].CompanyId.ToString(), "FileSendMail");
                if (!Directory.Exists(folderSource))
                {
                    Directory.CreateDirectory(folderSource);
                }
                string folderInvoice = String.Empty;
                string contentTextFile = String.Empty;
                var index = 1;
                var last = emailVeriCodes.Count;
                var countEmail = 0;
                List<string> listFile = new List<string>();
                foreach (var inv in emailVeriCodes)
                {
                    EMAILACTIVE curentEmailActive = GetEmailNotice(inv.EmailActiveId);
                    string fileNameInvoice = inv.InvoiceDate.ToString("yyyyMMdd") + "_" + inv.InvoiceNo;
                    List<string> emails = inv.EmailTo.Split(',').ToList();
                    if (emails.Count > 1)
                    {
                        var i = 1;
                        foreach (var item in emails)
                        {
                            if (i == emails.Count && index == last)
                            {
                                contentTextFile += fileNameInvoice + ".zip|" + item.Trim() + "|" + inv.InvoiceDate.ToString("yyyyMMdd") + "|" + inv.InvoiceNo + "|" + inv.CustomerCode;
                            }
                            else
                            {

                                contentTextFile += fileNameInvoice + ".zip|" + item.Trim() + "|" + inv.InvoiceDate.ToString("yyyyMMdd") + "|" + inv.InvoiceNo + "|" + inv.CustomerCode + @"
";
                            }
                            i++;
                            countEmail++;
                        }
                    }
                    else
                    {
                        //tạo value file txt
                        if (index == last)
                        {
                            contentTextFile += fileNameInvoice + ".zip|" + inv.EmailTo.Trim() + "|" + inv.InvoiceDate.ToString("yyyyMMdd") + "|" + inv.InvoiceNo + "|" + inv.CustomerCode;
                        }
                        else
                        {

                            contentTextFile += fileNameInvoice + ".zip|" + inv.EmailTo.Trim() + "|" + inv.InvoiceDate.ToString("yyyyMMdd") + "|" + inv.InvoiceNo + "|" + inv.CustomerCode + @"
";
                        }
                        countEmail++;
                    }
                    index++;
                    //tạo file .zip
                    folderInvoice = Path.Combine(folderSource, fileNameInvoice);
                    if (!Directory.Exists(folderInvoice))
                    {
                        Directory.CreateDirectory(folderInvoice);
                    }
                    List<FileInformation> Files = GetFileAttach(curentEmailActive);
                    listFile.Add(CreateZipFile(folderInvoice, inv.CustomerCode, Files));
                }
                string nameKey = DateTime.Today.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("hhMMssFFF");
                string pathFileText = Path.Combine(folderSource, nameKey + ".txt");
                string pathFileOk = Path.Combine(folderSource, nameKey + ".ok");
                CreateTextFile(pathFileText, contentTextFile);
                listFile.Add(pathFileText);

                //Tạo file .ok
                string contentOkFile = nameKey + ".txt," + countEmail;
                CreateTextFile(pathFileOk, contentOkFile);
                listFile.Add(pathFileOk);
            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Invoice", ex);
                return false;
            }
            return true;
        }
        private string CreateZipFile(string folderKey, string taxCode, List<FileInformation> fileInfomations)
        {
            string pathFileTo = String.Empty;
            List<string> files = new List<string>();
            foreach (var file in fileInfomations)
            {
                files.Add(file.FullPathFileattached);
                pathFileTo = Path.Combine(folderKey, Path.GetFileName(file.FileName));
                File.Copy(file.FullPathFileattached, pathFileTo);
            }
            zipFile(taxCode, folderKey, folderKey + ".zip");
            if (Directory.Exists(folderKey))
            {
                Directory.Delete(folderKey, true);
            }
            return folderKey + ".zip";
        }
        private void CreateTextFile(string pathFileText, string contentFile)
        {
            using (FileStream vStream = File.Create(pathFileText))
            {
                // Creates the UTF-8 encoding with parameter "encoderShouldEmitUTF8Identifier" set to true
                Encoding vUTF8Encoding = new UTF8Encoding(true);
                // Gets the preamble in order to attach the BOM
                var vPreambleByte = vUTF8Encoding.GetPreamble();
                // Writes the preamble first
                vStream.Write(vPreambleByte, 0, vPreambleByte.Length);
                // Gets the bytes from text
                byte[] vByteData = vUTF8Encoding.GetBytes(contentFile);
                vStream.Write(vByteData, 0, vByteData.Length);
                vStream.Close();
            }
        }
        public ResultCode SendEmailDaily(long id, SendEmailVerificationCode emailVerificationCodeInfo, bool isJob = false)
        {
            EMAILACTIVE curentEmailActive = GetEmailNotice(id);
            if (string.IsNullOrEmpty(curentEmailActive.EMAILTO))
            {
                return ResultCode.SendMailError;
            }
            bool isSuccess = SendEmailDailyInvoice(curentEmailActive, emailVerificationCodeInfo, isJob);
            return isSuccess ? ResultCode.NoError : ResultCode.SendMailError;
        }
        public ResultCode SendEmailAnoumentDaily(long id, SendEmailVerificationCodeAnnoun emailVerificationCodeInfo)
        {
            EMAILACTIVE curentEmailActive = GetEmailNotice(id);
            if (string.IsNullOrEmpty(curentEmailActive.EMAILTO))
            {
                return ResultCode.SendMailError;
            }
            bool isSuccess = SendEmailDailyAnoument(curentEmailActive, emailVerificationCodeInfo);
            return isSuccess ? ResultCode.NoError : ResultCode.SendMailError;
        }
        public ResultCode SendEmailByMonth(long id, List<SendEmailVerificationCode> emailVerificationCodeInfo, EmailServerInfo emailServerInfo)
        {
            EMAILACTIVE curentEmailActive = GetEmailNotice(id);
            bool isSuccess = false;
            isSuccess = SendEmailByMonth(curentEmailActive, emailServerInfo);
            StatusSendEmail statusSendEmail = isSuccess ? StatusSendEmail.Successfull : StatusSendEmail.Error;
            UpdateStatusSendEmail(id, statusSendEmail);
            return isSuccess ? ResultCode.NoError : ResultCode.UnknownError;
        }
        public ResultCode SendEmailByMonth(long id, List<SendEmailVerificationCode> emailVerificationCodeInfo)
        {
            EMAILACTIVE curentEmailActive = GetEmailNotice(id);
            bool isSuccess = false;
            isSuccess = SendMailMonthly(curentEmailActive, emailVerificationCodeInfo);
            return isSuccess ? ResultCode.NoError : ResultCode.UnknownError;
        }
        #endregion

        private EMAILACTIVE GetById(long id)
        {
            var emailActive = this.emailRepository.GetById(id);
            if (emailActive == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId, MsgApiResponse.DataNotFound);
            }

            return emailActive;
        }

        private bool UpdateEmailActive(EMAILACTIVE currentEmailActive, EmailActiveInfo emailActiveInfo, StatusSendEmail status)
        {
            currentEmailActive.EMAILTO = emailActiveInfo.EmailTo;
            currentEmailActive.SENDTEDDATE = DateTime.Now;
            currentEmailActive.SENDSTATUS = (int)status;
            return this.emailRepository.Update(currentEmailActive);
        }

        private EMAILACTIVE GetEmailActive(long id, EmailActiveInfo emailActiveInfo)
        {
            var currentEmailActive = GetById(id);
            if (currentEmailActive.ID != emailActiveInfo.Id)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            currentEmailActive.EMAILTO = emailActiveInfo.EmailTo;

            return currentEmailActive;
        }

        private EMAILACTIVE GetEmailNotice(long id)
        {
            var currentEmailActive = GetById(id);
            var listEmail = currentEmailActive.EMAILTO.Split(',').Select(x => x.Trim()).Distinct().ToList();
            currentEmailActive.EMAILTO = string.Join(",", listEmail);
            return currentEmailActive;
        }

        private bool SendEmailAccountInfo(EMAILACTIVE emailActive, ReceiverAccountActiveInfo noticeAccountInfo)
        {
            try
            {
                var listEmail = (emailActive.EMAILTO ?? "").Split(new char[] { ',' });
                foreach (var mail in listEmail)
                {
                    if (mail.Contains("@"))
                    {
                        int ecare_no = Int32.Parse(WebConfigurationManager.AppSettings["ProvidePasswordEmail"].ToString());
                        string invoiceData = "@s@|f|5|" + noticeAccountInfo.CustomerCode + "|" + noticeAccountInfo.CompanyName + "|";
                        invoiceData += mail + "|" + DateTime.Today.ToString("yyyyMMdd") + "|" + ecare_no + @"
";
                        invoiceData += "@g1@|f|3|" + noticeAccountInfo.UserId + "|" + noticeAccountInfo.Password + "|" + this.linkPortal;
                        var emailInfo = new EmailInfo()
                        {
                            TemplateMailId = ecare_no,
                            CustomerCode = noticeAccountInfo.CustomerCode,
                            CustomerName = noticeAccountInfo.CompanyName,
                            EmailTo = mail.Trim(),
                            InvoiceData = invoiceData,
                            SenderName = this.bankNameEn,
                            SenderEmail = this.sender_email,
                            EmailActiveId = emailActive.ID
                        };
                        emailInfo.CountPathFile = emailInfo.Files.Count;
                        this.emailRepository.InsertMailInvoice(emailInfo);
                    }
                    
                }
                
            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Account", ex);
                return false;
            }
            return true;
        }
        public ResultCode SendEmailResetPassword(ClientAddInfo clientAddInfo)
        {
            EMAILACTIVE curentEmailActive = GetEmailNotice(clientAddInfo.EmailActiveId);
            ReceiverAccountActiveInfo NoticeAccountInfo = this.clientRepository.GetClientAccountInfo(clientAddInfo.Id);
            if (string.IsNullOrEmpty(curentEmailActive.EMAILTO))
            {
                return ResultCode.SendMailError;
            }
            bool isSuccess = SendEmailResetPass(curentEmailActive, NoticeAccountInfo);
            //if (isSuccess)
            //{
            //    LOGINUSER loginUser = this.loginUserRepository.GetByClientId(clientAddInfo.Id);
            //    loginUser.PASSWORD = EncryptPassword(NoticeAccountInfo.Password);
            //    this.loginUserRepository.Update(loginUser);
            //}
            return isSuccess ? ResultCode.NoError : ResultCode.SendMailError;
        }
        private bool SendEmailResetPass(EMAILACTIVE emailActive, ReceiverAccountActiveInfo noticeAccountInfo)
        {
            try
            {
                var listEmail = (emailActive.EMAILTO ?? "").Split(new char[] { ',' });
                foreach (var mail in listEmail)
                {
                    int ecare_no = Int32.Parse(WebConfigurationManager.AppSettings["EmailResetPassWord"].ToString());
                    string invoiceData = "@s@|f|5|" + noticeAccountInfo.CustomerCode + "|" + noticeAccountInfo.CompanyName + "|";
                    invoiceData += mail + "|" + DateTime.Today.ToString("yyyyMMdd") + "|" + ecare_no + @"
";
                    invoiceData += "@g1@|f|3|" + noticeAccountInfo.UserId + "|" + noticeAccountInfo.Password + "|" + this.linkPortal;
                    var emailInfo = new EmailInfo()
                    {
                        TemplateMailId = ecare_no,
                        CustomerCode = noticeAccountInfo.CustomerCode,
                        CustomerName = noticeAccountInfo.CompanyName,
                        EmailTo = mail,
                        InvoiceData = invoiceData,
                        SenderName = this.bankNameEn,
                        SenderEmail = this.sender_email,
                        EmailActiveId = emailActive.ID
                    };
                    emailInfo.CountPathFile = emailInfo.Files.Count;
                    this.emailRepository.InsertMailInvoice(emailInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Account", ex);
                return false;
            }
            return true;
        }
        private bool SendEmail(EMAILACTIVE emailActive, EmailServerInfo emailServerInfo)
        {
            var emailInfo = new EmailInfo()
            {
                //Name = emailActive.TITLE,
                Subject = emailActive.TITLE,
                EmailTo = emailActive.EMAILTO,
                Content = emailActive.CONTENTEMAIL,
            };
            InsertLogoWhenSendMail(emailInfo);

            emailInfo.Files = GetFileAttach(emailActive);
            ProcessEmail processEmail = new ProcessEmail();
            return processEmail.SendEmail(emailInfo, emailServerInfo);
        }

        private bool SendEmail_2(EMAILACTIVE emailActive)
        {
            var emailInfo = new EmailInfo()
            {
                //Name = emailActive.TITLE,
                Subject = emailActive.TITLE,
                EmailTo = emailActive.EMAILTO,
                Content = emailActive.CONTENTEMAIL,
            };
            InsertLogoWhenSendMail(emailInfo);

            emailInfo.Files = GetFileAttach(emailActive);
            ProcessEmail processEmail = new ProcessEmail();
            return processEmail.SendEmail_2(emailInfo);
        }

        private ResultCode SendEmail(EmailActiveInfo emailActiveInfo, EmailServerInfo emailServerInfo, EMAILACTIVE curentEmailActive)
        {
            //bool isSuccess = SendEmail(curentEmailActive, emailServerInfo);
            //bool isSuccess = SendEmail_2(curentEmailActive);
            //if (isSuccess)
            //{

            //}

            bool isSuccess = UpdateEmailActive(curentEmailActive, emailActiveInfo, StatusSendEmail.Successfull);
            return isSuccess ? ResultCode.NoError : ResultCode.SendEmailFailed;
        }

        public ResultCode SendEmail(long id, EmailServerInfo emailServerInfo)
        {
            EMAILACTIVE curentEmailActive = GetEmailNotice(id);
            if (string.IsNullOrEmpty(curentEmailActive.EMAILTO))
            {
                return ResultCode.SendMailError;
            }
            var listMailTo = curentEmailActive.EMAILTO;
            bool isSuccess = false;
            isSuccess = SendEmail(curentEmailActive, emailServerInfo);
            curentEmailActive.EMAILTO = listMailTo;
            StatusSendEmail statusSendEmail = isSuccess ? StatusSendEmail.Successfull : StatusSendEmail.Error;
            UpdateStatusSendEmail(id, statusSendEmail);
            return isSuccess ? ResultCode.NoError : ResultCode.SendMailError;
        }

        public ResultCode SendEmail(long id, EmailActiveInfo emailActiveInfo, EmailServerInfo emailServerInfo)
        {
            if (emailActiveInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            EMAILACTIVE curentEmailActive = GetEmailActive(id, emailActiveInfo);
            if (curentEmailActive.TYPEEMAIL == (int)TypeEmailActive.DailyEmail || curentEmailActive.TYPEEMAIL == (int)TypeEmailActive.MonthlyEmail)
            {
                var listInvoiceId = this.emailRepository.GetRefIdsByEmailActiveId(curentEmailActive.ID);
                if (invoiceRepository.CheckInvoiceDeleted(listInvoiceId))
                {
                    logger.Error(userSessionInfo?.UserId, new BusinessLogicException(ResultCode.InvoiceDeleted, "Email could not be sent because this invoice has been deleted!"));
                    return ResultCode.InvoiceDeleted;
                }
            }

            Task<ResultCode> taskSendMail = null;

            switch (curentEmailActive.TYPEEMAIL)
            {
                case (int)TypeEmailActive.MonthlyEmail:
                    {
                        var listRefId = this.emailRepository.GetRefIdsByEmailActiveId(curentEmailActive.ID);
                        List<SendEmailVerificationCode> sendEmailVerificationCodes = new List<SendEmailVerificationCode>();
                        foreach (var item in listRefId)
                        {
                            InvoiceVerificationCode invoiceVerification = new InvoiceVerificationCode();
                            InvoiceReleaseInfo invoiceRelease = this.invoiceBO.GetInvoiceReleaseInfo(Convert.ToInt64(item));
                            invoiceVerification.VerificationCode = invoiceRelease.VerificationCode;
                            SendEmailVerificationCode sendEmailVerification = this.invoiceBO.GetEmailVerificationCodeInfo(invoiceVerification);
                            sendEmailVerificationCodes.Add(sendEmailVerification);
                        }
                        return SendEmailByMonth(id, sendEmailVerificationCodes);
                    }
                case (int)TypeEmailActive.DailyEmail:
                    {
                        var refId = this.emailRepository.GetRefIdByEmailActiveId(curentEmailActive.ID);
                        InvoiceVerificationCode invoiceVerification = new InvoiceVerificationCode();
                        InvoiceReleaseInfo invoiceRelease = this.invoiceBO.GetInvoiceReleaseInfo(refId);
                        invoiceVerification.VerificationCode = invoiceRelease.VerificationCode;
                        SendEmailVerificationCode sendEmailVerification = this.invoiceBO.GetEmailVerificationCodeInfo(invoiceVerification);
                        return SendEmailDaily(id, sendEmailVerification);
                    }
                case (int)TypeEmailActive.ProvideAccountEmail:
                    {
                        var refId = this.emailRepository.GetRefIdByEmailActiveId(curentEmailActive.ID);
                        ClientAddInfo clientAddInfo = new ClientAddInfo();
                        clientAddInfo.Id = refId;
                        clientAddInfo.EmailActiveId = curentEmailActive.ID;
                        return SendEmailProvideAccount(clientAddInfo, true);
                    }
                case (int)TypeEmailActive.AnnouncementEmail:
                    {
                        var refId = this.emailRepository.GetRefIdByEmailActiveId(curentEmailActive.ID);
                        AnnouncementVerificationCode announcementVerificationCode = new AnnouncementVerificationCode();
                        var releaseAnnouDetail = this.releaseAnnouncementBO.GetReleaseAnnouncementDetail(refId);
                        announcementVerificationCode.VerificationCode = releaseAnnouDetail.VERIFICATIONCODE;
                        break;
                    }
            }

            var t = Task.Run(() => SendEmail(emailActiveInfo, emailServerInfo, curentEmailActive));
            // neu gui mail trong 5 giay chua hoan thanh thi bao loi
            t.Wait(5000);
            if (t.Status == TaskStatus.Running)
            {
                return ResultCode.SendingEmail;
            }
            return t.Result;
        }

        private bool SendEmailDailyInvoice(EMAILACTIVE emailActive, SendEmailVerificationCode emailVeriCodes, bool isJob = false)
        {
            try
            {
                int ecare_no = Int32.Parse(WebConfigurationManager.AppSettings["DailyEmail"].ToString());
                string customerName = emailVeriCodes.IsOrg ? emailVeriCodes.CustomerName : emailVeriCodes.PersonContact;
                var listEmail = (emailActive.EMAILTO ?? "").Split(new char[] { ',' });
                var index = 0;
                foreach (var item in listEmail)
                {
                    string invoiceData = "@s@|f|5|" + emailVeriCodes.CustomerCode + "|" + customerName + "|";
                    invoiceData += item + "|" + DateTime.Today.ToString("yyyyMMdd") + "|" + ecare_no + @"
";
                    invoiceData += "@g1@|f|8||" + emailVeriCodes.Symbol + "|";
                    invoiceData += emailVeriCodes.InvoiceNo + "|" + emailVeriCodes.InvoiceDate.ToString("dd/MM/yyy") + "|" + this.linkPortal + "|";
                    string pass = emailVeriCodes.IsOrg ? "Mã Số Thuế của Công ty" : "Chứng minh nhân dân / Căn cước công dân của Cá nhân";
                    string passEn = emailVeriCodes.IsOrg ? "the company's tax code number" : "Identity number";
                    invoiceData += emailVeriCodes.VerificationCode + "|" + pass + "|" + passEn;
                    var emailInfo = new EmailInfo()
                    {
                        TemplateMailId = ecare_no,
                        CustomerCode = emailVeriCodes.CustomerCode,
                        CustomerName = customerName,
                        EmailTo = item,
                        InvoiceData = invoiceData,
                        SenderName = this.bankNameEn,
                        SenderEmail = this.sender_email,
                        EmailActiveId = emailActive.ID
                    };
                    emailInfo.Files = GetFileAttach(emailActive);
                    emailInfo.CountPathFile = emailInfo.Files.Count;
                    if (index > 0)
                    {
                        isJob = true;
                    }
                    emailInfo.pathFileZip = ProcessFile(emailVeriCodes.InvoiceNo, emailVeriCodes.InvoiceDate, emailVeriCodes.IsOrg, emailVeriCodes.TaxCode, emailVeriCodes.Identity, emailInfo.Files, true, emailVeriCodes.CompanyId, isJob);
                    index++;
                    emailInfo.PathFile1 = WebConfigurationManager.AppSettings["filepath1"] + Path.GetFileName(emailInfo.pathFileZip);
                    this.emailRepository.InsertMailInvoice(emailInfo);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Invoice", ex);
                return false;
            }
            return true;
        }
        private string ProcessFile(string invoiceNo, DateTime Date, bool IsOrg, string TaxCode, string Identity, List<FileInformation> Files, bool isInvoice = true, long companyId = 0, bool isJob = false)
        {
            string fileNameInvoice = "ShinhanBank_Inv_" + invoiceNo + "_" + companyId + "_" + Date.ToString("ddMMyyy");
            string fileNameMinutes = "ShinhanBank_Minutes_Inv_" + invoiceNo;
            string invoicePath = Path.Combine("Sign", PathUtil.FolderTree(Date));
            string announcementPath = Path.Combine("SignAnnouncement", PathUtil.FolderTree(Date));
            string folderTemp = Path.Combine(this.printConfig.PathInvoiceFile, companyId.ToString(), "Releases", isInvoice ? invoicePath : announcementPath, isInvoice ? fileNameInvoice : fileNameMinutes);
            string pathFileZip = folderTemp + ".zip";
            if (isJob)
            {
                return pathFileZip;
            }
            try
            {
                if (!File.Exists(pathFileZip))
                {
                    if (!Directory.Exists(folderTemp))
                    {
                        Directory.CreateDirectory(folderTemp);
                    }
                    string pathFileTo = String.Empty;
                    List<string> files = new List<string>();
                    foreach (var file in Files)
                    {
                        files.Add(file.FullPathFileattached);
                        pathFileTo = Path.Combine(folderTemp, Path.GetFileName(file.FileName));
                        File.Copy(file.FullPathFileattached, pathFileTo);
                    }
                    zipFile(IsOrg ? TaxCode : Identity, folderTemp, pathFileZip);
                    if (Directory.Exists(folderTemp))
                    {
                        Directory.Delete(folderTemp, true);
                    }
                }
                CopyFileToSFTP(pathFileZip);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return pathFileZip;
        }
        public bool JobSendMails(InvoicePrintModel printModel, string pathZip, long emailActiveId)
        {
            try
            {
                int ecare_no = Int32.Parse(WebConfigurationManager.AppSettings["DailyEmail"].ToString());
                string customerName = printModel.ISORG == "1" ? printModel.CUSTOMERNAME : printModel.PERSONCONTACT;
                string emails = printModel.USEREGISTEREMAIL == "1" ? printModel.EMAIL : printModel.RECEIVEDINVOICEEMAIL;
                //logger.Error("email of: " + printModel.ID, new Exception(emails));
                if (emails.IsNullOrEmpty() || !emails.Contains("@"))
                {
                    //logger.Error("Email not found for invoice " + printModel.INVOICENO, new Exception("JobSendMails"));
                    return false;
                }
                var listEmail = (emails ?? "").Split(new char[] { ',' });
                foreach (var item in listEmail)
                {
                    string invoiceData = "@s@|f|5|" + printModel.CUSTOMERCODE + "|" + customerName + "|";
                    invoiceData += item + "|" + DateTime.Today.ToString("yyyyMMdd") + "|" + ecare_no + @"
";//không xóa dấu xuống dòng
                    invoiceData += "@g1@|f|8||" + printModel.SYMBOL + "|";
                    invoiceData += printModel.INVOICENO + "|" + printModel.RELEASEDDATE.Value.ToString("dd/MM/yyy") + "|" + this.linkPortal + "|";
                    //string pass = emailVeriCodes.IsOrg ? "MST" : "CMND";
                    string pass = printModel.ISORG == "1" ? "Mã Số Thuế của Công ty" : "Chứng minh nhân dân / Căn cước công dân của Cá nhân";
                    string passEn = printModel.ISORG == "1" ? "the company's tax code number" : "Identity number";
                    invoiceData += printModel.VERIFICATIONCODE + "|" + pass + "|" + passEn;
                    var emailInfo = new EmailInfo()
                    {
                        TemplateMailId = ecare_no,
                        CustomerCode = printModel.CUSTOMERCODE,
                        CustomerName = customerName,
                        EmailTo = item,
                        InvoiceData = invoiceData,
                        SenderName = this.bankNameEn,
                        SenderEmail = this.sender_email,
                        EmailActiveId = emailActiveId//truyền xuống emailActiveId
                    };
                    emailInfo.CountPathFile = 2;
                    emailInfo.pathFileZip = pathZip;// ProcessFile(emailVeriCodes.InvoiceNo, emailVeriCodes.InvoiceDate, emailVeriCodes.IsOrg, emailVeriCodes.TaxCode, emailVeriCodes.Identity, emailInfo.Files, true, emailVeriCodes.CompanyId, true);
                    emailInfo.PathFile1 = WebConfigurationManager.AppSettings["filepath1"] + Path.GetFileName(emailInfo.pathFileZip);
                    this.emailRepository.InsertMailInvoice(emailInfo);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Invoice", ex);
                return false;
            }
        }
        public void SendEmailForJobShinhan(SendEmailVerificationCode emailVeriCodes)
        {
            try
            {
                int ecare_no = Int32.Parse(WebConfigurationManager.AppSettings["DailyEmail"].ToString());
                string customerName = emailVeriCodes.IsOrg ? emailVeriCodes.CustomerName : emailVeriCodes.PersonContact;
                var listEmail = (emailVeriCodes.EmailTo ?? "").Split(new char[] { ',' });
                foreach (var item in listEmail)
                {
                    string invoiceData = "@s@|f|5|" + emailVeriCodes.CustomerCode + "|" + customerName + "|";
                    invoiceData += item + "|" + DateTime.Today.ToString("yyyyMMdd") + "|" + ecare_no + @"";
                    invoiceData += "@g1@|f|8||" + emailVeriCodes.Symbol + "|";
                    invoiceData += emailVeriCodes.InvoiceNo + "|" + emailVeriCodes.InvoiceDate.ToString("dd/MM/yyy") + "|" + this.linkPortal + "|";
                    //string pass = emailVeriCodes.IsOrg ? "MST" : "CMND";
                    string pass = emailVeriCodes.IsOrg ? "Mã Số Thuế của Công ty" : "Chứng minh nhân dân / Căn cước công dân của Cá nhân";
                    string passEn = emailVeriCodes.IsOrg ? "the company's tax code number" : "Identity number";
                    invoiceData += emailVeriCodes.VerificationCode + "|" + pass + "|" + passEn;
                    var emailInfo = new EmailInfo()
                    {
                        TemplateMailId = ecare_no,
                        CustomerCode = emailVeriCodes.CustomerCode,
                        CustomerName = customerName,
                        EmailTo = item,
                        InvoiceData = invoiceData,
                        SenderName = this.bankNameEn,
                        SenderEmail = this.sender_email,
                        EmailActiveId = emailVeriCodes.InvoiceId
                    };
                    emailInfo.CountPathFile = 2;
                    emailInfo.pathFileZip = ProcessFile(emailVeriCodes.InvoiceNo, emailVeriCodes.InvoiceDate, emailVeriCodes.IsOrg, emailVeriCodes.TaxCode, emailVeriCodes.Identity, emailInfo.Files, true, emailVeriCodes.CompanyId, true);
                    emailInfo.PathFile1 = WebConfigurationManager.AppSettings["filepath1"] + Path.GetFileName(emailInfo.pathFileZip);
                    this.emailRepository.InsertMailInvoice(emailInfo);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Invoice", ex);
            }
        }
        private bool SendEmailDailyAnoument(EMAILACTIVE emailActive, SendEmailVerificationCodeAnnoun emailVeriCodes)
        {
            try
            {
                int ecare_no = Int32.Parse(WebConfigurationManager.AppSettings["Invoice_Adjustment"].ToString());
                string customerName = emailVeriCodes.IsOrg ? emailVeriCodes.CustomerName : emailVeriCodes.PersonContact;
                var listEmail = (emailVeriCodes.EmailTo ?? "").Split(new char[] { ',' });
                foreach (var item in listEmail)
                {
                    string invoiceData = "@s@|f|5|" + emailVeriCodes.CustomerCode + "|" + customerName + "|";
                    invoiceData += item + "|" + DateTime.Today.ToString("yyyyMMdd") + "|" + ecare_no + @"
";
                    //string pass = emailVeriCodes.IsOrg ? "MST" : "CMND";
                                        string pass = emailVeriCodes.IsOrg ? "Mã Số Thuế của Công ty" : "Chứng minh nhân dân / Căn cước công dân của Cá nhân";

                    string passEn = emailVeriCodes.IsOrg ? "the company's tax code number" : "Identity number";
                    invoiceData += "@g1@|f|4|" + pass + "|" + this.linkPortal + "|" + passEn + "|" + emailVeriCodes.VerificationCode + @"
";
                    invoiceData += "@g2@|r|4|" + emailVeriCodes.MinutesNo + "|" + emailVeriCodes.AnnouncementDate.ToString("dd/MM/yyy") + "|Biên bản điều chỉnh hóa đơn|" + emailVeriCodes.InvoiceNo;
                    var emailInfo = new EmailInfo()
                    {
                        TemplateMailId = ecare_no,
                        CustomerCode = emailVeriCodes.CustomerCode,
                        CustomerName = customerName,
                        EmailTo = item,
                        InvoiceData = invoiceData,
                        SenderName = this.bankNameEn,
                        SenderEmail = this.sender_email,
                        EmailActiveId = emailActive.ID
                    };
                    emailInfo.Files = GetFileAttach(emailActive);
                    emailInfo.CountPathFile = emailInfo.Files.Count;
                    emailInfo.pathFileZip = ProcessFile(emailVeriCodes.InvoiceNo, emailVeriCodes.AnnouncementDate, emailVeriCodes.IsOrg, emailVeriCodes.TaxCode, emailVeriCodes.Identity, emailInfo.Files, false, emailVeriCodes.CompanyId);
                    emailInfo.PathFile1 = WebConfigurationManager.AppSettings["filepath1"] + Path.GetFileName(emailInfo.pathFileZip);
                    this.emailRepository.InsertMailInvoice(emailInfo);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Invoice", ex);
                return false;
            }
            return true;
        }
        private bool SendMailMonthly(EMAILACTIVE emailActive, List<SendEmailVerificationCode> emailVeriCodes)
        {
            try
            {
                int ecare_no = Int32.Parse(WebConfigurationManager.AppSettings["MonthlyEmail"].ToString());
                var emailVeriCodeFirst = emailVeriCodes.FirstOrDefault();
                string customerName = emailVeriCodeFirst.IsOrg ? emailVeriCodeFirst.CustomerName : emailVeriCodeFirst.PersonContact;
                var listEmail = (emailActive.EMAILTO ?? "").Split(new char[] { ',' });
                var index = 0;
                foreach (var email in listEmail)
                {
                    string invoiceData = "@s@|f|5|" + emailVeriCodeFirst.CustomerCode + "|" + customerName + "|";
                    invoiceData += email + "|" + DateTime.Today.ToString("yyyyMMdd") + "|" + ecare_no + @"";
                    //string pass = emailVeriCodeFirst.IsOrg ? "MST" : "CMND";
                    string pass = emailVeriCodeFirst.IsOrg ? "Mã Số Thuế của Công ty" : "Chứng minh nhân dân / Căn cước công dân của Cá nhân";
                    string passEn = emailVeriCodeFirst.IsOrg ? "the company's tax code number" : "Identity number";
                    invoiceData += "@g1@|f|3|" + pass + "|" + this.linkPortal + "|" + passEn + @"@g2@|r|6|";
                    var no = 1;
                    var last = emailVeriCodes.Count;
                    foreach (var item in emailVeriCodes)
                    {
                        if (no == last)//dòng cuối cùng thì k xuống dòng
                        {
                            invoiceData += no + "|" + item.InvoiceCode + "|" + item.Symbol + "|" + item.InvoiceNo;
                            invoiceData += "|" + item.InvoiceDate.ToString("dd/MM/yyyy") + "|" + item.VerificationCode;
                        }
                        else
                        {
                            invoiceData += no + "|" + item.InvoiceCode + "|" + item.Symbol + "|" + item.InvoiceNo;
                            invoiceData += "|" + item.InvoiceDate.ToString("dd/MM/yyyy") + "|" + item.VerificationCode + @"
";
                        }
                        no++;
                    }

                    var emailInfo = new EmailInfo()
                    {
                        TemplateMailId = ecare_no,
                        CustomerCode = emailVeriCodeFirst.CustomerCode,
                        CustomerName = customerName,
                        EmailTo = email,
                        InvoiceData = invoiceData,
                        SenderName = this.bankNameEn,
                        SenderEmail = this.sender_email,
                        EmailActiveId = emailActive.ID
                    };
                    emailInfo.Files = GetFileAttach(emailActive);
                    emailInfo.CountPathFile = emailInfo.Files.Count;
                    emailInfo.pathFileZip = emailInfo.Files[0].FullPathFileattached;
                    if (index == 0)
                    {
                        CopyFileToSFTP(emailInfo.pathFileZip);
                        index++;
                    }
                    emailInfo.PathFile1 = WebConfigurationManager.AppSettings["filepath1"] + Path.GetFileName(emailInfo.pathFileZip);
                    this.emailRepository.InsertMailInvoice(emailInfo);
                }

            }
            catch (Exception ex)
            {
                logger.Error("Insert_Email_Invoice", ex);
                return false;
            }
            return true;
        }
        private bool SendEmailByMonth(EMAILACTIVE emailActive, EmailServerInfo emailServerInfo)
        {
            var emailInfo = new EmailInfo()
            {
                //Name = emailActive.TITLE,
                Subject = emailActive.TITLE,
                EmailTo = emailActive.EMAILTO,
                Content = emailActive.CONTENTEMAIL,
            };
            InsertLogoWhenSendMail(emailInfo);

            emailInfo.Files = GetFileAttach(emailActive);
            ProcessEmail processEmail = new ProcessEmail();
            return processEmail.SendEmailByMonth(emailInfo, emailServerInfo);
        }
        private void UpdateStatusSendEmail(long emailId, StatusSendEmail statusSendEmail)
        {
            var emailNotice = GetById(emailId);
            emailNotice.SENDSTATUS = (int)statusSendEmail;
            emailNotice.SENDTEDDATE = DateTime.Now;
            this.emailRepository.Update(emailNotice);
        }

        private List<FileInformation> GetFileAttach(EMAILACTIVE emailActive)
        {
            List<FileInformation> fileAttach = new List<FileInformation>();
            if (emailActive.EMAILACTIVEFILEATTACHes == null)
            {
                return fileAttach;
            }

            emailActive.EMAILACTIVEFILEATTACHes.ForEach(p =>
            {
                if (File.Exists(p.FULLPATHFILEATTACH))
                {
                    fileAttach.Add(new FileInformation(p.FILENAME, p.FULLPATHFILEATTACH));
                }
            });

            return fileAttach;
        }
        private void zipFile(string password, string folderTemp, string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (ZipFile zip = new ZipFile())
            {
                zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                zip.Password = password;
                zip.AddDirectory(folderTemp);
                zip.Save(fileName);
            }
        }
        private void CopyFileToFTP(List<string> PathFileSources)
        {
            try
            {
                string host = WebConfigurationManager.AppSettings["HostFtp"];
                string userName = WebConfigurationManager.AppSettings["UserNameFtp"];
                string passWord = WebConfigurationManager.AppSettings["PasswordFtp"];
                Parallel.ForEach(PathFileSources, file =>
                {
                    string fileName = Path.GetFileName(file);
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Path.Combine(host, fileName));
                    request.Credentials = new NetworkCredential(userName, passWord);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    using (Stream fileStream = File.OpenRead(file))
                    using (Stream ftpStream = request.GetRequestStream())
                    {
                        fileStream.CopyTo(ftpStream);
                    }
                });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void CopyFileToSFTP(string PathFileSource)
        {
            try
            {
                string host = WebConfigurationManager.AppSettings["Host"];
                string userName = WebConfigurationManager.AppSettings["UserName"];
                string passWord = WebConfigurationManager.AppSettings["Password"];
                string directory = WebConfigurationManager.AppSettings["Path"];
                int port = Int32.Parse(WebConfigurationManager.AppSettings["Port"].ToString());
                using (SftpClient client = new SftpClient(host, port, userName, passWord))
                {
                    client.Connect();
                    client.ChangeDirectory(directory);
                    using (FileStream fs = new FileStream(PathFileSource, FileMode.Open))
                    {
                        client.BufferSize = 4 * 1024;
                        client.UploadFile(fs, Path.GetFileName(PathFileSource));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void InsertLogoWhenSendMail(EmailInfo emailInfo)
        {
            if (emailInfo.Content.Contains(EmailType.PlaceHolderLogo))
            {
                emailInfo.Content = emailInfo.Content.Replace(EmailType.PlaceHolderLogo, $"data:image/jpeg;base64,{CommonUtil.GetBase64StringImage(DefaultFields.LOGO_MAIL_FILE)}");
            }
        }

        private void InsertLogoForClientView(EmailActiveInfo emailActiveInfo)
        {
            if (emailActiveInfo.ContentEmail.Contains(EmailType.PlaceHolderLogo))
            {
                emailActiveInfo.ContentEmail = emailActiveInfo.ContentEmail.Replace(EmailType.PlaceHolderLogo, $"data:image/jpeg;base64,{CommonUtil.GetBase64StringImage(DefaultFields.LOGO_MAIL_FILE)}");
            }
        }

        public long? checkSentEmail(long refId)
        {
            return this.emailRepository.checkEmailExists(refId);
        }
    }
}