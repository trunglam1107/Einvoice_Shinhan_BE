namespace InvoiceServer.Business.Models.Portal
{
    public class NotificationUsedInvoiceDetail
    {
        public NotificationUsedInvoiceDetail()
        {

        }

        public long? CompanyId { get; set; }

        public string CompanyName { get; set; }

        public long RegisterTemplatesId { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public string NotificationUseCode { get; set; }

        public decimal? NumberRegister { get; set; }
        public decimal? NumberUse { get; set; }
        public decimal? NumberCancelling { get; set; }

    }
}

