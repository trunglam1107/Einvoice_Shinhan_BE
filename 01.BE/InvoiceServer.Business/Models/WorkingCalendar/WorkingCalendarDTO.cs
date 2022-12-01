using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    /// <summary>
    /// Interactive with Frontend
    /// </summary>
    public class WorkingCalendarDTO
    {
        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("dayOfYear")]
        public int DayOfYear { get; set; }

        [JsonProperty("dayOfMonth")]
        public int DayOfMonth { get; set; }

        [JsonProperty("dateType")]
        public string DateType { get; set; }
    }
}
