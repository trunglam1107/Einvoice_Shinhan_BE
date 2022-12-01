using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ValidatorResult
    {
        [JsonProperty("result")]
        public string Result { get; set; }

    }
}
