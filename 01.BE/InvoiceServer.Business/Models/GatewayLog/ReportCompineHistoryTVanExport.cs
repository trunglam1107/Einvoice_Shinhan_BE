using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models.GatewayLog
{
    public class ReportCompineHistoryTVanExport
    {
        [JsonIgnore]

        [JsonProperty("items")]
        public List<GatewaylogDetail> Items { get; set; }

        public CompanyInfo Company { get; set; }

        public ReportCompineHistoryTVanExport(CompanyInfo company)
        {
            this.Company = company;
        }

    }
}
