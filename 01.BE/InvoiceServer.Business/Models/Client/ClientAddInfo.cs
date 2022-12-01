using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ClientAddInfo
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("userId")]
        [DataConvert("CustomerId")]
        public string CustomerId { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxCode")]
        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("customerCode")]
        [DataConvert("CustomerCode")]
        public string CustomerCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("customerName")]
        [DataConvert("CustomerName")]
        public string CustomerName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("typeSendInvoice")]
        [DataConvert("TypeSendInvoice")]
        public int TypeSendInvoice { get; set; }

        [StringTrimAttribute]
        [JsonProperty("mobile")]
        [DataConvert("Mobile")]
        public string Mobile { get; set; }

        [StringTrimAttribute]
        [JsonProperty("fax")]
        [DataConvert("Fax")]
        public string Fax { get; set; }

        [StringTrimAttribute]
        [JsonProperty("address")]
        [DataConvert("Address")]
        public string Address { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }

        [StringTrimAttribute]
        [JsonProperty("delegate")]
        [DataConvert("Delegate")]
        public string Delegate { get; set; }

        [StringTrimAttribute]
        [JsonProperty("personContact")]
        [DataConvert("PersonContact")]
        public string PersonContact { get; set; }

        [StringTrimAttribute]
        [JsonProperty("bankAccount")]
        [DataConvert("BankAccount")]
        public string BankAccount { get; set; }

        [StringTrimAttribute]
        [JsonProperty("accountHolder")]
        [DataConvert("AccountHolder")]
        public string AccountHolder { get; set; }

        [StringTrimAttribute]
        [JsonProperty("bankName")]
        [DataConvert("BankName")]
        public string BankName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("description")]
        [DataConvert("Description")]
        public string Description { get; set; }

        [StringTrimAttribute]
        [JsonProperty("customerType")]
        [DataConvert("CustomerType")]
        public int? CustomerType { get; set; }

        [StringTrimAttribute]
        [JsonProperty("isOrg")]
        [DataConvert("IsOrg")]
        public bool? IsOrg { get; set; }

        [StringTrimAttribute]
        [JsonProperty("sendInvoiceByMonth")]
        [DataConvert("SendInvoiceByMonth")]
        public bool? SendInvoiceByMonth { get; set; }

        [StringTrimAttribute]
        [JsonProperty("dateSendInvoice")]
        [DataConvert("DateSendInvoice")]
        public int? DateSendInvoice { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxIncentives")]
        [DataConvert("TaxIncentives")]
        public bool? TaxIncentives { get; set; }

        [StringTrimAttribute]
        [JsonProperty("useTotalInvoice")]
        [DataConvert("UseTotalInvoice")]
        public bool? UseTotalInvoice { get; set; }

        [StringTrimAttribute]
        [JsonProperty("useRegisterEmail")]
        [DataConvert("UseRegisterEmail")]
        public bool? UseRegisterEmail { get; set; }

        [StringTrimAttribute]
        [JsonProperty("receivedInvoiceEmail")]
        [DataConvert("ReceivedInvoiceEmail")]
        public string ReceivedInvoiceEmail { get; set; }

        [JsonIgnore()]
        public long UserAction { get; set; }

        [JsonIgnore()]
        public long EmailActiveId { get; set; }

        public ClientAddInfo()
        {

        }

        public ClientAddInfo(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ClientAddInfo>(srcObject, this);
            }
        }
    }
}
