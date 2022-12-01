using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceVerificationCode
    {
        [JsonProperty("code")]
        public string VerificationCode { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonIgnore]
        public long CompanyId { get; set; }

        [JsonIgnore]
        public string NumberFormat { get; set; }

        public InvoiceVerificationCode()
        {
        }
    }
}
