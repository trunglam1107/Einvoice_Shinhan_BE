using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceItem
    {
        [JsonProperty("Id")]
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        [JsonProperty("firstRow")]
        public bool FirstRow { get; set; }
        [JsonProperty("productCode")]
        [DataConvert("Product.ProductCode", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string ProductCode { get; set; }
        [JsonProperty("productName")]
        [DataConvert("ProductName", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string ProductName { get; set; }

        [DataConvert("PRODUCTNAME")]
        public string ProductNameInvoice { get; set; }

        [DataConvert("UnitName", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string Unit { get; set; }

        [DataConvert("Quantity")]
        public decimal? Quantity { get; set; }

        [DataConvert("Price")]
        public decimal? Price { get; set; }

        [DataConvert("Tax.Tax1", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public long Tax { get; set; }
        [DataConvert("TaxId")]
        public long TaxId { get; set; }

        [DataConvert("Tax.DisplayInvoice", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string DisplayInvoice { get; set; }

        [DataConvert("Tax.Name", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string TaxName { get; set; }

        [DataConvert("Tax.Code", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string TaxCode { get; set; }

        [DataConvert("Total")]
        public decimal? Total { get; set; }

        [DataConvert("Discount")]
        public bool? Discount { get; set; }

        [DataConvert("DiscountRatio")]
        public int? DiscountRatio { get; set; }

        [DataConvert("AmountDiscount")]
        public decimal? AmountDiscount { get; set; }

        [DataConvert("AmountTax")]
        public decimal? AmountTax { get; set; }
        [DataConvert("Sum")]
        public decimal? Sum { get; set; }
        public bool IsShowOrder { get; set; }

        [DataConvert("DiscountDescription", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string DiscountDescription { get; set; }

        [DataConvert("AdjustmentType", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public int? AdjustmentType { get; set; }

        public string Currency { get; set; }

        public decimal AmountTotal
        {
            get
            {
                return ((this.Total ?? 0) + (this.AmountTax ?? 0) - (this.AmountDiscount ?? 0));
            }
        }

        public InvoiceItem()
        {
            this.IsShowOrder = true;
        }

        public InvoiceItem(bool isShowOrder)//bool isShowOrder = false
            : this()
        {
            this.IsShowOrder = isShowOrder;
        }


        public InvoiceItem(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceItem>(srcObject, this);
            }
        }
        public InvoiceItem(InvoiceDetail invoiceDetail)
            : this()
        {
            this.Id = Int64.Parse(invoiceDetail.Id.ToString());
            this.InvoiceId = Int64.Parse(invoiceDetail.InvoiceId.ToString());
            this.ProductName = invoiceDetail.ProductName;
            this.Quantity = invoiceDetail.Quantity;
            this.Price = invoiceDetail.Price;
            this.TaxId = Int64.Parse(invoiceDetail.TaxId.ToString());
            this.AmountTax = invoiceDetail.AmountTax;
            this.TaxCode = invoiceDetail.TaxCode;
            this.TaxName = invoiceDetail.TaxName;
            this.Total = invoiceDetail.Total;
            this.Sum = invoiceDetail.Sum;
            this.DisplayInvoice = invoiceDetail.DisplayInvoice;
        }
    }
}
