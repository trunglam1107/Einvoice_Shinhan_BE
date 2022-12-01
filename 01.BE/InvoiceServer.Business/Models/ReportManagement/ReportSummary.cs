using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReportSummary
    {

        [JsonProperty("contractDate")] //2
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ContractDate { get; set; }

        [JsonProperty("taxCode")] //2
        public string TaxCode { get; set; }

        [JsonProperty("customerName")] //2
        public string CustomerName { get; set; }

        [JsonProperty("companyName")] //3
        public string CompanyName { get; set; }

        [JsonProperty("customerId")] //4
        public string CustomerId { get; set; }

        [JsonProperty("contracNo")] //5
        public string ContracNo { get; set; }

        [JsonProperty("numberInvoice")] //6
        public decimal? NumberInvoiceUse { get; set; }

        [JsonProperty("status")] //6
        public int Status { get; set; }

        public ReportSummary()
        {
        }
    }
}
