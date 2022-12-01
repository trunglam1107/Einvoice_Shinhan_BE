using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class SendMailTest
    {

        [JsonProperty("emailServer")]
        public EmailServerInfo EmailServer { get; set; }

        [JsonProperty("emailTo")]
        public string EmailTo { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
