using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class UpdateRoleFunctionInfo
    {
        [JsonProperty("roleId")]
        public long RoleId { get; set; }

        [JsonProperty("roles")]
        public List<RoleFunctionInfo> Roles { get; set; }
    }
}
