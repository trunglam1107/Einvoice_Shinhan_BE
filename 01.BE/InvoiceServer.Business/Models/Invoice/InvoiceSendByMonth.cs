namespace InvoiceServer.Business.Models
{
    public class InvoiceSendByMonth
    {
        public string InvoiceTemplate { get; set; }
        public string InvoiceSymbol { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string VerificationCode { get; set; }
        public string NumberFormat { get; set; }
        public long InvoiceId { get; set; }
    }
}
