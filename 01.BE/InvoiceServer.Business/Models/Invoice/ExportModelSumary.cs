using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class ExportModelSumary
    {
        public List<InvoiceExportModel> invoiceExportPdfModels { get; set; }
        public List<InvoiceExportXmlModel> invoiceExportXmlModels { get; set; }
    }
}
