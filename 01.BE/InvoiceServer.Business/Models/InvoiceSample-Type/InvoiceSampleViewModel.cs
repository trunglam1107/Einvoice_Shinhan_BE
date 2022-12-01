using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceSampleViewModel
    {
        [StringTrimAttribute]
        [JsonProperty("invoiceType")]
        [DataConvert("InvoiceType")]
        public string InvoiceType { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceTypeName")]
        [DataConvert("InvoiceTypeName")]
        public string InvoiceTypeName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        [StringTrimAttribute]
        [JsonProperty("nameInvoiceTypeTax")]
        [DataConvert("NameInvoiceType")]
        public string NameInvoiceType { get; set; }

        [StringTrimAttribute]
        [JsonProperty("denominator")]
        [DataConvert("Denominator")]
        public string Denominator { get; set; }

        [StringTrimAttribute]
        [JsonProperty("fileName")]
        [DataConvert("FileName")]
        public string FileName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("templateURL")]
        [DataConvert("TemplateURL")]
        public string TemplateURL { get; set; }


        [StringTrimAttribute]
        [JsonProperty("notes")]
        [DataConvert("Note")]
        public string Note { get; set; }

        [StringTrimAttribute]
        [JsonProperty("isMultiTax")]
        [DataConvert("IsMultiTax")]
        public bool? IsMultiTax { get; set; }

        [StringTrimAttribute]
        [JsonProperty("isDiscount")]
        [DataConvert("IsDiscount")]
        public int? IsDiscount { get; set; }

        public InvoiceSampleViewModel()
        {
        }
        public InvoiceSampleViewModel(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceSampleViewModel>(srcObject, this);
            }
        }
    }
}
