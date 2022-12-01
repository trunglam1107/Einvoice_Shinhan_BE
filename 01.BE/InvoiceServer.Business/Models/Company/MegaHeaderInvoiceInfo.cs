using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class MegaHeaderInvoiceInfo
    {
        [JsonProperty("companyNameVn")]
        public string CompanyNameVn { get; set; }

        [JsonProperty("companyBranchVn")]
        public string CompanyBranchVn { get; set; }

        [JsonProperty("companyAddressRow1")]
        public string CompanyAddressRow1 { get; set; }

        [JsonProperty("companyAddressRow2")]
        public string CompanyAddressRow2 { get; set; }

        public MegaHeaderInvoiceInfo()
        {

        }
    }
}
