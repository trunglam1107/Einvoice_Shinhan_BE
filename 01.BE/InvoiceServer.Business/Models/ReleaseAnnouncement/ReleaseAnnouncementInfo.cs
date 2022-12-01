using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ReleaseAnnouncementInfo
    {
        [JsonProperty("id")]
        public long AnnouncementId { get; set; }

        [JsonProperty("invoiceId")]
        public long InvoiceId { get; set; }
        [JsonProperty("companyId")]
        public long CompanyId { get; set; }

        [JsonIgnore]
        public string CompanyName { get; set; }

        [JsonIgnore]
        public string SerialNumber { get; set; }

        public ReleaseAnnouncementInfo()
        {

        }
        public ReleaseAnnouncementInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReleaseAnnouncementInfo>(srcObject, this);
            }
        }
    }
}
