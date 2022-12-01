using InvoiceServer.Business.Utils;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceMaster
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("code")]
        public string InvoiceCode { get; set; }

        [JsonProperty("invoiceName")]
        public string InvoiceName { get; set; }

        [JsonProperty("symbol")]
        public string InvoiceSymbol { get; set; }

        [JsonProperty("no")]
        public string No { get; set; }
        [JsonProperty("invoiceNo")]
        public decimal InvoiceNo { get; set; }

        [JsonProperty("isOrg")]
        public bool? IsOrg { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("customerCode")]
        public string CustomerCode { get; set; }

        [JsonProperty("personContact")]
        public string PersonContact { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("taxName")]
        public string TaxName { get; set; }

        [JsonProperty("numberAccount")]
        public string NumberAccount { get; set; }

        [JsonProperty("dateInvoice")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? InvoiceDate { get; set; }

        [JsonProperty("dateRelease")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; }
        public DateTime? Updated { get; set; }

        [JsonProperty("status")]
        public int? InvoiceStatus { get; set; }

        [JsonProperty("released")]
        public bool? Released { get; set; }

        [JsonProperty("note")]
        public string InvoiceNote { get; set; }

        [JsonProperty("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("isClientSign")]
        public bool IsClientSign { get; set; }

        [JsonProperty("converted")]
        public bool? Converted { get; set; }

        [JsonProperty("Sum")]
        public decimal? Sum { get; set; }

        [JsonProperty("total")]
        public decimal? Total { get; set; }

        [JsonProperty("taxTotal")]
        public decimal? TotalTax { get; set; }

        [JsonProperty("companyId")]
        public long CompanyId { get; set; }

        [JsonProperty("statisticalId")]
        public long StatisticalId { get; set; }

        [JsonProperty("canCreateAnnoun")]
        public bool CanCreateAnnoun { get; set; }

        [JsonProperty("typeAnnoun")]
        public int TypeAnnoun { get; set; }

        [JsonProperty("invoiceType")]
        public int? InvoiceType { get; set; }

        [JsonProperty("refNumber")]
        public string RefNumber { get; set; }

        [JsonIgnore]
        public long? ParentId { get; set; }

        [JsonProperty("childId")]
        public long? ChildId { get; set; }

        [JsonProperty("teller")]
        public string Teller { get; set; }

        [JsonProperty("haveAdjustment")]
        public bool HaveAdjustment { get; set; }

        [JsonProperty("isChange")]
        public bool IsChange { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("fileReceipt")]
        public string fileReceipt { get; set; }
        [JsonProperty("HTHDon")]
        public Nullable<int> HTHDon { get; set; }

        [JsonProperty("codeCQT")]
        public string codeCQT { get; set; }

        [JsonProperty("messageCode")]
        public string MessageCode { get; set; }

        [JsonProperty("pThuc")]
        public int? pThuc { get; set; }
        [JsonProperty("messageError")]
        public string MessageError { get; set; }
        [JsonProperty("statusCQT")]
        public int? statusCQT { get; set; }

        [JsonProperty("isSentMail")]
        public int? IsSentMail { get; set; }

        [JsonProperty("statusNotificationCancel")]
        public int? statusNotificationCancel { get; set; }

        [JsonProperty("statusNotificationDelete")]
        public int? statusNotificationDelete { get; set; }

        [JsonProperty("statusNotificationAdjustment")]
        public int? statusNotificationAdjustment { get; set; }
        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("vat_invoice_type")]
        public string Vat_invoice_type { get; set; }
        public int CountRecord { get; set; }
        [JsonProperty("productName")]
        public string ProductName { get; set; }
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("companyTaxCode")]
        public string CompanyTaxCode { get; set; }

        [JsonProperty("companyAddress")]
        public string CompanyAddress { get; set; }

        [JsonProperty("companyDelegate")]
        public string CompanyDelegate { get; set; }

        [JsonProperty("companyTel")]
        public string CompanyTel { get; set; }

        [JsonProperty("companyPosition")]
        public string CompanyPosition { get; set; }
        
        [JsonProperty("customerAddress")]
        public string CustomerAddress { get; set; }

        [JsonProperty("customerTaxCode")]
        public string CustomerTaxCode { get; set; }
        [JsonProperty("customerDelegate")]
        public string CustomerDelegate { get; set; }

        [JsonProperty("customerMobile")]
        public string CustomerMobile { get; set; }

        [JsonProperty("btherror")]
        public string Btherror { get; set; }

        [JsonProperty("btherrorstatus")]
        public int? Btherrorstatus { get; set; }

        public InvoiceMaster()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceMaster>(srcObject, this);
            }
        }
    }
}
