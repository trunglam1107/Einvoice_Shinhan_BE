using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class InvoiceSystemSettingInfo
    {
        [JsonProperty("url")]
        [DataConvert("Url")]
        public string Url { get; set; }

        [JsonProperty("invoiceWithCode")]
        [DataConvert("InvoiceWithCode")]
        public string InvoiceWithCode { get; set; }

        [JsonProperty("invoiceNotCode")]
        [DataConvert("InvoiceNotCode")]
        public string InvoiceNotCode { get; set; }

        [JsonProperty("username")]
        [DataConvert("Username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        [DataConvert("Password")]
        public string Password { get; set; }

        [JsonProperty("cancel")]
        [DataConvert("Cancel")]
        public string Cancel { get; set; }

        [JsonProperty("login")]
        [DataConvert("Login")]
        public string Login { get; set; }

        [JsonProperty("register")]
        [DataConvert("Register")]
        public string Register { get; set; }

        [JsonProperty("signUp")]
        [DataConvert("SignUp")]
        public string SignUp { get; set; }

        [JsonProperty("synthesis")]
        [DataConvert("Synthesis")]
        public string Synthesis { get; set; }

        [JsonProperty("mstnnt")]
        [DataConvert("Mstnnt")]
        public string Mstnnt { get; set; }

        [JsonProperty("mdvcs")]
        [DataConvert("mdvcs")]
        public string Mdvcs { get; set; }

        [JsonProperty("mstcgp")]
        [DataConvert("Mstcgp")]
        public string Mstcgp { get; set; }

        [JsonProperty("adminUrl")]
        [DataConvert("AdminUrl")]
        public string AdminUrl { get; set; }

        [JsonProperty("search")]
        [DataConvert("Search")]
        public string Search { get; set; }

        [JsonProperty("adminUserId")]
        [DataConvert("AdminUserId")]
        public string AdminUserId { get; set; }

        [JsonProperty("adminPassword")]
        [DataConvert("AdminPassword")]
        public string AdminPassword { get; set; }

        [JsonProperty("adminLogin")]
        [DataConvert("AdminLogin")]
        public string AdminLogin { get; set; }
    }

}
