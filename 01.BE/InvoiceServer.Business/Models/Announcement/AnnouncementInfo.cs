using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class AnnouncementInfo
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("denominator")]
        [DataConvert("Denominator")]
        public string Denominator { get; set; }

        [StringTrimAttribute]
        [JsonProperty("announcementType")]
        [DataConvert("AnnouncementType")]
        public int AnnouncementType { get; set; }

        [StringTrimAttribute]
        [JsonProperty("announcementStatus")]
        [DataConvert("AnnouncementStatus")]
        public int AnnouncementStatus { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companySign")]
        [DataConvert("CompanySign")]
        public bool? CompanySign { get; set; }

        [StringTrimAttribute]
        [JsonProperty("clientSign")]
        [DataConvert("ClientSign")]
        public bool? ClientSign { get; set; }

        [StringTrimAttribute]
        [JsonProperty("used")]
        public bool? Used { get; set; }
    }
}
