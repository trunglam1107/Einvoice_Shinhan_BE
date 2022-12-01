using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class FooterInvoiceInfo
    {
        [JsonProperty("footerCompanyName")]
        public string FooterCompanyName { get; set; }

        [JsonProperty("footerTaxCode")]
        public string FooterTaxCode { get; set; }

        [JsonProperty("footerPhone")]
        public string FooterPhone { get; set; }

        [JsonProperty("footerLink")]
        public string FooterLink { get; set; }

        [JsonProperty("footerLinkText")]
        public string FooterLinkText { get; set; }

        [JsonProperty("footerPortalLink")]
        public string FooterPortalLink { get; set; }

        [JsonProperty("footerPortalLinkText")]
        public string FooterPortalLinkText { get; set; }

        public FooterInvoiceInfo()
        {

        }
    }
}
