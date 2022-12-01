using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class AnnouncementsRelease
    {
        [JsonProperty("announcementId")]
        public long AnnouncementId { get; set; }

        [JsonProperty("signed")]
        public bool Signed { get; set; }

        [JsonProperty("invoiceId")]
        public long InvoiceId { get; set; }

        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }

        [JsonProperty("companySignDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime CompanySignDate { get; set; }
        public bool? ClientSign { get; set; }
        public long CompanyId { get; set; }

        public AnnouncementsRelease()
        {
        }

    }
}
