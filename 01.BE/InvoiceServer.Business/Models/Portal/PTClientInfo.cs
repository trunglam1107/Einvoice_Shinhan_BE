using System;

namespace InvoiceServer.Business.Models.Portal
{
    public class PTClientInfo
    {
        public PTClientInfo()
        {

        }

        public long ID { get; set; }
        public Nullable<long> CompanyID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerCode { get; set; }
        public string TaxCode { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public Nullable<int> TypeSendInvoice { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Delegate { get; set; }
        public string PersonContact { get; set; }
        public string BankAccount { get; set; }
        public string AccountHolder { get; set; }
        public string BankName { get; set; }
        public string Description { get; set; }
        public Nullable<int> CustomerType { get; set; }
        public string Password { get; set; }
        public long? ClientID { get; set; }

    }
}

