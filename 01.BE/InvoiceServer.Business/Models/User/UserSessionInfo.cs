using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class UserSessionInfo
    {
        private static ulong LastSessionId = 0;

        #region Fields, Properties

        [JsonIgnore]
        public readonly ulong SessionId;

        [JsonIgnore]
        public bool IsOverwriteLogin { get; set; }

        [JsonProperty("id")]
        [DataConvert("UserSID")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("userId")]
        [DataConvert("UserID")]
        public string UserId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("username")]
        [DataConvert("UserName")]
        public string UserName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("language")]
        [DataConvert("Language")]
        public string Language { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }

        [StringTrimAttribute]
        [JsonIgnore]
        [DataConvert("Password")]
        public string Password { get; set; }

        [StringTrimAttribute]
        [JsonProperty("passwordStatus")]
        [DataConvert("PasswordStatus")]
        public int? PasswordStatus { get; set; }

        [StringTrimAttribute]
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("role")]
        public RoleLevel RoleUser { get; set; }

        [JsonProperty("company")]
        public CompanyInfo Company { get; set; }

        [StringTrimAttribute]
        [JsonProperty("clientId")]
        [DataConvert("ClientId")]
        public long ClientId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("systemSetting")]
        public SystemSettingInfo SystemSetting { get; set; }

        [JsonIgnore]
        public EmailServerInfo EmailServer { get; set; }

        [JsonProperty("customerIndex")]
        public int CustomerIndex { get; private set; }


        [JsonIgnore]
        public long NumberInvoicePackage { get; set; }

        #endregion

        #region Contructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserSessionInfo()
        {
            SessionId = ++LastSessionId;
            this.CustomerIndex = Common.Constants.CustomerIndex.GetCustomerIndex();
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public UserSessionInfo(object srcObject, string token)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, UserSessionInfo>(srcObject, this);
            }

            this.Token = token;
        }


        public UserSessionInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, UserSessionInfo>(srcObject, this);
            }
        }

        public UserSessionInfo(object srcObject, object srcRole, string token)
            : this(srcObject)
        {
            this.RoleUser = srcRole != null ? new RoleLevel(srcRole) : new RoleLevel();
            this.Token = token;
        }

        public UserSessionInfo(object srcObject, object srcRole, object srcCompany, string token)
            : this(srcObject, srcRole, token)
        {
            this.Company = srcCompany != null ? new CompanyInfo(srcCompany) : new CompanyInfo();

            if (srcRole.GetType().GetProperty("LEVELS").GetValue(srcRole).ToString() == RoleInfo.CLIENT)
            {
                this.Company.Id = null;
            }
            this.Token = token;
        }
        #endregion
    }
}
