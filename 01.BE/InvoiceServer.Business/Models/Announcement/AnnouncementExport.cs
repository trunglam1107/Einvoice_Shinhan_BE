using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class AnnouncementExport
    {
        [JsonIgnore]
        public CompanyInfo company { get; set; }

        [JsonProperty("items")]

        public List<AnnouncementMaster> Items { get; set; }

        public AnnouncementExport(CompanyInfo company)
        {
            this.company = company;
        }
    }
}
