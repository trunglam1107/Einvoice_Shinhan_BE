using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class PasswordInfo
    {
        [JsonProperty("currentPassword")]
        public string CurrentPassword { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
        [JsonProperty("isReset")]
        public bool IsReset { get; set; }
    }
}
