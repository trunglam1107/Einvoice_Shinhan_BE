using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class SignDetailInfo
    {
        [JsonProperty("invoiceId")]
        public long InvoiceId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isClientSign")]
        public bool IsClientSign { get; set; }

        [JsonProperty("createdDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("announcementId")]
        public long AnnouncementId { get; set; }

        [JsonProperty("typeSign")]
        public int TypeSign { get; set; }

        public SignDetailInfo(object srcObject)
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, SignDetailInfo>(srcObject, this);
            }
        }

        public SignDetailInfo()
        {
        }
    }

}
