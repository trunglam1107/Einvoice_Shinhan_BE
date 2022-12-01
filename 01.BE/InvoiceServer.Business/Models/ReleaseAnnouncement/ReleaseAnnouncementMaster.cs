using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReleaseAnnouncementMaster
    {
        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime ReleasedDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonIgnore()]
        public long UserAction { get; set; }

        [JsonIgnore()]
        public long? CompanyId { get; set; }

        [JsonIgnore()]
        public long? UserActionId { get; set; }

        [JsonProperty("items")]
        public List<ReleaseAnnouncementInfo> ReleaseAnnouncementInfos { get; set; }

        public ReleaseAnnouncementMaster()
        {

        }
        public ReleaseAnnouncementMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReleaseAnnouncementMaster>(srcObject, this);
            }
        }
    }
}
