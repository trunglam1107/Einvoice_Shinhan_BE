using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoicesReleaseDetail
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("code")]
        public string InvoiceCode { get; set; }

        [JsonProperty("symbol")]
        public string InvoiceSymbol { get; set; }

        [JsonProperty("no")]
        public string InvoiceNo { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("dateInvoice")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? InvoiceDate { get; set; }
        public long CompanyId { get; set; }
        public InvoicesReleaseDetail()
        {
        }

    }
}
