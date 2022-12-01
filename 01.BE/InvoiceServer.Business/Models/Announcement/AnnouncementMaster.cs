using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class AnnouncementMaster
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("denominator")]
        public string Denominator { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("invoiceNo")]
        public string InvoiceNo { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("clientTaxCode")]
        public string ClientTaxCode { get; set; }

        [JsonProperty("invoiceId")]
        public long InvoiceId { get; set; }

        [JsonProperty("invoiceDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? InvoiceDate { get; set; }
        public DateTime? Updated { get; set; }

        [JsonProperty("announcementStatus")]
        public int? AnnouncementStatus { get; set; }

        [JsonProperty("announcementType")]
        public int? AnnouncementType { get; set; }

        [JsonProperty("announcementDate")]
        public DateTime? AnnouncementDate { get; set; }

        [JsonProperty("isClientSign")]
        public bool IsClientSign { get; set; }

        [JsonProperty("isCompanySign")]
        public bool IsCompanySign { get; set; }

        [JsonProperty("companyId")]
        public long CompanyId { get; set; }

        [JsonProperty("customerCode")]
        public string CustomerCode { get; set; }

        [JsonProperty("minutesNo")]
        public string MinutesNo { get; set; }
        public string InvoiceNo2 { get; set; }

        [JsonProperty("BranchId")]
        public string BranchId { get; set; }
        public AnnouncementMaster()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public AnnouncementMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, AnnouncementMaster>(srcObject, this);
            }
        }
    }
}
