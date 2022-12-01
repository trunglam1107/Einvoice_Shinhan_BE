using Newtonsoft.Json;

namespace InvoiceServer.Business.Models.Portal
{
    public class PTFooterInfo
    {
        [JsonProperty("footerCompanyName")]
        public string FooterCompanyName { get; set; }

        [JsonProperty("footerCompanyNameEN")]
        public string FooterCompanyNameEN { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("footerPhone")]
        public string FooterPhone { get; set; }

        [JsonProperty("addressStreet")]
        public string AddressStreet { get; set; }

        [JsonProperty("addressStreetEN")]
        public string AddressStreetEN { get; set; }

        [JsonProperty("addressProvince")]
        public string AddressProvince { get; set; }

        [JsonProperty("addressProvinceEN")]
        public string AddressProvinceEN { get; set; }

        [JsonProperty("phoneEmergency")]
        public string PhoneEmergency { get; set; }

        [JsonProperty("youtube")]
        public string Youtube { get; set; }

        [JsonProperty("facebook")]
        public string Facebook { get; set; }


        [JsonProperty("linkAddressTransaction")]
        public string LinkAddressTransaction { get; set; }

        [JsonProperty("textCopyright")]
        public string TextCopyright { get; set; }

        [JsonProperty("linkPrivacyPolicy")]
        public string LinkPrivacyPolicy { get; set; }

        [JsonProperty("linkTermsOfUse")]
        public string LinkTermsOfUse { get; set; }
    }
}
