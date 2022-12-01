using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class MyCompanyInfo
    {
        [JsonProperty("id")]
        [DataConvert("CompanySID")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [JsonProperty("address")]
        [DataConvert("Address")]
        public string Address { get; set; }

        [JsonProperty("tax")]
        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }

        [StringTrimAttribute]
        [JsonProperty("personContact")]
        [DataConvert("PersonContact")]
        public string PersonContact { get; set; }

        [JsonProperty("tel")]
        [DataConvert("Tel1")]
        public string Tel { get; set; }

        [JsonProperty("fax")]
        [DataConvert("Fax")]
        public string Fax { get; set; }

        [JsonProperty("delegate")]
        [DataConvert("Delegate")]
        public string Delegate { get; set; }

        [JsonProperty("bankAccount")]
        [DataConvert("BankAccount")]
        public string BankAccount { get; set; }

        [JsonProperty("accountHolder")]
        [DataConvert("AccountHolder")]
        public string AccountHolder { get; set; }


        [JsonProperty("bankName")]
        [DataConvert("BankName")]
        public string BankName { get; set; }

        [JsonIgnore()]
        [DataConvert("TaxDepartmentId")]
        public long? TaxDepartmentId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        [DataConvert("Description")]
        public string Description { get; set; }

        [JsonProperty("logo")]
        [DataConvert("LogoFileName")]
        public string Logo { get; set; }

        [StringTrimAttribute]
        [JsonProperty("signaturePicture")]
        [DataConvert("SignatureFileName")]
        public string SignaturePicture { get; set; }

        [StringTrimAttribute]
        [JsonProperty("website")]
        [DataConvert("WebSite")]
        public string WebSite { get; set; }


        [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
        public CompanyAccount Account { get; set; }


        [JsonProperty("openFileWhenSign")]
        [DataConvert("OpenFileWhenSign")]
        public bool OpenFileWhenSign { get; set; }

        [JsonProperty("reportTel")]
        [DataConvert("ReportTel")]
        public string ReportTel { get; set; }

        [JsonProperty("reportWebsite")]
        [DataConvert("ReportWebsite")]
        public string ReportWebsite { get; set; }

        [JsonProperty("levelAgencies")]
        [DataConvert("LevelAgencies")]
        public int LevelAgencies { get; set; }

        [JsonProperty("signatures")]
        public List<SignatureInfo> Signatures { get; set; }

        [JsonProperty("position")]
        [DataConvert("Position")]
        public string Position { get; set; }


        [JsonProperty("levelCustomer")]
        [DataConvert("LevelCustomer")]
        public string LevelCustomer { get; set; }

        [StringTrimAttribute]
        [JsonProperty("parentName")]
        [DataConvert("MYCOMPANY2.COMPANYNAME", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentCompanyName { get; set; }

        [JsonProperty("parentAddress")]
        [DataConvert("MYCOMPANY2.ADDRESS", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentAddress { get; set; }

        [JsonProperty("parentTel")]
        [DataConvert("MYCOMPANY2.TEL1", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentTel { get; set; }

        [JsonProperty("parentDelegate")]
        [DataConvert("MYCOMPANY2.DELEGATE", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentDelegate { get; set; }

        [JsonProperty("parentPosition")]
        [DataConvert("MYCOMPANY2.POSITION", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentPosition { get; set; }

        public MyCompanyInfo()
        {

        }

        public MyCompanyInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, MyCompanyInfo>(srcObject, this);
            }
        }

        public MyCompanyInfo(object srcObject, object srcAccount)
            : this(srcObject)
        {
            this.Account = srcAccount != null ? new CompanyAccount(srcAccount) : new CompanyAccount();
        }

    }
}
