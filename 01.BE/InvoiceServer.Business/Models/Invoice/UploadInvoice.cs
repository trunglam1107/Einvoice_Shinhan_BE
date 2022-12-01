using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class UploadInvoice
    {
        [JsonProperty("invoiceSampleId")]
        public long RegisterTemplateId { get; set; }// Mẫu số 

        [JsonProperty("symbol")]
        public string Symbo { get; set; } // Ký Hiệu 

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonIgnore()]
        public long UserActionId { get; set; }
    }
}
