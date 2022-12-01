using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ProductInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyId")]
        //[DataConvert("CompanyId")]
        public long CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("productCode")]
        [DataConvert("ProductCode")]
        public string ProductCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("productName")]
        [DataConvert("ProductName")]
        public string ProductName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("price")]
        [DataConvert("Price")]
        public decimal? Price { get; set; }

        [StringTrimAttribute]
        [JsonProperty("unit")]
        [DataConvert("Unit")]
        public string Unit { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        [DataConvert("Description")]
        public string Description { get; set; }


        [StringTrimAttribute]
        [JsonProperty("taxId")]
        [DataConvert("TaxId")]
        public long TaxId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxName")]
        [DataConvert("Tax.Name", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string TaxName { get; set; }

        [JsonProperty("unitId")]
        [DataConvert("UnitId")]
        public long UnitId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("unitName")]
        [DataConvert("UnitList.Name", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string UnitName { get; set; }

        [JsonIgnore()]
        public long UserAction { get; set; }

        public ProductInfo(object srcObject)
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ProductInfo>(srcObject, this);
            }
        }
    }

}
