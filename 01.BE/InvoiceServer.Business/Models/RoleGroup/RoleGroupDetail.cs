using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class RoleGroupDetail
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        public RoleGroupDetail()
        {

        }

        public RoleGroupDetail(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RoleGroupDetail>(srcObject, this);
            }
        }
    }
}
