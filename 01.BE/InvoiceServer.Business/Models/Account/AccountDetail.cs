using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class AccountDetail
    {
        [JsonProperty("id")]
        [DataConvert("UserSID")]
        public long UserSID { get; set; }

        [JsonProperty("loginId")]
        [DataConvert("UserID")]
        public string UserID { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("UserName")]
        public string UserName { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }

        [JsonProperty("mobile")]
        [DataConvert("Mobile")]
        public string Mobile { get; set; }

        [StringTrimAttribute]
        [JsonProperty("active")]
        [DataConvert("IsActive")]
        public bool IsActive { get; set; }

        [StringTrimAttribute]
        [JsonProperty("deleted")]
        [DataConvert("Deleted")]
        public bool Deleted { get; set; }

        [StringTrimAttribute]
        [JsonProperty("createdDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        [DataConvert("CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("roleId")]
        [DataConvert("UserLevelSID")]
        public long UserLevelSID { get; set; }

        [StringTrimAttribute]
        [JsonProperty("levelCustomer")]
        [DataConvert("LevelCustomer")]
        public string LevelCustomer { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("branchId")]
        [DataConvert("BranchId")]
        public string BranchId { get; set; }
        [StringTrimAttribute]
        [JsonProperty("role")]
        [DataConvert("Role")]
        public string Role { get; set; }

        [StringTrimAttribute]
        [JsonProperty("tenQuyen")]
        [DataConvert("TenQuyen")]
        public string TenQuyen { get; set; }
        public AccountDetail()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public AccountDetail(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, AccountDetail>(srcObject, this);
            }
        }

    }
}
