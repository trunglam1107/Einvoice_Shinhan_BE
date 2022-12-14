using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class AccountInfo
    {
        [JsonProperty("id")]
        [DataConvert("UserSID")]
        public long UserSID { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("UserName")]
        public string UserName { get; set; }

        [JsonProperty("loginId")]
        [DataConvert("UserID")]
        public string UserID { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }

        [StringTrimAttribute]
        [JsonProperty("password")]
        [DataConvert("Password")]
        public string Password { get; set; }

        [JsonProperty("mobile")]
        [DataConvert("Mobile")]
        public string Mobile { get; set; }

        [JsonProperty("roles")]
        public List<FunctionInfo> Roles { get; set; }

        public AccountInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public AccountInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, AccountInfo>(srcObject, this);
            }
        }
    }
}
