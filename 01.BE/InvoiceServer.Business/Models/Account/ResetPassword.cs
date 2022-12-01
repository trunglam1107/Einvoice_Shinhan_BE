using InvoiceServer.Common;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ResetPassword
    {
        [StringTrimAttribute]
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        public string Email { get; set; }


        [JsonProperty("clientId")]
        public long ClientId { get; set; }
    }
}
