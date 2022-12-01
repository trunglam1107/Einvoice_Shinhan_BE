using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class RegisterTemplateInfo
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        //[JsonIgnore()]
        [JsonProperty("companyId")]
        [DataConvert("companyId")]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceSampleId")]
        [DataConvert("invoiceSampleId")]
        public long? InvoiceSampleId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("templateId")]
        [DataConvert("InvoiceTemplateId")]
        public long? InvoiceTemplateId { get; set; }


        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("Code")]
        public string Code { get; set; }


        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("InvoiceSample.Code")]
        public string Name { get; set; }

        [StringTrimAttribute]
        [JsonProperty("prefix")]
        [DataConvert("Prefix")]
        public string Prefix { get; set; }

        [StringTrimAttribute]
        [JsonProperty("suffix")]
        [DataConvert("Suffix")]
        public string Suffix { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        [DataConvert("Description")]
        public string Description { get; set; }

        [JsonProperty("isMultiTax")]
        [DataConvert("InvoiceSample.IsMultiTax")]
        public bool? IsMultiTax { get; set; }

        [JsonProperty("isDiscount")]
        [DataConvert("InvoiceSample.IsDiscount")]
        public bool? IsDiscount { get; set; }

        [JsonProperty("invoiceTypeId")]
        [DataConvert("InvoiceSample.InvoiceTypeId")]
        public long InvoiceTypeId { get; set; }


        //[JsonProperty("notificationUseInvoiceDetails")]
        //[DataConvert("NotificationUseInvoiceDetails.Count")]
        public int NotificationUseInvoiceDetails { get; set; }

        public RegisterTemplateInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public RegisterTemplateInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RegisterTemplateInfo>(srcObject, this);
            }
        }
    }
}
