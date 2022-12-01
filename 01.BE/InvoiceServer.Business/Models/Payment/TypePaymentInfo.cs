using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class TypePaymentInfo
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("Code")]
        public string Code { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        [DataConvert("Description")]
        public string Description { get; set; }

        public TypePaymentInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public TypePaymentInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, TypePaymentInfo>(srcObject, this);
            }
        }
    }
}
