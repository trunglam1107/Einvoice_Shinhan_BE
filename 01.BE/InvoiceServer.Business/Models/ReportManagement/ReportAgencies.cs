using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class ReportAgencies
    {

        [JsonProperty("contractDate")] //2
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ContractDate { get; set; }

        [JsonProperty("contractNo")] //2
        public string ContractNo { get; set; }

        [JsonProperty("agenciesName")] //3
        public string AgenciesName { get; set; }

        [JsonProperty("personContact")] //4
        public string PersonContact { get; set; }

        [JsonProperty("mobilde")] //5
        public string Mobile { get; set; }

        [JsonProperty("email")] //6
        public string Email { get; set; }

        [JsonProperty("status")] //6
        public int Status { get; set; }
        public ReportAgencies()
        {
           
        }
    }
}
