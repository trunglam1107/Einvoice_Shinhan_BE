using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ReportCancellingDetail1
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("reportCancellingId")]
        public long ReportCancellingId { get; set; }

        [JsonProperty("notificationUseInvoiceDetailId")]
        public long NotificationUseInvoiceDetailId { get; set; }

        [JsonProperty("registerTemplatesId")]
        public long RegisterTemplatesId { get; set; }

        [JsonProperty("numberFrom")]
        public string NumberFrom { get; set; }

        [JsonProperty("numberTo")]
        public string NumberTo { get; set; }

        [JsonProperty("Number")]
        public decimal Number { get; set; }

        public ReportCancellingDetail1()
        {

        }

    }
}
