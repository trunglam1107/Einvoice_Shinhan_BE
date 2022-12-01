using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReportCombineExport
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        [JsonProperty("items")]

        public List<ReportInvoiceDetail> Items { get; set; }
        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("time")]
        public int Time { get; set; }


        [JsonProperty("timeNo")]
        public int TimeNo { get; set; }

        [JsonProperty("periods")]
        public string periods { get; set; }

        public int IsMonth { get; set; }

        public decimal? SBTHDulieu { get; set; }
        public ReportCombineExport(CompanyInfo company)
        {
            this.Company = company;
        }

    }
}
