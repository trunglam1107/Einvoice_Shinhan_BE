using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models.InvoiceSample_Type
{
    public class CurrencyListViewModel
    {
        [JsonProperty("id")]
        [DataConvert("ID")]
        public long Id { get; set; }

        [JsonProperty("companyId")]
        //
        public long CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("CODE")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("NAME")]
        public string Name { get; set; }

        [JsonProperty("exchangerate")]
        [DataConvert("EXCHANGERATE")]
        public decimal? Exchangerate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("decimalSeparator")]
        [DataConvert("DECIMALSEPARATOR")]
        public string DecimalSeparator { get; set; }

        [StringTrimAttribute]
        [JsonProperty("decimalUnit")]
        [DataConvert("DECIMALUNIT")]
        public string DecimalUnit { get; set; }
    }
}
