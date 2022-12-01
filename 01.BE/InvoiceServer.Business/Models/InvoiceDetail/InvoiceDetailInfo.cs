using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace InvoiceServer.Business.Models
{
    [XmlRoot("details")]
    public class InvoiceDetailInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }// 

        [JsonProperty("productId")]
        [DataConvert("ProductId")]
        public long? ProductId { get; set; }

        [JsonProperty("productName")]
        [DataConvert("ProductName", DefaultValue = null)]
        [XmlElement("productName")]
        public string ProductName { get; set; }// Tên sản phẩm 

        [JsonProperty("unitId")]
        [DataConvert("UnitId", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public int UnitId { get; set; }// đơn vị tính// đơn vị tính

        [JsonProperty("unitName")]
        [DataConvert("UnitName", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string UnitName { get; set; }// tên đơn vị tính

        [JsonProperty("quantity")]
        [DataConvert("Quantity")]
        [XmlElement("quantity")]
        public decimal? Quantity { get; set; }// Số lượng

        [JsonProperty("price")]
        [DataConvert("Price")]
        [XmlElement("price")]
        public decimal? Price { get; set; }// Số lượng

        [JsonProperty("taxId")]
        [DataConvert("TaxId")]
        public long? TaxId { get; set; }// thuế xuất

        [JsonProperty("amountTax")]
        [DataConvert("AmountTax")]
        public decimal? AmountTax { get; set; }// thành tiền

        [JsonProperty("total")]
        [DataConvert("Total")]
        public decimal? Total { get; set; }// thành tiền

        [JsonProperty("sum")]
        [DataConvert("Sum")]
        public decimal? Sum { get; set; }// thành tiền
        [JsonProperty("discount")]
        [DataConvert("Discount")]
        public bool? Discount { get; set; }//chiết khấu

        [JsonProperty("discountRatio")]
        [DataConvert("DiscountRatio")]
        public int? DiscountRatio { get; set; }// Tiền chiết khấu

        [JsonProperty("amountDiscount")]
        [DataConvert("AmountDiscount")]
        public decimal? AmountDiscount { get; set; }// số tiền chiết khấu

        [JsonProperty("adjustmentType")]
        [DataConvert("AdjustmentType")]
        [XmlElement("adjustmentType")]
        public int? AdjustmentType { get; set; }// điều chỉnh

        [JsonProperty("discountDescription")]
        [DataConvert("DiscountDescription")]
        public string DiscountDescription { get; set; }// điều chỉnh

        [JsonProperty("refNumber")]
        //[DataConvert("RefNumber")]
        public string RefNumber { get; set; }

        public InvoiceDetailInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceDetailInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceDetailInfo>(srcObject, this);
            }
        }
    }
}
