using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class FileSystemLogInfo
    {
        [StringTrimAttribute]
        [JsonProperty("fileName")]
        [DataConvert("Name")]
        public string fileName { get; set; }

        [JsonProperty("modifyDate")]
        [DataConvert("LastWriteTime")]
        public System.DateTime? modifyDate { get; set; }

        [JsonProperty("fileSize")]
        [DataConvert("Length")]
        public long fileSize { get; set; }

        public FileSystemLogInfo()
        {

        }
        public FileSystemLogInfo(object srcObject)
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, FileSystemLogInfo>(srcObject, this);
            }
        }
    }

}
