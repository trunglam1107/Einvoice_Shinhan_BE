namespace InvoiceServer.Business.Models
{
    public class SendEmailRelease
    {
        public string InvoiceNo { get; set; }
        public long? CompanyId { get; set; }
        public long InvoiceId { get; set; }
        public long ParentId { get; set; }
        public decimal? TotalInvoice { get; set; }
        public string CustomerName { get; set; }

        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public long? ReleaseId { get; set; }
        public long ReleaseDetailId { get; set; }

        public int InvoiceSample { get; set; }

        public string InvoicePattern { get; set; }

        public string InvoiceSeria { get; set; }

        public string InvoiceNoOld { get; set; }

        public string InvoicePatternOld { get; set; }

        public string InvoiceSeriaOld { get; set; }

    }
}
