using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReportCustomer
    {

        [JsonProperty("contractDate")] //2
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ContractDate { get; set; }

        [JsonProperty("contractNo")] //2
        public string ContractNo { get; set; }

        [JsonProperty("customerName")] //3
        public string CustomerName { get; set; }

        [JsonProperty("taxCode")] //4
        public string TaxCode { get; set; }

        [JsonProperty("personContact")] //4
        public string FullPersonContact
        {
            get
            {
                return string.Format("{0} <br/> {1} <br/> {2}", this.PersonContact, this.Email, this.Tel);
            }
        }

        [JsonProperty("numberRegister")] //5
        public decimal? RumberRegister { get; set; }

        [JsonProperty("numberUse")] //6
        public decimal? NumberUse { get; set; }

        [JsonIgnore]
        public string PersonContact { get; set; }

        [JsonIgnore]
        public string Email { get; set; }

        [JsonIgnore]
        public string Tel { get; set; }

        [JsonProperty("overbalance")] //6
        public decimal? Overbalance
        {
            get
            {
                if (this.RumberRegister.HasValue && this.NumberUse.HasValue)
                {
                    return (this.RumberRegister.Value - this.NumberUse.Value);
                }
                else
                {
                    return this.RumberRegister;
                }
            }
        }

        [JsonProperty("status")] //6
        public int Status { get; set; }

        public ReportCustomer()
        {

        }

        [JsonIgnore]
        public DateTime DateApproved { get; set; }
    }
}
