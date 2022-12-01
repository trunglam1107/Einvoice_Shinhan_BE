using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class InvoiceReleasesInfo
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxCode")]
        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("no")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("manager")]
        [DataConvert("Manager")]
        public string Manager { get; set; }

        [StringTrimAttribute]
        [JsonProperty("persionSuggest")]
        [DataConvert("PersionSuggest")]
        public string PersonSuggest { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note")]
        [DataConvert("Note")]
        public string Note { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note1")]
        [DataConvert("Note1")]
        public string Note1 { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note2")]
        [DataConvert("Note2")]
        public string Note2 { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note3")]
        [DataConvert("Note3")]
        public string Note3 { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note4")]
        [DataConvert("Note4")]
        public string Note4 { get; set; }

        [StringTrimAttribute]
        [JsonProperty("dateRelease")]
        [DataConvert("ReleasedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("recipients")]
        [DataConvert("Recipients")]
        public string Recipients { get; set; }

        [StringTrimAttribute]
        [JsonProperty("cityId")]
        [DataConvert("CityId")]
        public long? CityId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("items")]
        public List<InvoiceReleasesDetailInfo> InvoiceReleasesDetailInfo { get; set; }

        [StringTrimAttribute]
        [JsonProperty("status")]
        [DataConvert("Status")]
        public int? Status { get; set; }

        [StringTrimAttribute]
        [JsonProperty("createdDate")]
        [DataConvert("CreatedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? CreatedDate { get; set; }

        [JsonProperty("cityName", NullValueHandling = NullValueHandling.Ignore)]
        [DataConvert("CITYNAME", ThrowExceptionIfSourceNotExist = false)]
        public string CityName { get; set; }

        [JsonIgnore()]
        public long UserActionId { get; set; }

        public InvoiceReleasesInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceReleasesInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceReleasesInfo>(srcObject, this);
            }
        }

        public InvoiceReleasesInfo(object srcObject, List<InvoiceReleasesDetailInfo> concludeDetailInfos)
            : this(srcObject)
        {
            this.InvoiceReleasesDetailInfo = concludeDetailInfos;
        }
    }
}
