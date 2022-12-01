using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class NotificeUseInvoiceMaster
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("MyCompany.CompanyName")]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxDepartment")]
        [DataConvert("TaxDepartment.Name", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string TaxDepartment { get; set; }

        [StringTrimAttribute]
        [JsonProperty("createdDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        [DataConvert("CreatedDate")]
        public string CreatedDate { get; set; }


        [StringTrimAttribute]
        [JsonProperty("status")]
        [DataConvert("Status")]
        public int Status { get; set; }

        public NotificeUseInvoiceMaster()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public NotificeUseInvoiceMaster(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, NotificeUseInvoiceMaster>(srcObject, this);
            }
        }
    }
}
