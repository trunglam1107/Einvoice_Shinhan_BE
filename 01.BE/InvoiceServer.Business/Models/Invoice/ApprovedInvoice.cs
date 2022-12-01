using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ApprovedInvoice
    {
        [JsonIgnore()]
        public long UserAction { get; set; }

        [JsonIgnore()]
        public long CompanyId { get; set; }

        [JsonIgnore()]
        public long? UserActionId { get; set; }

        [JsonProperty("items")]
        public List<ReleaseInvoiceInfo> ReleaseInvoiceInfos { get; set; }

        public ApprovedInvoice()
        {

        }

    }
}
