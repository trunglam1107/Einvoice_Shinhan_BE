using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class RoleLevel
    {
        [JsonProperty("roleName")]
        [DataConvert("RoleName")]
        public string RoleName { get; set; }

        [JsonProperty("defaultUrl")]
        [DataConvert("DefaultPage")]
        public string DefaultPage { get; set; }

        [JsonProperty("permission")]
        public List<string> Permissions { get; set; }

        [JsonProperty("level")]
        [DataConvert("Levels")]
        public string Level { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RoleLevel()
        {

        }

        public RoleLevel(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, RoleLevel>(srcObject, this);
            }
        }

        public RoleLevel(object srcObject, List<string> permissions)
            : this(srcObject)
        {
            if (permissions != null)
            {
                this.Permissions = permissions;
            }
        }
    }

}
