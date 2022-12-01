using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class InvoiceDetail
    {
        public decimal Id { get; set; }
        public decimal InvoiceId { get; set; }
        public string ProductName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal TaxId { get; set; }
        public string DisplayInvoice { get; set; }
        public string TaxName { get; set; }
        public string TaxCode { get; set; }
        public decimal? Total { get; set; }
        public decimal? AmountTax { get; set; }
        public decimal? Sum { get; set; }
    }
}