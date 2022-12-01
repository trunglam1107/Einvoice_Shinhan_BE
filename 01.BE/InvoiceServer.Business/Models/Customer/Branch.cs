using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class Branch
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }

        public Branch()
        {

        }

    }
}
