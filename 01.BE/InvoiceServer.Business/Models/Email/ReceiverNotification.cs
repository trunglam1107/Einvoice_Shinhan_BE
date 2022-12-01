using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReceiverNotification
    {
        public string Subject { get; set; }
        public string Content { get; set; }
        public List<string> Emails { get; set; }

        public ReceiverNotification(string subject, string content, List<string> emails)
        {
            this.Content = content;
            this.Subject = subject;
            this.Emails = emails;
        }
    }
}
