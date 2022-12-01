using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models.Portal
{
    public class InvoiceExportPortal
    {
        //[JsonIgnore]
        //public CompanyInfo company { get; set; }

        [JsonProperty("items")]

        public List<PTInvoice> Items { get; set; }


    }
}
