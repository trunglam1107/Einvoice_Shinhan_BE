using System;

namespace InvoiceServer.Business.Models.Portal
{
    public class PTInvoice
    {
        public PTInvoice()
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
        public Nullable<bool> Deleted { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> Updated { get; set; }
        public Nullable<long> UpdateBy { get; set; }
        public string InvoiceCode { get; set; }
        public string Symbol { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceSample_Name { get; set; }
        public string InvoiceType_Name { get; set; }
        public long? ClientID { get; set; }

        public int? InvoiceStatus { get; set; }

        public bool ClientSign { get; set; }

        public string VerificationCode { get; set; }
        public Nullable<System.DateTime> ReleasedDate { get; set; }

        public Nullable<decimal> Sum { get; set; }
        public Nullable<decimal> CurrencyExchangeRate { get; set; }

        public string TaxCodeBuyer { get; set; }
        public string UserId { get; set; }

        public Nullable<decimal> InvoiceNo2 { get; set; }

        public string CurrencyCode { get; set; }

        public Nullable<int> InvoiceType { get; set; }

    }
}

