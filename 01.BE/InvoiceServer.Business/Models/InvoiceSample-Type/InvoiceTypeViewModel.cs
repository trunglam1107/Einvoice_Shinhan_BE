using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceTypeViewModel
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        [StringTrimAttribute]
        [JsonProperty("denominator")]
        [DataConvert("Denominator")]
        public string Denominator { get; set; }

        [StringTrimAttribute]
        [JsonProperty("notes")]
        [DataConvert("Note")]
        public string Note { get; set; }
    }
}
