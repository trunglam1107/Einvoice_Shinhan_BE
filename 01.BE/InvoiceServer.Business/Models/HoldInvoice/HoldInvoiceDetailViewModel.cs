using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class HoldInvoiceDetailViewModel
    {
        [JsonProperty("id")]
        [DataConvert("ID")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceNo")]
        [DataConvert("INVOICENO")]
        public string InvoiceNo { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceStatus")]
        [DataConvert("INVOICESTATUS")]
        public int InvoiceStatus { get; set; }

        public HoldInvoiceDetailViewModel()
        {
        }

        public HoldInvoiceDetailViewModel(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, HoldInvoiceDetailViewModel>(srcObject, this);
            }
        }
    }
}
