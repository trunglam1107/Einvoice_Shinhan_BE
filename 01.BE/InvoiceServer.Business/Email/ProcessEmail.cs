using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;

namespace InvoiceServer.Business.Email
{
    public class ProcessEmail : IProcessEmail
    {
        public bool SendEmail(EmailInfo emailInfo, EmailServerInfo emailServerInfo)
        {
            var typeMail = Activator.CreateInstance(GetTypeSendMail(), emailInfo, emailServerInfo) as IEmail;
            return typeMail.Send();
        }

        public bool SendEmail_2(EmailInfo emailInfo)
        {
            var typeMail = Activator.CreateInstance(GetTypeSendMail(), emailInfo) as IEmail;
            return typeMail.Send();
        }

        public bool SendEmail(IEmail email)
        {
            return email.Send();
        }

        public bool SendEmailByMonth(EmailInfo emailInfo, EmailServerInfo emailServerInfo)
        {
            var typeMail = Activator.CreateInstance(GetTypeSendMail(), emailInfo, emailServerInfo) as IEmail;
            return typeMail.SendByMonth();
        }

        public bool SendEmailByMonth(IEmail email)
        {
            return email.SendByMonth();
        }

        public bool Test(EmailInfo emailInfo, EmailServerInfo emailServerInfo)
        {
            var typeMail = Activator.CreateInstance(GetTypeSendMail(), emailInfo, emailServerInfo) as IEmail;
            return typeMail.Test();
        }

        private Type GetTypeSendMail()
        {
            int sendMailTypeConfig = CommonUtil.GetConfig("SendMailType", 0);
            switch (sendMailTypeConfig)
            {
                case 0: return typeof(SendGmail);
                case 1: return typeof(SendGmailMailKit);
                default: return typeof(SendGmail);
            }
        }
    }
}
