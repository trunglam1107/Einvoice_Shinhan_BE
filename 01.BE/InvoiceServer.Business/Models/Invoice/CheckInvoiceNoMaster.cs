using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.Models
{
    public class CheckInvoiceNoMaster
    {
        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        [JsonProperty("companyId")]
        public long CompanyId { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("minInvoiceNo")]
        public decimal MinInvoiceNo { get; set; }

        [JsonProperty("countInvoiceNo")]
        public decimal CountInvoiceNo { get; set; }
    }
}
