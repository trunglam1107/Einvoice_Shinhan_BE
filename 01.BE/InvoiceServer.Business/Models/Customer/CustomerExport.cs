using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class CustomerExport
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        [JsonProperty("items")]

        public List<CustomerInfo> Items { get; set; }

        public CustomerExport(CompanyInfo customer)
        {
            this.Company = Company;
        }
    }
}
