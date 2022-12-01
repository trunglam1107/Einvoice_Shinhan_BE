using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class HuaNanHeaderInvoiceInfo
    {
        [JsonProperty("companyNameCn")]
        public string CompanyNameCn { get; set; }

        [JsonProperty("companyBranchCn")]
        public string CompanyBranchCn { get; set; }

        [JsonProperty("companyNameVn")]
        public string CompanyNameVn { get; set; }

        [JsonProperty("companyBranchVn")]
        public string CompanyBranchVn { get; set; }

        public HuaNanHeaderInvoiceInfo()
        {

        }
    }
}
