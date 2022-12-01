using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class ReportHistoryXml
    {

        [JsonProperty("id")]
        public long Id { get; set; }


        public ExportFileInfo fileInfo { get; set; }

        [JsonProperty("companyId")]
        public long? CompanyId { get; set; }

        public List<long> listInvoiceId { get; set; }

        public string CompanyTaxCode { get; set; }
        public ReportHistoryXml()
        {
        }
        public ReportHistoryXml(ExportFileInfo srcObject, long Id, List<long> listInvoiceId)
        {
            this.fileInfo = srcObject;
            this.Id = Id;
            this.listInvoiceId = listInvoiceId;
        }

    }
}
