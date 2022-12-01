using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class UseInvoiceDetailInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("invoiceTemplateId")]
        [DataConvert("RegisterTemplatesId")]
        public long? RegisterTemplateId { get; set; }

        [JsonProperty("invoiceTypeId")]
        [DataConvert("RegisterTemplate.InvoiceSample.InvoiceTypeId")]
        public long? InvoiceTypeId { get; set; }

        [JsonProperty("invoiceTypeName")]
        [DataConvert("InvoiceTypeName")]
        public string InvoiceTypeName { get; set; }

        [JsonProperty("code")]
        [DataConvert("RegisterTemplateCode", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string RegisterTemplateCode { get; set; }

        [JsonProperty("name")]
        [DataConvert("InvoiceSampleName", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string InvoiceSampleName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("prefix")]
        [DataConvert("Prefix")]
        public string Prefix { get; set; }

        [StringTrimAttribute]
        [JsonProperty("suffix")]
        [DataConvert("Suffix")]
        public string Suffix { get; set; }

        [JsonProperty("symbol")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [JsonProperty("number")]
        [DataConvert("NumberUse")]
        public decimal? NumberUse { get; set; }

        [JsonProperty("numberFrom")]
        [DataConvert("NumberFrom")]
        public string NumberFrom { get; set; }

        [JsonProperty("numberTo")]
        [DataConvert("NumberTo")]
        public string NumberTo { get; set; }

        [JsonProperty("numberInvoice")]
        [DataConvert("Contract.NumberInvoice", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public Decimal? NumberInvoice { get; set; }

        [JsonProperty("dateOfUse")]
        [DataConvert("UsedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime UsedDate { get; set; }

        [DataConvert("Contract.No", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        [JsonProperty("contractNo")]
        public string ContractNo { get; set; }

        [JsonProperty("contractDate")]
        [DataConvert("Contract.CreatedDate", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        [JsonConverter(typeof(JsonDateConverterString))]
        public string ContractDate { get; set; }


        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long CompanyId { get; set; }


        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("MYCOMPANY.COMPANYNAME", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("levelCustomer")]
        [DataConvert("MYCOMPANY.LEVELCUSTOMER", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string LevelCustomer { get; set; }

        public UseInvoiceDetailInfo()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public UseInvoiceDetailInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, UseInvoiceDetailInfo>(srcObject, this);
            }
        }
    }
}
