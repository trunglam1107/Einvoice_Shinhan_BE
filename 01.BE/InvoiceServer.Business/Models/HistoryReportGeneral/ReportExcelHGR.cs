using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models.HistoryReportGeneral
{
    public class ReportExcelHGR
    {
        [JsonProperty("items")]

        public List<HistoryReportGeneralInfo> Items { get; set; }

        public ReportExcelHGR()
        {

        }
    }
}
