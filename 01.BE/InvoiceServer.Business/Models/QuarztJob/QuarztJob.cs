using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class QuarztJob
    {
        [JsonProperty("id")]
        [DataConvert("ID")]
        public long ID { get; set; }

        [JsonProperty("JobID")]
        [DataConvert("JOBID")]
        public long JOBID { get; set; }

        [JsonProperty("ScheduleName")]
        [DataConvert("SCHEDULENAME")]
        public string SCHEDULENAME { get; set; }

        [JsonProperty("JobName")]
        [DataConvert("JOBNAME")]
        public string JOBNAME { get; set; }

        [JsonProperty("Status")]
        [DataConvert("STATUS")]
        public bool STATUS { get; set; }

        [JsonProperty("CronExpression")]
        [DataConvert("SCRONEXPRESSION")]
        public string SCRONEXPRESSION { get; set; }

        [JsonProperty("NextTimeRun")]
        [DataConvert("NEXTTIMERUN")]
        public string NEXTTIMERUN { get; set; }

        [JsonProperty("LastTimeRun")]
        [DataConvert("LASTTIMERUN")]
        public string LASTTIMERUN { get; set; }


        [JsonProperty("MethodName")]
        [DataConvert("METHODNAME")]
        public string METHODNAME { get; set; }

        [JsonProperty("Processing")]
        [DataConvert("PROCESSING")]
        public bool PROCESSING { get; set; }
    }
}
