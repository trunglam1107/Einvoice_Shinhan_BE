using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ApprovedAnnouncement
    {
        [JsonIgnore()]
        public long UserAction { get; set; }

        [JsonIgnore()]
        public long CompanyId { get; set; }

        [JsonIgnore()]
        public long? UserActionId { get; set; }

        [JsonProperty("items")]
        public List<ReleaseAnnouncementInfo> ReleaseAnnouncementInfos { get; set; }

        public ApprovedAnnouncement()
        {

        }

    }
}
