using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class EmailActiveInfo
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("title")]
        [DataConvert("Title")]
        public string Title { get; set; }

        [StringTrimAttribute]
        [JsonProperty("emailTo")]
        [DataConvert("EmailTo")]
        public string EmailTo { get; set; }

        [StringTrimAttribute]
        [JsonProperty("content")]
        [DataConvert("ContentEmail")]
        public string ContentEmail { get; set; }

        [StringTrimAttribute]
        [JsonProperty("createdDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        [DataConvert("CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("sendtedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        [DataConvert("SendtedDate")]
        public DateTime? SendtedDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("status")]
        [DataConvert("SendStatus")]
        public int SendStatus { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceNo")]
        public string InvoiceNo { get; set; }

        public decimal No { get; set; }

        [StringTrimAttribute]
        [JsonProperty("dateInvoice")]
        public string DateInvoice { get; set; }
        [JsonProperty("releaseDate")]
        public DateTime? ReleaseDate { get; set; }
        [StringTrimAttribute]
        [JsonProperty("teller")]
        public string Teller { get; set; }

        [JsonIgnore]
        [DataConvert("TypeEmail")]
        public int TypeEmail { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("customerCode")]
        public string CustomerCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        public EmailActiveInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public EmailActiveInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, EmailActiveInfo>(srcObject, this);
            }
        }
    }
}
