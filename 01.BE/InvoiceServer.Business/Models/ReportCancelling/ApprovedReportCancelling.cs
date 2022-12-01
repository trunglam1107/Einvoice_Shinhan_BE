using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models.ReportCancelling
{
    public class ApprovedReportCancelling
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonIgnore()]
        public long UserAction { get; set; }

        [JsonIgnore()]
        public long CompanyId { get; set; }

        [JsonIgnore()]
        public long? UserActionId { get; set; }

        [JsonProperty("items")]
        public List<ReportCancellingInfo> ReportCancellingInfos { get; set; }

        public ApprovedReportCancelling()
        {

        }
    }
}
