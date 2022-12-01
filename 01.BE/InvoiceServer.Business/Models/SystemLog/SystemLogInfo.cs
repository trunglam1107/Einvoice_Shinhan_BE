using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class SystemLogInfo
    {
        [StringTrimAttribute]
        [JsonProperty("functionCode")]
        [DataConvert("FunctionCode")]
        public string FunctionCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("functionName")]
        [DataConvert("FunctionName")]
        public string FunctionName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("logSummary")]
        [DataConvert("LogSummary")]
        public string LogSummary { get; set; }

        [JsonProperty("logType")]
        [DataConvert("LogType")]
        public decimal? LogType { get; set; }

        [StringTrimAttribute]
        [JsonProperty("logDate")]
        [DataConvert("LogDate")]
        public System.DateTime? LogDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("logDetail")]
        [DataConvert("LogDetail")]
        public string LogDetail { get; set; }

        [StringTrimAttribute]
        [JsonProperty("ip")]
        [DataConvert("IP")]
        public string IP { get; set; }

        [StringTrimAttribute]
        [JsonProperty("userName")]
        [DataConvert("UserName")]
        public string UserName { get; set; }

        public SystemLogInfo()
        {

        }
        public SystemLogInfo(object srcObject)
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, SystemLogInfo>(srcObject, this);
            }
        }
    }

}
