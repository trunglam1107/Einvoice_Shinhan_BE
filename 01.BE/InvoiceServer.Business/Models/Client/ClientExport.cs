using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ClientExport
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        public string Username { get; set; }

        [JsonProperty("items")]

        public List<ClientDetail> Items { get; set; }

        public ClientExport(CompanyInfo company)
        {
            this.Company = company;
        }
    }
}
