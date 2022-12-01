using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ChangeLanguage
    {
        [JsonProperty("language")]
        public string Language { get; set; }
    }
}
