using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceSampleInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long? Id { get; set; }

        [JsonProperty("code")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        [JsonProperty("isMultiTax")]
        [DataConvert("IsMultiTax")]
        public bool? IsMultiTax { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceTypeName")]
        [DataConvert("InvoiceTypeName")]
        public string InvoiceTypeName { get; set; }

        [JsonProperty("isDiscount")]
        [DataConvert("IsDiscount")]
        public bool? IsDiscount { get; set; }

        [JsonProperty("invoiceType")]
        [DataConvert("InvoiceTypeId")]
        public long InvoiceTypeId { get; set; }


        [JsonProperty("denominator")]
        [DataConvert("Denominator")]
        public string Denominator { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public Nullable<long> CompanyId { get; set; }

        [JsonProperty("fileName")]
        [DataConvert("FileName")]
        public string FileName { get; set; }

        [JsonProperty("notes")]
        [DataConvert("Note")]
        public string Note { get; set; }

        [JsonProperty("templateURL")]
        [DataConvert("TemplateURL")]
        public string TemplateURL { get; set; }

        [JsonProperty("invoiceTemplateSampleId")]
        [DataConvert("InvoiceTemplateSampleId")]
        public long? InvoiceTemplateSampleId { get; set; }

        [JsonProperty("registerTemplateCount")]
        //[DataConvert("Invoice")]
        public long? RegisterTemplateCount { get; set; }

        public InvoiceSampleInfo()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceSampleInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceSampleInfo>(srcObject, this);
            }
        }
    }
}
