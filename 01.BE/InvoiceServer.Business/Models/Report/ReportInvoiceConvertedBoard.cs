using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReportInvoiceConvertedBoard
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        [JsonProperty("items")]
        public List<ReportInvoiceConverted> items { get; set; }

        public ReportInvoiceConvertedBoard()
        {
        }

    }
}
