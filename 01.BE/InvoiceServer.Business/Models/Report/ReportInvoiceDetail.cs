using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReportInvoiceDetail
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("customerCode")]
        public string CustomerCode { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("personContact")]
        public string PersonContact { get; set; }

        [JsonProperty("createdDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? Created { get; set; }

        [JsonProperty("invoiceCode")]
        public string InvoiceCode { get; set; }

        [JsonProperty("invoiceSymbol")]
        public string InvoiceSymbol { get; set; }

        [JsonProperty("invoiceNo")]
        public string InvoiceNo { get; set; }

        [JsonProperty("productCode")]
        public string ProductCode { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("quantity")]
        public decimal? Quantity { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("taxName")]
        public string TaxName { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("tax")]
        public decimal Tax { get; set; }
        [JsonProperty("taxId")]
        public long TaxId { get; set; }
        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }
        public bool? IsMultiTax { get; set; }
        [JsonProperty("taxAmount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("total")]
        public decimal? Total { get; set; }

        [JsonProperty("sum")]
        public decimal? Sum { get; set; }

        [JsonProperty("totalDiscount")]
        public decimal? TotalDiscount { get; set; }

        [JsonProperty("totalDiscountTax")]
        public decimal? TotalDiscountTax { get; set; }

        [JsonProperty("unitPriceAfterTax")]
        public decimal? UnitPriceAfterTax { get; set; }
        [JsonProperty("invoiceType")]
        public int? InvoiceType { get; set; }

        [JsonProperty("totalTax")]
        public decimal? TotalTax { get; set; }
        [JsonProperty("sumAmountInvoice")]
        public decimal? SumAmountInvoice { get; set; }
        public decimal? BSLThu { get; set; }

        public string Report_class { get; set; }
        public string Ltbao { get; set; }

        public long? CompanyId { get; set; }

        [JsonProperty("bthErrorStatus")]
        public decimal? BTHERRORSTATUS { get; set; }
        [JsonProperty("bthError")]
        public string BTHERROR { get; set; }
        public string MltDiep { get; set; }
        [JsonProperty("lTBao")]
        public string LtBao { get; set; }
        public ReportInvoiceDetail()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public ReportInvoiceDetail(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ReportInvoiceDetail>(srcObject, this);
            }
        }

        [JsonProperty("typePayment")]
        public long TypePayment { get; set; }

        [JsonProperty("invoiceStatus")]
        public int InvoiceStatus { get; set; }

        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("productId")]
        public long? ProductId { get; set; }

        [JsonProperty("discount")]
        public bool Discount { get; set; }

        [JsonProperty("discountDescription")]
        public string DiscountDescription { get; set; }

        [JsonProperty("amountTax")]
        public decimal AmountTax { get; set; }

        [JsonProperty("currencyExchangeRate")]
        public decimal CurrencyExchangeRate { get; set; }

        [JsonProperty("isOrg")]
        public bool IsOrg { get; set; }

        [JsonProperty("parentSymbol")]
        public string ParentSymbol { get; set; }

        [JsonProperty("parentNo")]
        public string ParentNo { get; set; }

        [JsonProperty("parentCode")]
        public string ParentCode { get; set; }

        public int? SendCQT { get; set; }

        public string MessageCode { get; set; }

        public decimal? InvoiceNo2 { get; set; }

        public long? InvoiceDetailId { get; set; }
    }
}
