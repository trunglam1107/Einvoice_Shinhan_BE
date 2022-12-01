using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class HoldInvoiceViewModel
    {
        [JsonProperty("id")]
        [DataConvert("ID")]
        public long Id { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("COMPANYID")]
        public long CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("CODE")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("registerTemplateId")]
        [DataConvert("REGISTERTEMPLATEID")]
        public long RegisterTemplateId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("symbol")]
        [DataConvert("SYMBOL")]
        public string Symbol { get; set; }

        [StringTrimAttribute]
        [JsonProperty("invoiceDate")]
        [DataConvert("INVOICEDATE")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime InvoiceDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("quantity")]
        [DataConvert("QUANTITY")]
        public decimal Quantity { get; set; }

        [StringTrimAttribute]
        [JsonProperty("numberFrom")]
        [DataConvert("NUMBERFROM")]
        public string NumberFrom { get; set; }

        [StringTrimAttribute]
        [JsonProperty("numberTo")]
        [DataConvert("NUMBERTO")]
        public string NumberTo { get; set; }

        [JsonProperty("holdInvoiceDetails")]
        public List<HoldInvoiceDetailViewModel> HoldInvoiceDetails { get; set; }

        public HoldInvoiceViewModel()
        {
            this.HoldInvoiceDetails = new List<HoldInvoiceDetailViewModel>();
        }

        public HoldInvoiceViewModel(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, HoldInvoiceViewModel>(srcObject, this);
            }
        }
    }
}
