using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReportListInvoices
    {
        [JsonProperty("isCategoryTax")]
        public bool IsCategoryTax { get; set; }

        [JsonProperty("isSummary")]
        public bool IsSummary { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("invoiceName")]
        public string InvoiceName { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("invoiceCode")]
        public string InvoiceCode { get; set; }

        [JsonProperty("invoiceSymbol")]
        public string InvoiceSymbol { get; set; }

        [JsonProperty("invoiceNo")]
        public string InvoiceNo { get; set; }

        public decimal? InvoiceNo2 { get; set; }

        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }
        [JsonProperty("total")]
        public decimal? Total { get; set; }

        [JsonProperty("totalTax")]
        public decimal? TotalTax { get; set; }

        [JsonProperty("taxtId")]
        public long? TaxId { get; set; }

        [JsonProperty("invoiceNote")]
        public string InvoiceNote { get; set; }

        [JsonProperty("totalByTax")]
        public decimal? TotalByTax { get; set; }

        [JsonProperty("totalTaxByTax")]
        public decimal? TotalTaxByTax { get; set; }

        [JsonProperty("refNumber")]
        public string RefNumber { get; set; }

        [JsonProperty("adjustmentType")]
        public int? AdjustmentType { get; set; }

        [JsonIgnore]
        public decimal CurrencyExchangeRate { get; set; }

        [JsonIgnore]
        public string CurrencyCode { get; set; }

        [JsonIgnore]
        public decimal? InvoiceTotalTax { get; set; }

        [JsonIgnore]
        public int? InvoiceType { get; set; }

        [JsonIgnore]
        public long? ParentId { get; set; }

        public ReportListInvoices()
        {
        }

        public ReportListInvoices(string invoiceName, bool isCategoryTax = true)
        {
            this.InvoiceName = invoiceName;
            this.IsCategoryTax = isCategoryTax;

        }

        public ReportListInvoices(decimal totalByTax, decimal totalTaxByTax, string taxCode, bool isCategoryTax = true)
        {
            this.Code = taxCode;
            this.TotalByTax = totalByTax;
            this.IsSummary = isCategoryTax;
            this.TotalTaxByTax = totalTaxByTax;
        }


    }
}
