using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class CustomerInfo
    {
        [JsonProperty("id")]
        [DataConvert("CompanySID")]
        public long? Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("name")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [JsonProperty("tax")]
        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("address")]
        [DataConvert("Address")]
        public string Address { get; set; }

        [JsonProperty("delegate")]
        [DataConvert("Delegate")]
        public string Delegate { get; set; }


        [JsonProperty("tel")]
        [DataConvert("Tel1")]
        public string Tel { get; set; }

        [JsonProperty("fax")]
        [DataConvert("Fax")]
        public string Fax { get; set; }

        [JsonProperty("bankAccount")]
        [DataConvert("BankAccount")]
        public string BankAccount { get; set; }

        [JsonProperty("accountHolder")]
        [DataConvert("AccountHolder")]
        public string AccountHolder { get; set; }


        [JsonProperty("bankName")]
        [DataConvert("BankName")]
        public string BankName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("personContact")]
        [DataConvert("PersonContact")]
        public string PersonContact { get; set; }

        [StringTrimAttribute]
        [JsonProperty("position")]
        [DataConvert("Position")]
        public string Position { get; set; }

        [StringTrimAttribute]
        [JsonProperty("mobile")]
        [DataConvert("Mobile")]
        public string Mobile { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }


        [JsonProperty("active")]
        [DataConvert("Active")]
        public bool Active { get; set; }

        [JsonProperty("website")]
        [DataConvert("WebSite")]
        public string WebSite { get; set; }

        [JsonProperty("levelCustomer")]
        [DataConvert("LevelCustomer")]
        public string LevelCustomer { get; set; }

        [JsonIgnore]
        public UserSessionInfo SessionInfo { get; set; }

        [JsonProperty("cityId")]
        [DataConvert("CityId")]
        public long? CityId { get; set; }

        [JsonProperty("city")]
        public CityInfo City { get; set; }

        [JsonProperty("company2")]
        public CompanyInfo Company2 { get; set; }

        [JsonProperty("taxDepartmentId")]
        [DataConvert("TaxDepartmentId")]
        public long? TaxDepartmentId { get; set; }

        [JsonProperty("taxDepartment")]
        public TaxDepartmentInfo TaxDepartment { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [JsonProperty("openFileWhenSign")]
        [DataConvert("OpenFileWhenSign")]
        public bool OpenFileWhenSign { get; set; }


        [StringTrimAttribute]
        [JsonProperty("emailContract")]
        [DataConvert("EmailOfContract")]
        public string EmailContract { get; set; }

        [StringTrimAttribute]
        [JsonProperty("branchId")]
        [DataConvert("BranchId")]
        public string BranchId { get; set; }

        [JsonProperty("myCompanyUpgrade")]
        public CompanyUpgradeInfo MyCompanyUpgrade { get; set; }
        public CustomerInfo()
        {

        }
        public CustomerInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, CustomerInfo>(srcObject, this);
            }
        }

        public CustomerInfo(object srcObject, object srcCity)
            : this(srcObject)
        {
            this.City = srcCity != null ? new CityInfo(srcCity) : new CityInfo();
        }

        public CustomerInfo(object srcObject, object srcCity, object srcTaxDepartment)
            : this(srcObject, srcCity)
        {
            this.TaxDepartment = srcCity != null ? new TaxDepartmentInfo(srcTaxDepartment) : new TaxDepartmentInfo();
        }

        public CustomerInfo(object srcObject, object srcCity, object srcTaxDepartment, object srcCompany2)
            : this(srcObject, srcCity, srcTaxDepartment)
        {
            this.Company2 = srcCompany2 != null ? new CompanyInfo(srcCompany2) : new CompanyInfo();
        }

        public CustomerInfo(object srcObject, object srcCity, object srcTaxDepartment, object srcCompany2, object srcUpgrade)
           : this(srcObject, srcCity)
        {
            this.TaxDepartment = srcCity != null ? new TaxDepartmentInfo(srcTaxDepartment) : new TaxDepartmentInfo();
            this.MyCompanyUpgrade = srcUpgrade != null ? new CompanyUpgradeInfo(srcUpgrade) : new CompanyUpgradeInfo();
        }

    }
}
