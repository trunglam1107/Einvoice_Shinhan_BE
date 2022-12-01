using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class InvoiceCkNumberExport
    {
        [JsonIgnore]
        public CompanyInfo company { get; set; }

        [JsonProperty("items")]

        public List<InvoiceCheckDaily> Items { get; set; }

       
        public InvoiceCkNumberExport(CompanyInfo company)
        {
            this.company = company;
        }
    }
}
