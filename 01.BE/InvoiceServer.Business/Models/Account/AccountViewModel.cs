using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models.Account
{
    public class AccountViewModel
    {
        [JsonProperty("id")]
        [DataConvert("UserSID")]
        public long UserSID { get; set; }


        [StringTrimAttribute]
        [JsonProperty("userName")]
        [DataConvert("UserName")]
        public string UserName { get; set; }

        [JsonProperty("userID")]
        [DataConvert("UserID")]
        public string UserID { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }

        [JsonProperty("roleName")]
        [DataConvert("RoleName")]
        public string RoleName { get; set; }
        [JsonProperty("companyName")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }
        [StringTrimAttribute]
        [JsonProperty("password")]
        [DataConvert("Password")]
        public string Password { get; set; }

        [JsonProperty("isActive")]
        [DataConvert("IsActive")]
        public bool IsActive { get; set; }
        [JsonProperty("createdDate")]
        [DataConvert("CreatedDate")]
        public DateTime? CreatedDate { get; set; }
        [JsonProperty("updatedDate")]
        [DataConvert("UpdatedDate")]
        public DateTime? UpdatedDate { get; set; }
        [JsonProperty("lastAccessedTime")]
        [DataConvert("LastAccessedTime")]
        public DateTime? LastAccessedTime { get; set; }
        [JsonProperty("lastChangedpasswordTime")]
        [DataConvert("LastChangedpasswordTime")]
        public DateTime? LastChangedpasswordTime { get; set; }

        [JsonProperty("mobile")]
        [DataConvert("Mobile")]
        public string Mobile { get; set; }
        [JsonProperty("userLevelsId")]
        [DataConvert("UserLevelsId")]
        public long? UserLevelsId { get; set; }
        [JsonProperty("roles")]
        public List<FunctionInfo> Roles { get; set; }

        [JsonProperty("customerCode")]
        [DataConvert("CustomerCode")]
        public string CustomerCode { get; set; }

        [JsonProperty("taxCode")]
        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }

        public AccountViewModel()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public AccountViewModel(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, AccountViewModel>(srcObject, this);
            }
        }
    }
}
