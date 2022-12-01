using InvoiceServer.Common;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class SaveSystemLog
    {
        [StringTrimAttribute]
        [JsonProperty("functionCode")]
        public string FunctionCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("logSummary")]
        public string LogSummary { get; set; }

        [JsonProperty("logType")]
        public int LogType { get; set; }

        [StringTrimAttribute]
        [JsonProperty("logDetail")]
        public string LogDetail { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        public SaveSystemLog()
        {
        }
    }

}
