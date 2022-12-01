using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class DataPermissionInfo
    {
        [JsonProperty("id")]
        [DataConvert("SID")]
        public long Id { get; set; }

        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        [DataConvert("UrlFunction")]
        public string UrlFunction { get; set; }

        public DataPermissionInfo()
        {

        }

        public DataPermissionInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, DataPermissionInfo>(srcObject, this);
            }
        }
    }
}
