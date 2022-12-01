namespace InvoiceServer.Business.Models
{
    public class JobApprovedInvoice
    {
        public long CompanyId { get; set; }
        public long InvoiceId { get; set; }
        public decimal InvoiceNo { get; set; }
        public JobApprovedInvoice() { }
    }
}
