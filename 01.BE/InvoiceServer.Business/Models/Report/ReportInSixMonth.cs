using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ReportInSixMonth
    {
        [JsonIgnore]
        public long NotificationUseInvoiceDetailId { get; set; }

        [JsonProperty("DaSuDung")] //18
        public long DaSuDung { get; set; }

        [JsonProperty("ChuaSuDung")] //18
        public long ChuaSuDung { get; set; }

        [JsonProperty("MonthYear")] //18
        public string MonthYear { get; set; }
    }
}
