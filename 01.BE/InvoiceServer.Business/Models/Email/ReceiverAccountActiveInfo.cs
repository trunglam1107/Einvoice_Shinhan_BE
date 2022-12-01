namespace InvoiceServer.Business.Models
{
    public class ReceiverAccountActiveInfo
    {
        public string UserName { get; set; }

        public string UserId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string CompanyName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }

        public bool IsOrg { get; set; }
        public long CustomerId { get; set; }

        public ReceiverAccountActiveInfo()
        {
        }
    }
}
