namespace InvoiceServer.Business.Models
{
    public class EmailWebConfig
    {
        public string TemplateEmailFolder { get; set; }

        public int RetryNumberSendEmail { get; set; }

        public EmailConfig SenderConfig { get; set; }
    }
}
