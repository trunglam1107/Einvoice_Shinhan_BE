using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReleaseInvoiceInfo
    {
        [JsonProperty("id")]
        public long InvoiceId { get; set; }

        [JsonProperty("no")]
        public string InvoiceNo { get; set; }
        public string CompanyName { get; set; }
        public string SerialNumber { get; set; }
        public long InvoiceDetailId { get; set; }
        public bool IsImportedByJob { get; set; }
        public string Description { get; set; }
        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }
        public DateTime? DateRelease { get; set; }
        public DateTime CancellingDate { get; set; }

        public ReleaseInvoiceInfo()
        {

        }
        public ReleaseInvoiceInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReleaseInvoiceInfo>(srcObject, this);
            }
        }
    }
}
