using InvoiceServer.Common;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceNo
    {
        [StringTrimAttribute]
        [JsonProperty("no")]
        public string No { get; set; }

        public InvoiceNo(string no)
        {
            this.No = no;
        }
    }
}
