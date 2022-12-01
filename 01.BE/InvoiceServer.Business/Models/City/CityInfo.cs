using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class CityInfo
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

        public CityInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public CityInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, CityInfo>(srcObject, this);
            }
        }
    }
}
