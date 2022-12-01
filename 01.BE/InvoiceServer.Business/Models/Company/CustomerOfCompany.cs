namespace InvoiceServer.Business.Models
{
    public class CustomerOfCompany
    {
        public long CompanySID { get; set; }

        public long? CustomerIdOfCompany { get; set; }
        public string CompanyName { get; set; }

        public string CustomerName { get; set; }

        public string TaxCode { get; set; }

        public string Email { get; set; }

        public string PersonContact { get; set; }

        public string Tel { get; set; }

        public string LevelCustomer { get; set; }

        public CustomerOfCompany()
        {

        }
    }
}
