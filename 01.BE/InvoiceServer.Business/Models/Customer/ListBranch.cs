using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class BranchLevel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("companyId")]
        public long? CompanyId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("clever")]
        public string Clever { get; set; }

        [JsonProperty("taxcode")]
        public string TaxCode { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("tell")]
        public string Tell { get; set; }

        [JsonProperty("fax")]
        public string Fax { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("personContact")]
        public string PersonContact { get; set; }

        [JsonProperty("delegate")]
        public string Delegate { get; set; }

        [JsonProperty("bankAccount")]
        public string BankAccount { get; set; }

        [JsonProperty("accountHolder")]
        public string AccountHolder { get; set; }

        [JsonProperty("bankName")]
        public string BankName { get; set; }

        [JsonProperty("cityId")]
        public long? CityId { get; set; }

        [JsonProperty("taxDepartmentId")]
        public long? TaxDepartmentId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("delete")]
        public bool? Delete { get; set; }

        [JsonProperty("emailOfContract")]
        public string EmailOfContract { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        public BranchLevel()
        {

        }

    }
}
