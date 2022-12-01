using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceClientSign
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; } // Ký Hiệu 

        [JsonProperty("no")]
        public string No { get; set; }//Số [No]

        [JsonProperty("invoiceDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? Created { get; set; } // Ngày hóa đơn

        [JsonProperty("locationSignLeft")]
        public double LocationSignLeft { get; set; }

        [JsonProperty("locationSignButton")]
        public double LocationSignButton { get; set; }

        [JsonProperty("clientSign")]
        public bool ClientSign { get; set; } // Ký Hiệu 

        [JsonProperty("dateReleaseClient")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime DateReleaseClient { get; set; }
        public InvoiceClientSign()
        {
        }
    }
}
