using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceTemplateInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        [JsonProperty("urlFile")]
        [DataConvert("UrlFile")]
        public string UrlFile { get; set; }

        [JsonProperty("imageTemplate")]
        [DataConvert("ImageTemplate")]
        public string ImageTemplate { get; set; }

        [JsonProperty("detailInvoice")]
        [DataConvert("DetailInvoice")]
        public string DetailInvoice { get; set; }

        [JsonProperty("headerInvoice")]
        [DataConvert("HeaderInvoice")]
        public string HeaderInvoice { get; set; }

        [JsonProperty("isMultiTax")]
        [DataConvert("IsMultiTax")]
        public bool? IsMultiTax { get; set; }

        [JsonProperty("isDiscount")]
        [DataConvert("IsDiscount")]
        public bool? IsDiscount { get; set; }

        public InvoiceTemplateInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceTemplateInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceTemplateInfo>(srcObject, this);
            }
        }
    }
}
