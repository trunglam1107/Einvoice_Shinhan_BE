using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ProductExport
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        [JsonProperty("items")]

        public List<ProductInfo> Items { get; set; }

        public ProductExport(CompanyInfo company)
        {
            this.Company = company;
        }
    }
}
