using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReportInvoiceConverted
    {
        [JsonProperty("invoiceCode")]
        public string InvoiceCode { get; set; }

        [JsonProperty("invoiceSymbol")]
        public string InvoiceSymbol { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("customerCode")]
        public string CustomerCode { get; set; }

        [JsonProperty("personContact")]
        public string PersonContact { get; set; }

        [JsonProperty("invoiceNo")]
        public decimal InvoiceNo { get; set; }

        [JsonProperty("no")]
        public string No { get; set; }

        [JsonProperty("invoiceDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? InvoiceDate { get; set; }

        public ReportInvoiceConverted()
        {
        }

    }
}
