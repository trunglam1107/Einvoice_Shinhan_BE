using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class UseInvoiceInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [JsonProperty("companyId")]
        [DataConvert("CompanyId")]
        public long? CompanyId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("CompanyName")]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("levelCustomer")]
        [DataConvert("MYCOMPANY.LEVELCUSTOMER", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string LevelCustomer { get; set; }

        [StringTrimAttribute]
        [JsonProperty("address")]
        [DataConvert("COMPANYADDRESS")]
        public string Address { get; set; }

        [StringTrimAttribute]
        [JsonProperty("delegate")]
        [DataConvert("Delegate")]
        public string Delegate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxCode")]
        [DataConvert("COMPANYTAXCODE")]
        public string TaxCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("bankAcount")]
        [DataConvert("BankAccount")]
        public string BankAccount { get; set; }

        [StringTrimAttribute]
        [JsonProperty("tel")]
        [DataConvert("COMPANYPHONE")]
        public string Tel { get; set; }

        [JsonProperty("taxDepartmentId")]
        [DataConvert("TaxDepartmentId")]
        public long? TaxDepartmentId { get; set; }

        [JsonProperty("taxDepartmentName")]
        [DataConvert("TaxDepartmentName", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string TaxDepartmentName { get; set; }

        [JsonIgnore]
        [DataConvert("CityName", DefaultValue = null, ThrowExceptionIfSourceNotExist = false)]
        public string CityName { get; set; }

        [JsonProperty("cityId")]
        [DataConvert("CityId")]
        public long? CityId { get; set; }

        [JsonProperty("description")]
        [DataConvert("Description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        [DataConvert("Status")]
        public int Status { get; set; }

        [JsonProperty("items")]
        public List<UseInvoiceDetailInfo> UseInvoiceDetails { get; set; }

        [StringTrimAttribute]
        [JsonProperty("createdDate")]
        [DataConvert("CreatedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? CreatedDate { get; set; }


        [JsonIgnore()]
        public long UserActionId { get; set; }
        public UseInvoiceInfo()
        {
        }
        /// <summary>
        /// Constructor current Client object by copying data in the specified object
        /// </summary>
        /// <param name="srcUser">Source object</param>
        public UseInvoiceInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, UseInvoiceInfo>(srcObject, this);
            }
        }

        public UseInvoiceInfo(object srcObject, List<UseInvoiceDetailInfo> useInvoiceDetails)
            : this(srcObject)
        {
            this.UseInvoiceDetails = useInvoiceDetails;
        }
    }
}
