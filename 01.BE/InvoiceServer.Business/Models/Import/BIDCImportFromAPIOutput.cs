using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class BIDCImportFromAPIOutput
    {
        [JsonProperty("TXNREFCODE")]
        public string TXNREFCODE { get; set; }

        [JsonProperty("STATUS")]
        public string STATUS { get; set; }

        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }


        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long CompanyId { get; set; }

        [JsonProperty("no")]
        [DataConvert("No")]
        public string No { get; set; }

        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("customerCode")]
        public string CustomerCode { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("customerTaxCode")]
        public string CustomerTaxCode { get; set; }

        [JsonProperty("registerTemplateId")]
        [DataConvert("RegisterTemplateId")]
        public long RegisterTemplateId { get; set; }

        [JsonProperty("symbol")]
        [DataConvert("Symbol")]
        public string Symbol { get; set; }

        [JsonProperty("dateRelease")]
        [DataConvert("ReleasedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        [JsonProperty("refNumber")]
        [DataConvert("refNumber")]
        public string RefNumber { get; set; }

        public BIDCImportFromAPIOutput()
        {
        }

        public BIDCImportFromAPIOutput(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, BIDCImportFromAPIOutput>(srcObject, this);
            }
        }
    }
}
