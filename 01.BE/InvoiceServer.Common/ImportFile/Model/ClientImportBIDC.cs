using System;
using System.ComponentModel.DataAnnotations;

namespace InvoiceServer.Common.Models
{
    public class ClientImportBIDC
    {
        public int IsOrg { get; set; }  // bool

        [StringLength(50)]
        public string TaxCode { get; set; }

        [StringLength(50)]
        public string CustomerCode { get; set; }

        [StringLength(255)]
        public string CustomerName { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        public int SendInvoiceByMonth { get; set; }  // bool

        public DateTime DateSendInvoice { get; set; }

        [StringLength(255)]
        public string Mobile { get; set; }

        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Delegate { get; set; }

        [StringLength(255)]
        public string PersonalContact { get; set; }

        public int IsCreateAccount { get; set; }
    }
}
