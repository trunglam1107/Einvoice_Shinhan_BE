using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class SendMultiMail
    {
        [JsonProperty("ids")]
        public List<long> ids { get; set; }

        public SendMultiMail()
        {
        }
    }
}
