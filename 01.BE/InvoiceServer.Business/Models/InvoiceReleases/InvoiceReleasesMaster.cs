using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceReleasesMaster
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public string Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("MyCompany.CompanyName")]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("no")]
        [DataConvert("Code")]
        public string Code { get; set; }


        [StringTrimAttribute]
        [JsonProperty("persionSuggest")]
        [DataConvert("PersionSuggest")]
        public string PersionSuggest { get; set; }

        [StringTrimAttribute]
        [JsonProperty("status")]
        [DataConvert("Status")]
        public int Status { get; set; }

        [StringTrimAttribute]
        [JsonProperty("dateRelease")]
        [DataConvert("ReleasedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        public InvoiceReleasesMaster()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceReleasesMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceReleasesMaster>(srcObject, this);
            }
        }
    }
}
