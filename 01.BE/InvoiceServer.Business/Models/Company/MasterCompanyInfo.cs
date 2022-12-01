using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class MasterCompanyInfo
    {
        [JsonProperty("id")]
        [DataConvert("CompanySID")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [JsonProperty("tax")]
        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }

        [StringTrimAttribute]
        [JsonProperty("personContact")]
        [DataConvert("PersonContact")]
        public string PersonContact { get; set; }

        [StringTrimAttribute]
        [JsonProperty("delegate")]
        [DataConvert("Delegate")]
        public string Delegate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("website")]
        [DataConvert("WebSite")]
        public string WebSite { get; set; }

        [StringTrimAttribute]
        [JsonProperty("tel")]
        [DataConvert("Tel1")]
        public string Tel1 { get; set; }

        public MasterCompanyInfo()
        {

        }

        /// <summary>
        /// Constructor current Company object by copying data in the specified object
        /// </summary>
        /// <param name="srcObject">Source object</param>
        public MasterCompanyInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, MasterCompanyInfo>(srcObject, this);
            }
        }
    }
}
