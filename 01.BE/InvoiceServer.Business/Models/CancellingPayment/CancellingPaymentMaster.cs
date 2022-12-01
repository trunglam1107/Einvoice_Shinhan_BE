using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class CancellingPaymentMaster
    {
        [JsonProperty("dateCancelling")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime CancellingDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("items")]
        public List<CancellingPaymentInfo> CancellingPayment { get; set; }

        [JsonIgnore()]
        public long CompanyId { get; set; }

        [JsonIgnore()]
        public long ActionUserId { get; set; }

        public CancellingPaymentMaster()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public CancellingPaymentMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, CancellingPaymentMaster>(srcObject, this);
            }
        }
    }
}
