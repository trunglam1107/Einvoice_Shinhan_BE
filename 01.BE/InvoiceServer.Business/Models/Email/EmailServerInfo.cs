using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class EmailServerInfo
    {

        [StringTrimAttribute]
        [JsonProperty("companyId")]
        //[DataConvert("CompanyId")]
        public long CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("autoSendEmail")]
        [DataConvert("AutoSendEmail")]
        public bool AutoSendEmail { get; set; }

        [StringTrimAttribute]
        [JsonProperty("methodSendSSL")]
        [DataConvert("MethodSendSSL")]
        public bool MethodSendSSL { get; set; }

        [StringTrimAttribute]
        [JsonProperty("smtpServer")]
        [DataConvert("SMTPServer")]
        public string SMTPServer { get; set; }

        [StringTrimAttribute]
        [JsonProperty("port")]
        [DataConvert("Port")]
        public int Port { get; set; }

        [StringTrimAttribute]
        [JsonProperty("emailServer")]
        [DataConvert("EmailServer")]
        public string EmailServer { get; set; }

        [StringTrimAttribute]
        [JsonProperty("userName")]
        [DataConvert("UserName")]
        public string UserName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("password")]
        [DataConvert("Password")]
        public string Password { get; set; }

        [StringTrimAttribute]
        [JsonProperty("secureSocketOptions")]
        [DataConvert("SecureSocketOptions")]
        public int SecureSocketOptions { get; set; }

        [StringTrimAttribute]
        [JsonProperty("sendMailType")]
        public int SendMailType
        {
            get
            {
                return CommonUtil.GetConfig("SendMailType", 0);
            }
        }

        public EmailServerInfo()
        {
        }

        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public EmailServerInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, EmailServerInfo>(srcObject, this);
            }
        }
    }
}
