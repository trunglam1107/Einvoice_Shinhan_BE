using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceStatistical
    {
        [JsonProperty("id")]
        [DataConvert("ID")]
        public long Id { get; set; }
        [JsonProperty("invoiceId")]
        [DataConvert("INVOICEID")]
        public long? InvoiceId { get; set; }
        [JsonProperty("amount")]
        [DataConvert("AMOUNT")]
        public decimal? Amount { get; set; }

        [JsonProperty("tradDate")]
        [DataConvert("TRADDATE")]
        public string TradDate { get; set; }

        [JsonProperty("currency")]
        [DataConvert("CURRENCY")]
        public string Currency { get; set; }
        [JsonProperty("exchangeRate")]
        [DataConvert("EXCHANGERATE")]
        public decimal? ExchangeRate { get; set; }

        [JsonProperty("taxAmount")]
        [DataConvert("AMOUNTTAX")]
        public decimal? TaxAmount { get; set; }
        [JsonProperty("description")]
        [DataConvert("DESCRIPTION")]
        public string Description { get; set; }

        public InvoiceStatistical()
        {

        }
        public InvoiceStatistical(object srcObject)
           : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceStatistical>(srcObject, this);
            }
        }
    }
}
