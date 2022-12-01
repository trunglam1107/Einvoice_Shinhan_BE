using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class HistoryReportGeneralInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long? Id { get; set; }

        [JsonProperty("fileName")]
        [DataConvert("FileName")]
        public string FileName { get; set; }

        [JsonProperty("additionalTimes")]
        [DataConvert("AdditionalTimes")]
        public decimal? AdditionalTimes { get; set; }

        [JsonProperty("messageCode")]
        [DataConvert("MessageCode")]
        public string MessageCode { get; set; }

        [JsonProperty("month")]
        [DataConvert("Month")]
        public int? Month { get; set; }

        [JsonProperty("quarter")]
        [DataConvert("Quarter")]
        public int? Quarter { get; set; }

        [JsonProperty("year")]
        [DataConvert("Year")]
        public int? Year { get; set; }

        [JsonProperty("cqtStatus")]
        [DataConvert("CQTStatus")]
        public int? CQTStatus { get; set; }

        [JsonProperty("periodsReport")]
        [DataConvert("PeriodsReport")]
        public decimal? PeriodsReport { get; set; }

        [JsonProperty("status")]
        [DataConvert("Status")]
        public int? Status { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [JsonProperty("companyName")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [JsonProperty("branchId")]
        [DataConvert("BranchId")]
        public string BranchId { get; set; }

        [JsonProperty("lanNop")]
        [DataConvert("LanNop")]
        public string LanNop { get; set; }

        [JsonProperty("mst")]
        [DataConvert("MST")]
        public string MST { get; set; }

        [JsonProperty("error")]
        [DataConvert("Error")]
        public string Error { get; set; }

        [JsonProperty("createDate")]
        [DataConvert("CreateDate")]
        public DateTime? CreateDate { get; set; }

        [JsonProperty("mltdiep")]
        [DataConvert("MLTDIEP")]
        public string MLTDIEP { get; set; }

        [JsonProperty("ltbao")]
        [DataConvert("LTBAO")]
        public string LTBAO { get; set; }

        [JsonProperty("SBTHDulieu")]
        [DataConvert("SBTHDulieu")]
        public long? SBTHDulieu { get; set; }

        public HistoryReportGeneralInfo()
        {
        }

        public HistoryReportGeneralInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, HistoryReportGeneralInfo>(srcObject, this);
            }
        }
    }
}
