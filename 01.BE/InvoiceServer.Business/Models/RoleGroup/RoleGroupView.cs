using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class RoleGroupView
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        [DataConvert("Name")]
        public string Name { get; set; }

        [JsonProperty("isRoleOfUser")]
        public bool IsRoleOfUser { get; set; }

        public RoleGroupView()
        {

        }

        public RoleGroupView(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RoleGroupView>(srcObject, this);
            }
        }
    }
}
