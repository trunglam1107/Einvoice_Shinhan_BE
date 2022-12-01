using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models.Account
{
    public class ClientAccountExport
    {
        [JsonProperty("items")]

        public List<AccountViewModel> Items { get; set; }

        public ClientAccountExport()
        {
        }
    }
}
