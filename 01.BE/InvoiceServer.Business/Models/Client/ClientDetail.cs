using InvoiceServer.Common;
using InvoiceServer.Data.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ClientDetail
    {
        [JsonProperty("id")]
        [DataConvert("Id")]
        public long Id { get; set; }

        [StringTrimAttribute]
        [JsonProperty("code")]
        [DataConvert("CustomerCode")]
        public string CustomerCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("taxCode")]
        [DataConvert("TaxCode")]
        public string TaxCode { get; set; }

        [StringTrimAttribute]
        [JsonProperty("customerName")]
        [DataConvert("CustomerName")]
        public string CustomerName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("personContact")]
        [DataConvert("PersonContact")]
        public string PersonContact { get; set; }

        [StringTrimAttribute]
        [JsonProperty("companyName")]
        [DataConvert("CustomerName")]
        public string CompanyName { get; set; }

        [StringTrimAttribute]
        [JsonProperty("address")]
        [DataConvert("Address")]
        public string Address { get; set; }

        [StringTrimAttribute]
        [JsonProperty("mobile")]
        [DataConvert("Mobile")]
        public string Mobile { get; set; }

        [StringTrimAttribute]
        [JsonProperty("email")]
        [DataConvert("Email")]
        public string Email { get; set; }

        [StringTrimAttribute]
        [JsonProperty("userId")]
        [DataConvert("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("isOrg")]
        [DataConvert("IsOrg")]
        public bool? IsOrg { get; set; }

        [JsonProperty("bankAccount")]
        [DataConvert("BankAccount")]
        public string BankAccount { get; set; }

        [JsonProperty("bankName")]
        [DataConvert("BankName")]
        public string BankName { get; set; }

        [JsonProperty("receivedInvoiceEmail")]
        [DataConvert("ReceivedInvoiceEmail")]
        public string ReceivedInvoiceEmail { get; set; }

        [JsonProperty("useTotalInvoice")]
        [DataConvert("UseTotalInvoice")]
        public bool? UseTotalInvoice { get; set; }

        [JsonProperty("taxIncentives")]
        [DataConvert("TaxIncentives")]
        public bool? TaxIncentives { get; set; }

        [JsonProperty("sendInvoiceByMonth")]
        [DataConvert("SendInvoiceByMonth")]
        public bool? SendInvoiceByMonth { get; set; }

        [JsonProperty("useRegisterEmail")]
        public bool? UseRegisterEmail { get; set; }

        public ClientDetail()
        {

        }

        public ClientDetail(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, ClientDetail>(srcObject, this);
            }
        }
    }
}
