using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReporDebts
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        [JsonProperty("items")]
        public List<ReportListDebts> items { get; set; }
        public ReporDebts(CompanyInfo company)
        {
            this.Company = company;
        }
    }
}
