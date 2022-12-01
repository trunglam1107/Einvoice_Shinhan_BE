using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.IO;
using System.Net.Mail;
using System.Web.Configuration;

namespace InvoiceServer.Business
{
    public class SendGmail : IEmail
    {
        private static readonly Logger logger = new Logger();
        private readonly EmailInfo email;
        private readonly SmtpClient smtpClient;
        private readonly EmailServerInfo emailServerInfo;
        public SendGmail(EmailInfo email, EmailServerInfo emailServerInfo)
        {
            this.email = email;
            if (emailServerInfo == null)
            {
                emailServerInfo = GetMailServeConfig();
            }
            this.emailServerInfo = emailServerInfo;
            SmtpClient smtp = new SmtpClient();
            smtp.EnableSsl = this.emailServerInfo.MethodSendSSL;
            smtp.Port = this.emailServerInfo.Port;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new System.Net.NetworkCredential(this.emailServerInfo.UserName, this.emailServerInfo.Password);
            smtp.Host = this.emailServerInfo.SMTPServer;
            this.smtpClient = smtp;
        }
        public bool Send()
        {
            return SendEmail();
        }
        public bool SendByMonth()
        {
            return SendEmailByMonth();
        }

        private EmailServerInfo GetMailServeConfig()
        {
            EmailServerInfo infoOfEmailServer = new EmailServerInfo();
            infoOfEmailServer.EmailServer = WebConfigurationManager.AppSettings["emailServer"];
            infoOfEmailServer.SMTPServer = WebConfigurationManager.AppSettings["smtpServer"];
            infoOfEmailServer.Port = Int32.Parse(WebConfigurationManager.AppSettings["port"].ToString());
            infoOfEmailServer.UserName = WebConfigurationManager.AppSettings["userName"];
            infoOfEmailServer.Password = WebConfigurationManager.AppSettings["password"];
            infoOfEmailServer.MethodSendSSL = bool.Parse(WebConfigurationManager.AppSettings["ssl"].ToString());
            return infoOfEmailServer;
        }

        private bool SendEmailByMonth()
        {
            bool resultSendEmail = true;

            try
            {
                if (email.Files.Count > 0)
                {
                    using (Attachment attachment = new Attachment(email.Files[0].FullPathFileattached))
                    {
                        MailMessage mailMessage = new MailMessage();
                        mailMessage.From = new MailAddress(this.emailServerInfo.EmailServer);
                        string[] emailsTo = email.EmailTo.Split(',');
                        if (emailsTo.Length == 0)
                        {
                            return false;
                        }

                        foreach (var emailTo in emailsTo)
                        {
                            mailMessage.To.Add(new MailAddress(emailTo, email.Name));
                        }
                        mailMessage.Subject = email.Subject;
                        mailMessage.Body = email.Content;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Attachments.Add(attachment);
                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                resultSendEmail = false;
                logger.Error(ex, "Send email error", null);
            }

            return resultSendEmail;
        }

        private bool SendEmail()
        {
            bool resultSendEmail = true;

            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(this.emailServerInfo.EmailServer);
                string[] emailsTo = email.EmailTo.Split(',');
                if (emailsTo.Length == 0)
                {
                    return false;
                }

                foreach (var emailTo in emailsTo)
                {
                    mailMessage.To.Add(new MailAddress(emailTo, email.Name));
                }
                mailMessage.Subject = email.Subject;
                mailMessage.Body = email.Content;
                mailMessage.IsBodyHtml = true;
                if (email.Files.Count > 0)
                {
                    email.Files.ForEach(p =>
                    {
                        if (p.FileName != null)
                        {
                            byte[] file = File.ReadAllBytes(p.FullPathFileattached);
                            MemoryStream str = new MemoryStream(file);
                            mailMessage.Attachments.Add(new Attachment(str, p.FileName));
                        }
                        else
                        {
                            mailMessage.Attachments.Add(new Attachment(p.FullPathFileattached));
                        }
                    });
                }
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                resultSendEmail = false;
                logger.Error(ex, "Send email error", null);
            }

            return resultSendEmail;
        }

        public bool Test()
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(this.emailServerInfo.EmailServer);
            string[] emailsTo = email.EmailTo.Split(',');
            foreach (var emailTo in emailsTo)
            {
                mailMessage.To.Add(new MailAddress(emailTo, email.Name));
            }
            mailMessage.Subject = email.Subject;
            mailMessage.Body = email.Content;
            mailMessage.IsBodyHtml = true;
            smtpClient.Send(mailMessage);
            return true;
        }
    }
}
