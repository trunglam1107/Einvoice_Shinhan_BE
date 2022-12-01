using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class EmailInfo
    {
        public long EmailActiveId { get; set; }
        public List<long> RefIds { get; set; }
        public long? CompanyId { get; set; }
        public long InvoiceId { get; set; }
        public string Name { get; set; }
        public string EmailTo { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public List<string> EmailBccs { get; set; }
        public DateTime DateSend { get; set; }
        public string EmailType { get; set; }
        public long TemplateMailId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string InvoiceData { get; set; }
        public int CountPathFile { get; set; }
        public string pathFileZip { get; set; }
        public string PathFile1 { get; set; }
        public List<FileInformation> Files { get; set; }

        public EmailInfo()
        {
            this.Files = new List<FileInformation>();
            this.EmailBccs = new List<string>();
            this.RefIds = new List<long>();
        }

        public EmailInfo(string name, string emailto, string subject, string content, List<string> emailBccs)
            : this()
        {
            this.Name = name;
            this.EmailTo = emailto;
            this.Subject = subject;
            this.Content = content;
            this.EmailBccs = emailBccs;
        }
    }
}
