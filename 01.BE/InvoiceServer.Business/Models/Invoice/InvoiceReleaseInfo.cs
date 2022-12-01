using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceReleaseInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }

        [JsonProperty("converted")]
        public bool Converted { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("childId")]
        public long ChildId { get; set; }

        public InvoiceReleaseInfo()
        {

        }
    }
}

