using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class UpdateUsersInRole
    {
        [JsonProperty("role")]
        public RoleGroupDetail Role { get; set; }

        [JsonProperty("usersInRole")]
        public List<AccountDetail> UsersInRole { get; set; }

        public UpdateUsersInRole()
        {

        }
    }
}
