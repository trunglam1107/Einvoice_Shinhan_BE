using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class RegisterTemplateMaster
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("CompanyName", ThrowExceptionIfSourceNotExist = false)]
        public string CompanyName { get; set; }
        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("prefix")]
        [DataConvert("Prefix")]
        public string Prefix { get; set; }

        [StringTrimAttribute]
        [JsonProperty("suffix")]
        [DataConvert("Suffix")]
        public string Suffix { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceTemplateId")]
        [DataConvert("InvoiceTemplateId")]
        public long InvoiceTemplateId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("InvoiceSample.Name")]
        public string Name { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        [DataConvert("Description")]
        public string Description { get; set; }

        [JsonProperty("createdDate")]
        [DataConvert("CreatedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? CreatedDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("contractStatus")]
        public int ContractStatus { get; set; }

        [JsonProperty("numberRecord")]
        [DataConvert("InvoiceTemplate.NumberRecord", ThrowExceptionIfSourceNotExist = false)]
        public int NumberRecord { get; set; }

        [JsonProperty("numberCharacterInLine")]
        [DataConvert("InvoiceTemplate.NumberCharacterInLine", ThrowExceptionIfSourceNotExist = false)]
        public int NumberCharacterInLine { get; set; }

        [StringTrimAttribute]
        [JsonProperty("isMultiTax")]
        [DataConvert("InvoiceSample.IsMultiTax", ThrowExceptionIfSourceNotExist = false, DefaultValue = 0)]
        public float IsMultiTax { get; set; }

        [StringTrimAttribute]
        [JsonProperty("isDiscount")]
        [DataConvert("InvoiceSample.IsDiscount", ThrowExceptionIfSourceNotExist = false, DefaultValue = 0)]
        public float IsDiscount { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceTemplateSampleId")]
        [DataConvert("InvoiceSample.InvoiceTemplateSampleId", ThrowExceptionIfSourceNotExist = false, DefaultValue = 0)]
        public long InvoiceTemplateSampleId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceTypeName")]
        [DataConvert("InvoiceSample.InvoiceType.Name", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string InvoiceTypeName { get; set; }

        [JsonProperty("invoiceTypeId")]
        [DataConvert("InvoiceSample.InvoiceType.Id", ThrowExceptionIfSourceNotExist = false)]
        public long InvoiceTypeId { get; set; }

        [JsonProperty("invoiceTemplateName")]
        [DataConvert("INVOICETEMPLATE.URLFILE", ThrowExceptionIfSourceNotExist = false)]
        public string InvoiceTemplateName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceSampleId")]
        [DataConvert("InvoiceSampleId")]
        public long InvoiceSampleId { get; set; }

        public RegisterTemplateMaster()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public RegisterTemplateMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RegisterTemplateMaster>(srcObject, this);
            }
        }
    }
}
