namespace InvoiceServer.Business.Models.Portal
{
    public class PTClientAccountInfo
    {
        public PTClientAccountInfo()
        {

        }

        public long ID { get; set; }
        public string Password { get; set; }
        public long? ClientID { get; set; }
        public string Email { get; set; }
        public bool? UseRegisterEmail { get; set; }
        public string ReceivedInvoiceEmail { get; set; }

        public long? CompanyId { get; set; }

    }
}

