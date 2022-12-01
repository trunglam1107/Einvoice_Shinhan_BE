using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceTemplateSampleName
    {
        [JsonProperty("templateSampleName")]
        public string TemplateSampleName { get; set; }

        [JsonProperty("templateSampleFileName")]
        public string TemplateSampleFileName { get; set; }

        public InvoiceTemplateSampleName()
        {
        }

        /// <summary>
        /// Constructor current InvoiceTemplateSample object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceTemplateSampleName(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceTemplateSampleName>(srcObject, this);
            }
        }
    }
}
