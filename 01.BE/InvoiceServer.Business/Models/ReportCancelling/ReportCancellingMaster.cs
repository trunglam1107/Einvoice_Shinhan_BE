using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReportCancellingMaster
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("typeCancelling")]
        [DataConvert("TypeCancelling")]
        public string TypeCancelling { get; set; }

        [JsonProperty("timeCancelling")]
        public string TimeCancelling { get; set; }

        [JsonProperty("dateCreated")]
        [DataConvert("CreatedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("createdBy")]
        [DataConvert("CreatedBy")]

        public string CreatedBy { get; set; }

        [JsonIgnore]
        [DataConvert("HourCancelling")]
        public int HourCancelling { get; set; }

        [JsonIgnore]
        [DataConvert("MinuteCancelling")]
        public int MinuteCancelling { get; set; }


        [JsonProperty("status")]
        [DataConvert("status")]
        public int Status { get; set; }

        public ReportCancellingMaster()
        {

        }

        public ReportCancellingMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReportCancellingMaster>(srcObject, this);
            }

            this.TimeCancelling = string.Format("{0} Giờ {1} Phút", this.HourCancelling, this.MinuteCancelling);
        }

    }
}
