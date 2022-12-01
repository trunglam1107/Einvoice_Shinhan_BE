using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReleaseInvoices
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

        [JsonProperty("dateInvoice")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? InvoiceDate { get; set; }

        [JsonProperty("note")]
        public string InvoiceNote { get; set; }

        [JsonProperty("releaseStatus")]
        public string Status { get; set; }

        [JsonProperty("releaseId")]
        public long ReleaseId { get; set; }

        public ReleaseInvoices()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public ReleaseInvoices(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReleaseInvoices>(srcObject, this);
            }
        }
    }
}
