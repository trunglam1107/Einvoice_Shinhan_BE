using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReleaseListInvoice
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        [JsonProperty("userRelease")]
        public string UserRelease { get; set; }

        [JsonProperty("status")]
        public int? Status { get; set; }

        public ReleaseListInvoice()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public ReleaseListInvoice(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReleaseListInvoice>(srcObject, this);
            }
        }
    }
}
