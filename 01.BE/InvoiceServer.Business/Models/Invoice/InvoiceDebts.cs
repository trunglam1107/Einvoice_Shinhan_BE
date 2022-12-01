using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceDebts
    {
        public DateTime? DateOfInvoice { get; private set; }

        public decimal Summary { get; set; }

        public long? ClientId { get; set; }
        public InvoiceDebts()
        {

        }
    }
}
