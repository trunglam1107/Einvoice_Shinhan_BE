using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoicesRelease
    {
        [JsonProperty("invoiceId")]
        public long InvoiceId { get; set; }

        [JsonProperty("signed")]
        public bool Signed { get; set; }

        [JsonProperty("invoiceNo")]
        public string InvoiceNo { get; set; }

        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }

        [JsonProperty("printed")]
        public bool Printed { get; set; }

        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime ReleasedDate { get; set; }

        [JsonProperty("locationSignLeft")]
        public double LocationSignLeft { get; set; }

        [JsonProperty("locationSignRight")]
        public double LocationSignRight { get; set; }

        [JsonProperty("locationSignButton")]
        public double LocationSignButton { get; set; }


        public string CompanyName { get; set; }
        public string SerialNumber { get; set; }


        public InvoicesRelease()
        {
        }

    }
}
