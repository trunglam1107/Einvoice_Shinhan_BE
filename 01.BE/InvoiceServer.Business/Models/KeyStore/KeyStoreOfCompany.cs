using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class KeyStoreOfCompany
    {
        [JsonProperty("serialNumber")]
        [DataConvert("SerialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long CompanyId { get; set; }

        [JsonProperty("type")]
        [DataConvert("Type")]
        public string Type { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("fromDate")]
        public DateTime FromDate { get; set; }

        [JsonProperty("toDate")]
        public DateTime ToDate { get; set; }

        public KeyStoreOfCompany()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public KeyStoreOfCompany(object srcObject)
          : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, KeyStoreOfCompany>(srcObject, this);
            }
        }
    }
}
