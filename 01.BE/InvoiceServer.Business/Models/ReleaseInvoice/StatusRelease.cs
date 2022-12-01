using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class StatusRelease
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonIgnore]
        public long LoginId { get; set; }

        public StatusRelease()
        {

        }

        public StatusRelease(int status, string message)
            : this()
        {
            this.Status = status;
            this.Message = message;
        }

    }
}
