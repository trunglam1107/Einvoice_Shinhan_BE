using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ReportListDebts
    {
        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("personContact")]
        public string PersonContact { get; set; }

        [JsonProperty("isOrg")]
        public bool? IsOrg { get; set; }

        [JsonProperty("opening")]
        public decimal Opening { get; set; }

        [JsonProperty("happening")]
        public decimal Happening { get; set; }

        [JsonIgnore]
        public decimal HappeningNotPayment { get; set; }

        [JsonProperty("closing")]
        public decimal Closing
        {
            get
            {
                return (this.Opening + this.HappeningNotPayment);
            }
        }


        public ReportListDebts()
        {
        }
    }
}
