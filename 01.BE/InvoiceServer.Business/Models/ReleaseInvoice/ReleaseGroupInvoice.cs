using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReleaseGroupInvoice
    {
        [StringTrimAttribute]
        [JsonProperty("invoiceTemplateId")]
        public long? invoiceTemplateId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceSymbolId")]
        public string Symbol { get; set; }

        [JsonProperty("dateFrom")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? DateFrom { get; set; }

        [JsonProperty("dateTo")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? DateTo { get; set; }

        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        [JsonIgnore]
        public long CompanyId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonIgnore]
        public long UserActionId { get; set; }

        public ReleaseGroupInvoice()
        {

        }
    }
}
