using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class SystemLogExport
    {
        [JsonIgnore]
        public CompanyInfo company { get; set; }

        [JsonProperty("items")]

        public List<SystemLogInfo> Items { get; set; }

        public SystemLogExport(CompanyInfo company)
        {
            this.company = company;
        }
    }
}
