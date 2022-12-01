using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class CompanyInfo
    {
        [JsonProperty("id")]
        [DataConvert("CompanySID")]
        public long? Id { get; set; }

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

        [JsonProperty("position")]
        [DataConvert("Position")]
        public string Position { get; set; }

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

        [JsonProperty("webSite")]
        [DataConvert("WebSite")]
        public string WebSite { get; set; }

        [StringTrimAttribute]
        [JsonProperty("signaturePicture")]
        [DataConvert("SignatureFileName")]
        public string SignaturePicture { get; set; }


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

        [JsonProperty("emailOfContract")]
        [DataConvert("EmailOfContract")]
        public string EmailOfContract { get; set; }

        [JsonIgnore]
        [DataConvert("NumberFormat", DefaultValue = "{0:#,##0.###}")]
        public string NumberFormat { get; set; }

        [JsonProperty("numberFormat")]
        [DataConvert("ClientNumberFormat", DefaultValue = "0,0")]
        public string ClientNumberFormat { get; set; }

        [JsonProperty("verificationTax")]
        [DataConvert("VerificationTax", ThrowExceptionIfSourceNotExist = false, DefaultValue = false)]
        public bool VerificationTax { get; set; }

        [JsonProperty("levelCustomer")]
        [DataConvert("LevelCustomer")]
        public string LevelCustomer { get; set; }

        [JsonProperty("coreBrId")]
        [DataConvert("CoreBrId")]
        public string CoreBrId { get; set; }

        [JsonConverter(typeof(JsonDateConverterString))]
        [JsonProperty("currentDate")]

        public DateTime? CurrentDate
        {
            get
            {
                return DateTime.Now;
            }
        } // Ngày hóa đơn

        [JsonProperty("parentCompanyId")]
        [DataConvert("CompanyId")]
        public long ParentCompanyId { get; set; }

        [JsonProperty("parentCompanyName")]
        [DataConvert("MYCOMPANY2.COMPANYNAME", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentCompanyName { get; set; }

        [JsonProperty("parentCompanyAddress")]
        [DataConvert("MYCOMPANY2.ADDRESS", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentCompanyAddress { get; set; }

        [JsonProperty("parentTel")]
        [DataConvert("MYCOMPANY2.TEL1", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentTel { get; set; }

        [JsonProperty("parentDelegate")]
        [DataConvert("MYCOMPANY2.DELEGATE", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentDelegate { get; set; }

        [JsonProperty("parentPosition")]
        [DataConvert("MYCOMPANY2.POSITION", ThrowExceptionIfSourceNotExist = false, DefaultValue = "")]
        public string ParentPosition { get; set; }

        [StringTrimAttribute]
        [JsonProperty("branchId")]
        [DataConvert("BranchId")]
        public string BranchId { get; set; }
        [JsonProperty("cityId")]
        [DataConvert("CityId")]
        public long? CityId { get; set; }
        public CompanyInfo()
        {

        }

        public CompanyInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, CompanyInfo>(srcObject, this);
            }
        }

        public CompanyInfo(object srcObject, object srcAccount)
            : this(srcObject)
        {
            this.Account = srcAccount != null ? new CompanyAccount(srcAccount) : new CompanyAccount();
        }

    }
}
