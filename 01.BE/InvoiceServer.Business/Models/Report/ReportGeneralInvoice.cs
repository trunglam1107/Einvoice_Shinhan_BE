using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReportGeneralInvoice
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        [JsonProperty("items")]
        public List<ReportInvoiceDetail> items { get; set; }

        [JsonProperty("precious")]
        public int Precious { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("branch")]
        public MyCompanyInfo Branch { get; set; }

        [JsonIgnore]
        public DateTime DateFrom { get; set; }

        [JsonIgnore]
        public DateTime DateTo { get; set; }

        public bool IsViewByMonth { get; set; }

        public long TotalRecords { get; set; }

        [JsonProperty("BSLThu")]
        public decimal? BSLThu { get; set; }

        public ReportGeneralInvoice(CompanyInfo company)
        {
            this.Company = company;
        }
    }
}
