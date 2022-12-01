using InvoiceServer.Common.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvoiceServer.Common.Models
{
    public class InvoiceImportBIDC
    {
        public int IndexImport { get; set; }
        public long Id { get; set; }
        [StringLength(20)]
        public string BranchId { get; set; }

        [StringLength(7)]
        public string InvoiceNo { get; set; }

        public DateTime InvoiceDate { get; set; }

        [StringLength(50)]
        public string CustomerCode { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(3)]
        public string CurrencyCode { get; set; }

        public decimal ExchangeRate { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public decimal Quantity { get; set; }

        public decimal Amount { get; set; }

        public string VatRate { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal TotalAmount { get; set; }

        [StringLength(20)]
        public string BankRef { get; set; }

        [StringLength(50)]
        public string BankAccount { get; set; }

        public long TaxId { get; set; }

        public string TaxCode { get; set; }

        public string CustomerName { get; set; }

        public string Address { get; set; }

        public bool CurrentTransactions
        {
            // Giao dịch vãng lai
            get
            {
                return string.IsNullOrEmpty(CustomerCode) || CustomerCode == BIDCDefaultFields.CURRENT_CLIENT;
            }
        }
    }
}
