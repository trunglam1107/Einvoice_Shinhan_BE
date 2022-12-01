using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceCheckDaily
    {
        public long? CompanyId { get; set; }

        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("branchName")]
        public string CompanyName { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        [JsonProperty("minInvoiceNo")]
        public decimal? MinInvoiceNo { get; set; }

        [JsonProperty("maxInvoiceNo")]
        public decimal? MaxInvoiceNo { get; set; }

        [JsonProperty("quantity")]
        public decimal? Quantity { get; set; }

        [JsonProperty("invNoResult")]
        public string InvNoResult { get; set; }

        [JsonProperty("invInfoResult")]
        public string InvInfoResult { get; set; }
    }
}
