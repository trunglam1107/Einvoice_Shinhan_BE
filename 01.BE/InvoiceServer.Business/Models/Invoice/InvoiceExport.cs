using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class InvoiceExport
    {
        [JsonIgnore]
        public CompanyInfo company { get; set; }

        [JsonProperty("items")]

        public List<InvoiceMaster> Items { get; set; }

        [JsonProperty("replaced")]
        public List<ReplacedInvoice> Replaced { get; set; }

        public InvoiceExport(CompanyInfo company)
        {
            this.company = company;
        }
    }
}
