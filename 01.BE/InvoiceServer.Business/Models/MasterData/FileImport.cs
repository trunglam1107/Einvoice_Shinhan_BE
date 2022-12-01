using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class FileImport
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonIgnore()]
        public long UserActionId { get; set; }
    }
}
