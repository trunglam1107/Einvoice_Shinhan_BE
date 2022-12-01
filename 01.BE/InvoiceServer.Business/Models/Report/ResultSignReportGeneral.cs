using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
   public class ResultSignReportgeneral
    {
        public long Id { get; set; }
        public string Message { get; set; }

        public int? StatusCQT { get; set; }

        public string MessageCode { get; set; }

        public string MLDiep { get; set; }

        public string LTBAO { get; set; }
    }
}
