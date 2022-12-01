using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceSampleInfoEdit
    {
        [JsonProperty("id")]
        [DataConvert("InvoiceSample.Id")]
        public long Id { get; set; }

        [JsonProperty("code")]
        [DataConvert("InvoiceSample.Code", ThrowExceptionIfSourceNotExist = false)]
        public string Code { get; set; }

        [JsonProperty("name")]
        [DataConvert("InvoiceSample.Name", ThrowExceptionIfSourceNotExist = false)]
        public string Name { get; set; }


        public InvoiceSampleInfoEdit()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceSampleInfoEdit(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceSampleInfoEdit>(srcObject, this);
            }
        }


    }
}
