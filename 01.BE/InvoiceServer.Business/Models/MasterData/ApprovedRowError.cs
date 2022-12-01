using InvoiceServer.Common;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ApprovedRowError
    {
        [JsonProperty("errorCode", NullValueHandling = NullValueHandling.Ignore)]
        public ResultCode ErrorCode { get; set; }

        [JsonProperty("invoiceNo", NullValueHandling = NullValueHandling.Ignore)]
        public string InvoiceNo { get; set; }

        public ApprovedRowError(ResultCode errorCode, string invoiceNo)
        {
            this.ErrorCode = errorCode;
            this.InvoiceNo = invoiceNo;
        }
    }
}
