using InvoiceServer.Common;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceReleaseDate
    {
        [StringTrimAttribute]
        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; private set; }

        public InvoiceReleaseDate(DateTime releaseDate)
        {
            this.ReleaseDate = releaseDate.ToString("dd/MM/yyyy");
        }
    }
}
