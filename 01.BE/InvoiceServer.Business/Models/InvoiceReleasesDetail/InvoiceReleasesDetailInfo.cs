using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceReleasesDetailInfo
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonIgnore()]
        [DataConvert("InvoiceConcludeId")]
        public long? InvoiceConcludeId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceTemplateId")]
        [DataConvert("RegisterTemplatesId")]
        public long? RegisterTemplatesId { get; set; }

        [JsonProperty("invoiceTypeId")]
        [DataConvert("RegisterTemplate.InvoiceSample.InvoiceTypeId")]
        public long? InvoiceTypeId { get; set; }

        [JsonProperty("invoiceTypeName")]
        [DataConvert("InvoiceTypeName")]
        public string InvoiceTypeName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("RegisterTemplateCode")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("symbol")]
        [DataConvert("Symbol")]
        public string Symbol { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceSampleId")]
        [DataConvert("InvoiceSample.Id", ThrowExceptionIfSourceNotExist = false)]
        public string InvoiceSampleId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceSampleName", NullValueHandling = NullValueHandling.Ignore)]
        [DataConvert("InvoiceSampleName", ThrowExceptionIfSourceNotExist = false)]
        public string InvoiceSampleName { get; set; }


        [StringTrimAttribute]
        [JsonProperty("description")]
        [DataConvert("Description")]
        public string Description { get; set; }

        public InvoiceReleasesDetailInfo()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceReleasesDetailInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceReleasesDetailInfo>(srcObject, this);
            }
        }
    }
}
