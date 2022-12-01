using InvoiceServer.Common;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class BIDCImportFromAPI
    {
        [StringTrimAttribute]
        [JsonProperty("transRef")]
        public string TransRef { get; set; }
    }
}
