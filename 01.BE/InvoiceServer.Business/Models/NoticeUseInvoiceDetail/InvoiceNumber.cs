using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceNumber
    {
        [JsonProperty("startNumberOfInvoice")]
        public decimal StartNumberInvoice { get; private set; }

        public InvoiceNumber(decimal startNumberInvoice)
        {
            this.StartNumberInvoice = startNumberInvoice;
        }
    }
}
