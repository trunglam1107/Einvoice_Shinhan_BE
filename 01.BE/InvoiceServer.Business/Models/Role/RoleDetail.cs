using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class RoleDetail
    {
        [JsonProperty("id")]
        [DataConvert("UserLevelSID")]
        public long Id { get; set; }

        [JsonProperty("name")]
        [DataConvert("RoleName")]
        public string Name { get; set; }

        public RoleDetail()
        {

        }

        public RoleDetail(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RoleDetail>(srcObject, this);
            }
        }
    }
}
