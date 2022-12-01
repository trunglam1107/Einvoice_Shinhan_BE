using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceNote
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonIgnore()]
        public long? CompanyId { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        public InvoiceNote()
        {
        }
    }
}
