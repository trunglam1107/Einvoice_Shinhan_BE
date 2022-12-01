using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class Result
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("releaseId")]
        public long ReleaseId { get; set; }

        public Result()
        {

        }
    }
}
