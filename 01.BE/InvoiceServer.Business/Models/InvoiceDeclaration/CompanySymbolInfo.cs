using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace InvoiceServer.Business.Models
{
    public class CompanySymbolInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long? Id { get; set; }
        [JsonProperty("refId")]
        [DataConvert("RefId")]
        public long? RefId { get; set; }
        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [JsonProperty("companyIdParent")]
        public long? CompanyIdParent { get; set; }
        [JsonProperty("symbol")]
        [DataConvert("Symbol")]
        public string Symbol { get; set; }

        [JsonProperty("name")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [JsonProperty("taxCode")]
        public string Taxcode { get; set; }

        [JsonProperty("branchCode")]
        public string BranchCode { get; set; }

        public CompanySymbolInfo()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public CompanySymbolInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, CompanySymbolInfo>(srcObject, this);
            }
        }
    }
}
