using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class TemplateAccepted
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonIgnore()]
        public long? CompanyId { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("invoiceTemplateSampleId")]
        public long InvoiceTemplateSampleId { get; set; }
    }
}

