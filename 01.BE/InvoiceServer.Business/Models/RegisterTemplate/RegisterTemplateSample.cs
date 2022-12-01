using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class RegisterTemplateSample
    {
        [StringTrimAttribute]
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("sampleName")]
        [DataConvert("InvoiceSample.Name")]
        public string SampleName { get; set; }
       

        public RegisterTemplateSample()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public RegisterTemplateSample(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RegisterTemplateSample>(srcObject, this);
            }
        }
    }
}
