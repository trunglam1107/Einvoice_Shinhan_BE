using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class CancellingInvoiceInfo
    {

        [StringTrimAttribute]
        [JsonProperty("invoiceId")]
        public long InvoiceId { get; set; }


        public CancellingInvoiceInfo()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public CancellingInvoiceInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, CancellingInvoiceInfo>(srcObject, this);
            }
        }
    }
}
