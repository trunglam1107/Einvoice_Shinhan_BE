using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class CompanyUpgradeInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long? Id { get; set; }

        [JsonProperty("type")]
        [DataConvert("Type")]
        public long? Type { get; set; }

        [JsonProperty("companySid")]
        [DataConvert("CompanySid")]
        public long CompanySid { get; set; }

        [JsonProperty("companyid")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [JsonProperty("address")]
        [DataConvert("Address")]
        public string Address { get; set; }

        [JsonProperty("tax")]
        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("tel")]
        [DataConvert("Tel")]
        public string Tel { get; set; }

        [JsonProperty("delegate")]
        [DataConvert("Delegate")]
        public string Delegate { get; set; }

        [JsonProperty("applyDate")]
        [DataConvert("ApplyDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public Nullable<DateTime> ApplyDate { get; set; }

        public CompanyUpgradeInfo()
        {

        }
        public CompanyUpgradeInfo(object srcObject)
           : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, CompanyUpgradeInfo>(srcObject, this);
            }
        }
    }
}
