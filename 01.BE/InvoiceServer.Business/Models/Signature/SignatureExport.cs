using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class SignatureExport
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        [JsonProperty("items")]

        public List<SignatureInfo> Items { get; set; }

        public SignatureExport(CompanyInfo customer)
        {
            this.Company = Company;
        }
    }
}
