using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class InvoiceReleasesTemplateInfo
    {

        [StringTrimAttribute]
        [JsonProperty("note")]
        [DataConvert("Note")]
        public string Note { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note1")]
        [DataConvert("Note1")]
        public string Note1 { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note2")]
        [DataConvert("Note2")]
        public string Note2 { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note3")]
        [DataConvert("Note3")]
        public string Note3 { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note4")]
        [DataConvert("Note4")]
        public string Note4 { get; set; }

        [StringTrimAttribute]
        [JsonProperty("note5")]
        [DataConvert("Note5")]
        public string Note5 { get; set; }


        public InvoiceReleasesTemplateInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public InvoiceReleasesTemplateInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, InvoiceReleasesTemplateInfo>(srcObject, this);
            }
        }
    }
}
