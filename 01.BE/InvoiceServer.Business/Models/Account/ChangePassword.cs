using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ChangePassword
    {
        [JsonProperty("password")]
        public string NewPassword { get; set; }

        [JsonProperty("oldPassword")]
        public string OldPassword { get; set; }

        [JsonProperty("isForgot")]
        public bool? IsForgot { get; set; }
    }
}
