using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class NoticePrintInvoice
    {
        public VendorInfo Vendor { get; set; }

        [JsonProperty("invoiceSampleCode")] //2
        public string InvoiceSampleCode { get; set; }

        [JsonProperty("invoiceSample")] //2
        public string InvoiceSample { get; set; }

        [JsonProperty("code")] //3
        public string InvoiceCode { get; set; }

        [JsonProperty("symbol")] //4
        public string InvoiceSymbol { get; set; }

        public string NumberFromUse { get; set; }

        public string NumberToUse { get; set; }

        public decimal? NumberTotall { get; set; }

    }
}
