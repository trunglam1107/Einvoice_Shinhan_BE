using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ResultValidDate
    {
        [JsonProperty("result")]
        public int Result { get; set; }
    }
}
