using InvoiceServer.Business.Email;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using ContentType = System.Net.Mime.ContentType;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace InvoiceServer.Business
{
    public class SendGmailMailKit : IEmail
    {
        private static readonly Logger logger = new Logger();
        private readonly EmailInfo email;
        private SmtpClient client;
        private readonly EmailServerInfo emailServerInfo;

        public SendGmailMailKit(EmailInfo email, EmailServerInfo _emailServerInfo)
        {
            this.email = email;
            if (_emailServerInfo == null)
            {
                _emailServerInfo = GetMailServeConfig();
            }
            this.emailServerInfo = _emailServerInfo;
            OpenClient();
        }

        ~SendGmailMailKit()
        {
            DisconnectClient();
        }

        private void DisconnectClient()
        {
            try
            {
                client.Disconnect(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DisconnectClient error", new object[] { client });
            }
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

        private bool OpenClient()
        {
            try
            {
                client = new SmtpClient();
                client.ServerCertificateValidationCallback = (object sender,
                            X509Certificate certificate,
                            X509Chain chain,
                            SslPolicyErrors sslPolicyErrors) => true;
                client.Connect(emailServerInfo.SMTPServer, emailServerInfo.Port, (SecureSocketOptions)emailServerInfo.SecureSocketOptions);
                client.Authenticate(emailServerInfo.UserName, emailServerInfo.Password);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "OpenClient error", new object[] { emailServerInfo });
                return false;
            }
        }

        private Stream Base64ToImageStream(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            return ms;
        }

        private AlternateView ContentToAlternateView(string content)
        {
            var imgCount = 0;
            List<LinkedResource> resourceCollection = new List<LinkedResource>();
            foreach (Match m in Regex.Matches(content, "<img.+?(src=[\"'].+?[\"']).*?>", RegexOptions.IgnoreCase))
            {
                imgCount++;
                var srcImgContent = m.Groups[1].Value;
                var quoteLevel1 = srcImgContent.Substring(srcImgContent.Length - 1);

                string type = Regex.Match(srcImgContent, ":(?<type>.*?);base64,").Groups["type"].Value;
                string base64 = Regex.Match(srcImgContent, "base64,(?<base64>.*?)[\"']").Groups["base64"].Value;
                if (String.IsNullOrEmpty(type) || String.IsNullOrEmpty(base64))
                {
                    //ignore replacement when match normal <img> tag
                    continue;
                }
                var replacement = $" src={quoteLevel1}cid:{imgCount}{quoteLevel1}";
                content = content.Replace(srcImgContent, replacement);
                var tempResource = new LinkedResource(Base64ToImageStream(base64), new ContentType(type))
                {
                    ContentId = imgCount.ToString()
                };
                resourceCollection.Add(tempResource);
            }
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(content, null, MediaTypeNames.Text.Html);
            foreach (var item in resourceCollection)
            {
                alternateView.LinkedResources.Add(item);
            }

            return alternateView;
        }

        private MailMessage ParseMimeSendMail()
        {
            MailMessage message = new MailMessage();
            try
            {
                message.From = new MailAddress(emailServerInfo.EmailServer);
                string[] emailsTo = email.EmailTo.Split(',');
                if (emailsTo.Length == 0)
                {
                    return null;
                }
                foreach (var emailTo in emailsTo)
                {
                    message.To.Add(new MailAddress(emailTo, email.Name));
                }
                message.SubjectEncoding = Encoding.UTF8;
                message.Subject = email.Subject;
                message.IsBodyHtml = true;
                AlternateView alterView = ContentToAlternateView(email.Content);
                message.AlternateViews.Add(alterView);

                if (email.Files.Count > 0)
                {
                    email.Files.ForEach(p =>
                    {
                        if (p.FileName != null)
                        {
                            byte[] file = File.ReadAllBytes(p.FullPathFileattached);
                            MemoryStream str = new MemoryStream(file);
                            message.Attachments.Add(new Attachment(str, p.FileName));
                        }
                        else
                        {
                            message.Attachments.Add(new Attachment(p.FullPathFileattached));
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "ParseMimeSendMail error", new object[] { emailServerInfo, email });
            }
            return message;
        }

        public bool SendEmail()
        {
            bool result = false;
            MimeMessage message = null;
            try
            {
                if (email.EmailTo.Split(',').Length == 0)
                {
                    return false;
                }
                message = MimeMessage.CreateFromMailMessage(ParseMimeSendMail());
                client.Send(message);
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SendEmail error", new object[] { client, message });
            }
            return result;
        }

        public bool Send()
        {
            return SendEmail();
        }

        public bool SendByMonth()
        {
            return SendEmail();
        }

        public bool Test()
        {
            MimeMessage message = null;
            message = MimeMessage.CreateFromMailMessage(ParseMimeSendMail());
            client.Send(message);
            return true;
        }
    }
}
