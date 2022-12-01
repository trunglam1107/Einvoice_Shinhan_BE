using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReplacedInvoice
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("no")]
        public string No { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        public DateTime? Created { get; set; }
        [JsonProperty("items")]
        public List<BeReplacedInvoice> BeReplacedInvoices { get; set; }

        [JsonIgnore]
        public long ParentId { get; set; }

        [JsonProperty("invoiceStatus")]
        public int InvoiceStatus { get; set; }

        [JsonProperty("fileReceipt")]
        public string FileReceipt { get; set; }

        public ReplacedInvoice()
        {

        }
    }

    public class BeReplacedInvoice
    {
        [JsonProperty("id")]
        public long IdSubstitute { get; set; }

        [JsonProperty("code")]
        public string CodeSubstitute { get; set; }

        [JsonProperty("symbol")]
        public string SymbolSubstitute { get; set; }

        [JsonProperty("no")]
        public string NoSubstitute { get; set; }

        [JsonProperty("note")]
        public string NoteSubstitute { get; set; }

        [JsonProperty("fileReceipt")]
        public string FileReceiptSubstitute { get; set; }

        public BeReplacedInvoice()
        {

        }

    }
}


