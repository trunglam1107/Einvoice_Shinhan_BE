using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class HistoryReportItem
    {
        public string MessageCode { get; set; }
        public int? CQTStatus { get; set; }
        public int? Status { get; set; }
        public long? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string BranchId { get; set; }
        public string Symbol { get; set; }  
        public decimal InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public long SBTHDLIEU { get; set; }

    }
}
