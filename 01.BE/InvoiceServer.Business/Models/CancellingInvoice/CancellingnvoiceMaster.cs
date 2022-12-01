using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class CancellingInvoiceMaster
    {
        [JsonProperty("invoiceId")]
        public long InvoiceId { get; set; }

        [JsonProperty("dateCancelling")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime CancellingDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fileReceipt")]
        [DataConvert("FileReceipt")]
        public string FileReceipt { get; set; }

        public string FolderPath { get; set; }

        [JsonIgnore()]
        public long CompanyId { get; set; }

        [JsonIgnore()]
        public long ActionUserId { get; set; }

        [JsonProperty("items")]
        public List<ReleaseInvoiceInfo> ReleaseInvoiceInfos { get; set; }


        public CancellingInvoiceMaster()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public CancellingInvoiceMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, CancellingInvoiceMaster>(srcObject, this);
            }
        }
    }
}
