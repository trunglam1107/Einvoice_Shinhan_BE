using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class StatusReleaseInvoice
    {
        [JsonProperty("invoicesRelease")]
        public List<InvoicesRelease> InvoicesRelease { get; set; }
    }
}
