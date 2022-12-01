using InvoiceServer.Common;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class LoginInfo
    {
        [StringTrimAttribute]
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
