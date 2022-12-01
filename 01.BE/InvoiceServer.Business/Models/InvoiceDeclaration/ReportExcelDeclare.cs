using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class ReportExcelDeclare
    {
        [JsonProperty("items")]

        public List<DeclarationInfo> Items { get; set; }

        public ReportExcelDeclare()
        {

        }
    }
}
