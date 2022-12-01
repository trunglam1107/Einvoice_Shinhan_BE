using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class SummaryInvoice
    {
        [JsonProperty("numberNotRelease")]
        public decimal NumberNotRelease { get; set; }


        [JsonProperty("numberAdjustment")]
        public decimal NumberAdjustment { get; set; }

        [JsonProperty("numberCancel")]
        public decimal NumberCancel { get; set; }

        [JsonProperty("numberSubstitute")]
        public decimal NumberSubstitute { get; set; }

        [JsonProperty("numberDeleted")]
        public decimal NumberDeleted { get; set; }

        [JsonProperty("numberRelease")]
        public decimal NumberRelease { get; set; }

        public SummaryInvoice()
        {

        }

        public SummaryInvoice(decimal numberNotRelease, decimal numberAdjustment, decimal numberCancel, decimal numberSubstitute, decimal numberDeleted,decimal numberRelease)
        {
            this.NumberNotRelease = numberNotRelease;
            this.NumberRelease = numberRelease;
            this.NumberAdjustment = numberAdjustment;
            this.NumberCancel = numberCancel;
            this.NumberSubstitute = numberSubstitute;
            this.NumberDeleted = numberDeleted;
        }

    }
}
