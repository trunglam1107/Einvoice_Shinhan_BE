namespace InvoiceServer.Business.Models
{
    public class ServerSignResult
    {
        public int? InvoiceSuccess { get; set; }
        public int? InvoiceFail { get; set; }
        public string Message { get; set; }
        public string MessageError { get; set; }

        public int? StatusTwan { get; set; }
    }
}
