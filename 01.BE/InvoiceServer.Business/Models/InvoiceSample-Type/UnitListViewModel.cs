using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class UnitListViewModel
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }
    }
}
