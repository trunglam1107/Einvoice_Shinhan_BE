using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReportCancellingInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonIgnore]
        public long CompanyId { get; set; }

        [JsonProperty("typeCancelling")]
        [DataConvert("TypeCancelling")]
        public string TypeCancelling { get; set; }

        [JsonProperty("hourCancelling")]
        [DataConvert("HourCancelling")]
        public int HourCancelling { get; set; }

        [JsonProperty("minuteCancelling")]
        [DataConvert("MinuteCancelling")]
        public int MinuteCancelling { get; set; }


        [JsonProperty("dateCancelling")]
        [DataConvert("CancellingDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime CancellingDate { get; set; }

        [JsonProperty("dateCreate")]
        [DataConvert("CreatedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("delegate")]
        [DataConvert("Delegate")]
        public string Delegate { get; set; }


        [JsonProperty("createdBy")]
        [DataConvert("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("updateBy")]
        [DataConvert("updatedBy")]
        public long? UpdateBy { get; set; }


        [JsonProperty("updatedDate")]
        [DataConvert("updatedDate")]
        public string UpdateDate { get; set; }


        [JsonProperty("status")]
        [DataConvert("status")]
        public int Status { get; set; }

        [JsonProperty("items")]
        public List<RegisterTemplateCancelling> RegisterTemplatesCancelling { get; set; }

        public ReportCancellingInfo()
        {

        }

        public ReportCancellingInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReportCancellingInfo>(srcObject, this);
            }

        }

    }
}
