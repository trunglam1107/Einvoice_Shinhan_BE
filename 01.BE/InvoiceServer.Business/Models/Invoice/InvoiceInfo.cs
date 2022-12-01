using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace InvoiceServer.Business.Models
{
    [XmlRoot("Invoice")]
    public class InvoiceInfo
    {
        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [DataConvert("COMPANYNAME")]
        public string CompanyNameInvoice { get; set; }

        [DataConvert("COMPANYADDRESS")]
        public string CompanyAddress { get; set; }

        [DataConvert("COMPANYTAXCODE")]
        public string CompanyTaxCode { get; set; }
        public string Delegate { get; set; }

        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("invoiceTemplateId")]
        [DataConvert("RegisterTemplateId")]
        public long RegisterTemplateId { get; set; }// Mẫu số 

        [JsonProperty("customerId")]
        [DataConvert("ClientId")]
        public long? ClientId { get; set; }// Id Khách Hàng 

        [JsonProperty("personContact")]
        [DataConvert("PersonContact")]
        public string PersonContact { get; set; }// Id Khách Hàng 


        [XmlElement("customerCode")]
        [JsonProperty("customerCode")]
        public string CustomerCode { get; set; }

        [JsonProperty("symbol")]
        [DataConvert("Symbol")]
        [XmlElement("invoiceSymbol")]
        public string Symbol { get; set; } // Ký Hiệu 

        [XmlElement("invoiceTemplateCode")]
        public string InvoiceTemplateCode { get; set; }

        [JsonProperty("parentSymbol")]
        [DataConvert("ParentSymbol")]
        public string ParentSymbol { get; set; } // Ký Hiệu cha

        [JsonProperty("originNo")]
        [DataConvert("OriginNo")]
        public string OriginNo { get; set; }//Số [OriginNo]

        [JsonProperty("no")]
        [DataConvert("No")]
        [XmlElement("no")]
        public string No { get; set; }//Số [No]

        [JsonProperty("customerName")]
        [DataConvert("Client.Personcontact", DefaultValue = null)]
        //[DataConvert("CustomerName", DefaultValue = null)]
        [XmlElement("buyerName")]
        public string CustomerName { get; set; }//Họ tên khách hàng

        [XmlElement("sellerTaxCode")]
        public string SellerTaxCode { get; set; }// Mã số thuế

        [JsonProperty("companyName")]
        [DataConvert("Client.CustomerName", DefaultValue = null)]
        public string CompanyName { get; set; }//Tên công ty
        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("isOrg")]
        [DataConvert("Client.IsOrg", DefaultValue = null)]
        [XmlElement("isOrg")]
        public bool IsOrg { get; set; }//là Tổ chứ

        [JsonProperty("taxCode")]
        [DataConvert("Client.TaxCode", DefaultValue = null)]
        [XmlElement("buyerTaxCode")]
        public string TaxCode { get; set; }// Mã số thuế

        [JsonProperty("address")]
        [DataConvert("Client.Address", DefaultValue = null)]
        [XmlElement("buyerAddress")]
        public string Address { get; set; } //ĐỊa chỉ

        [JsonProperty("mobile")]
        [DataConvert("Client.Mobile", DefaultValue = null)]
        public string Mobile { get; set; }//Số điện thoại khách hàng

        [JsonProperty("email")]
        [DataConvert("Client.ReceivedInvoiceEmail", DefaultValue = null)]
        [XmlElement("buyerEmail")]
        public string Email { get; set; }// Mã số thuế

        [JsonProperty("invoiceCode")]
        [DataConvert("Code")]
        public string InvoiceCode { get; set; }//Mã Số

        [JsonProperty("parentCode")]
        [DataConvert("ParentCode")]
        public string ParentCode { get; set; }//Mã Số cha

        [JsonProperty("parentReleaseDate")]
        public DateTime? ParentReleaseDate { get; set; }//Ngày ký cha

        [JsonProperty("parentId")]
        [DataConvert("ParentId")]
        public string ParentId { get; set; }//Mã Số cha

        [JsonProperty("typePayment")]
        [DataConvert("TypePayment")]
        public int TypePayment { get; set; }//Hình thức thanh toán

        [JsonProperty("vatInvoiceType")]
        public string VatInvoiceType { get; set; }//Loại hóa đơn

        [JsonProperty("note")]
        [DataConvert("Note")]
        public string Note { get; set; } // Ghi chú

        [JsonProperty("taxId")]
        //[DataConvert("TaxId")]
        public long? TaxId { get; set; } // Phần trăm thuế

        [JsonProperty("taxTotal")]
        [DataConvert("TotalTax")]
        public decimal? TotalTax { get; set; } // Tổng tiền thuế

        [JsonProperty("taxTotal5")]
        public decimal? TotalTax5 { get; set; }// tổng tiền thuế 5%
        [JsonProperty("taxTotal10")]
        public decimal? TotalTax10 { get; set; }// tổng tiền thuế 10%

        [JsonProperty("total")]
        [DataConvert("Total")]
        public decimal? Total { get; set; } // Tổng tiền thuế

        [JsonProperty("isDiscount")]
        public int? IsDiscount { get; set; }

        [JsonProperty("totalDiscount")]
        [DataConvert("TotalDiscount")]
        public decimal? TotalDiscount { get; set; } // Tổng tiền chiết khấu

        [JsonProperty("totalDiscountTax")]
        [DataConvert("TotalDiscountTax")]
        public decimal? TotalDiscountTax { get; set; }// Phần trăm chiết khấu

        [JsonProperty("sum")]
        [DataConvert("Sum")]
        [XmlElement("sum")]
        public decimal? Sum { get; set; } // Tổng tiền thuế

        [JsonProperty("invoiceDate")]
        [DataConvert("CreatedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? Created { get; set; } // Ngày hóa đơn

        [JsonProperty("dateRelease")]
        [DataConvert("ReleasedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ReleasedDate { get; set; } // Ngày ky

        [JsonIgnore]
        public long UserAction { get; set; }

        [JsonIgnore]
        public long EmailActiveId { get; set; }

        [JsonProperty("amountInWords")]
        public string AmountInWords { get; set; }

        public string FolderPath { get; set; }

        [JsonProperty("invoiceSample")]
        [DataConvert("InvoiceType")]
        public int? InvoiceType { get; set; } // Loại hóa đơn

        [JsonProperty("fileReceipt")]
        [DataConvert("FileReceipt")]
        public string FileReceipt { get; set; }

        [JsonProperty("items")]
        [XmlElement("details")]
        public List<InvoiceDetailInfo> InvoiceDetail { get; set; }

        [JsonProperty("statisticals")]
        public List<InvoiceStatistical> InvoiceStatistical { get; set; }

        [JsonProperty("gifts")]
        public List<InvoiceGift> InvoiceGift { get; set; }

        [JsonProperty("bankAccount")]
        [DataConvert("Client.BankAccount")]
        public string BankAccount { get; set; }

        [JsonProperty("customerBankAccount")]
        [DataConvert("CustomerBankAcc")]
        public string CustomerBankAccount { get; set; }

        [JsonProperty("bankName")]
        [DataConvert("Client.BankName")]
        public string BankName { get; set; }

        [JsonProperty("currencyId")]
        [DataConvert("CurrencyId")]
        public long? CurrencyId { get; set; }

        [XmlElement("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("exchangeRate")]
        [DataConvert("CURRENCYEXCHANGERATE")]
        public decimal ExchangeRate { get; set; }

        [JsonProperty("statisticalSumAmount")]
        public decimal? StatisticalSumAmount { get; set; }

        [JsonProperty("statisticalSumTaxAmount")]
        public decimal? StatisticalSumTaxAmount { get; set; }


        [JsonProperty("giftSumAmount")]
        public decimal? GiftSumAmount { get; set; }

        [JsonProperty("giftSumTaxAmount")]
        public decimal? GiftSumTaxAmount { get; set; }

        [JsonProperty("refNumber")]
        [DataConvert("refNumber")]
        public string RefNumber { get; set; }

        [JsonProperty("parentTemplateId")]
        public long? OldTemplateId { get; set; }
        [JsonProperty("declareId")]
        [DataConvert("DeclareId")]
        public long? DeclareId { get; set; }

        [JsonProperty("HTHDon")]
        [DataConvert("HTHDon")]
        public int? HTHDon { get; set; }

        [JsonProperty("invoiceStatus")]
        [DataConvert("INVOICESTATUS")]
        public int? InvoiceStatus { get; set; } // Trạng thái hóa đơn

        public string Report_Class { get; set; }

        public string Vat_invoice_type { get; set; }

        public string Fee_ben_our { get; set; }

        public string Cif_no { get; set; }
        public InvoiceInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceInfo>(srcObject, this);
                this.ConverForCurrentCustomerIndex5(srcObject);
            }
        }

        public InvoiceInfo(object srcObject, List<InvoiceDetailInfo> invoiceDetails)
            : this(srcObject)
        {
            this.InvoiceDetail = invoiceDetails;
        }
        public InvoiceInfo(object srcObject, List<InvoiceDetailInfo> invoiceDetails, List<InvoiceStatistical> invoiceStatistical, List<InvoiceGift> invoiceGifts)
            : this(srcObject)
        {
            this.InvoiceDetail = invoiceDetails;
            this.InvoiceStatistical = invoiceStatistical;
            this.InvoiceGift = invoiceGifts;
        }

        private void ConverForCurrentCustomerIndex5(object srcObject)
        {
            try
            {
                var invoice = srcObject as INVOICE;
                if (OtherExtensions.IsCurrentClient(invoice.CLIENT.CUSTOMERCODE))
                {
                    CustomerName = invoice.PERSONCONTACT;
                    CompanyName = invoice.CUSTOMERNAME;
                    TaxCode = invoice.CUSTOMERTAXCODE;
                    Address = invoice.CUSTOMERADDRESS;
                    BankAccount = invoice.CUSTOMERBANKACC;
                    IsOrg = invoice.CUSTOMERTAXCODE != null;
                }
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.Trace("InvoiceInfo.ConverForCurrentCustomerIndex5(object srcObject)", ex);
                throw;
            }
        }
    }
}
