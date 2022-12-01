using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class StatusReleaseAnnouncement
    {
        [JsonProperty("announcementsRelease")]
        public List<AnnouncementsRelease> AnnouncementsRelease { get; set; }
    }
}
