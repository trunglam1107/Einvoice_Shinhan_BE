using Newtonsoft.Json;
using System.Web;

namespace InvoiceServer.Business.Models
{
    public class FileInfomation
    {
        [JsonIgnore]
        public string FileType { get; set; }

        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        public FileInfomation SetFullPathFile()
        {
            this.FilePath = this.FilePath.Replace(HttpContext.Current.Server.MapPath("~"), string.Empty);
            return this;
        }
    }
}
