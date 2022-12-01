using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class InvoiceSign
    {
        public string Password { get; set; }
        public string FilePathPdf { get; set; }
        public string PathFileXml { get; set; }
        public bool ClientSign { get; set; }
        public string CompanyName { get; set; }
        public long InvoiceId { get; set; }
        public string DateSign { get; set; }
        public string SerialNumber { get; set; }
        public bool SignHSM { get; set; }
        public int? Slot { get; set; }
        public string DateFolder { get; set; }
        public bool isInvoice { get; set; }
    }
    public class SignListModel
    {
        public string Password { get; set; }
        public bool ClientSign { get; set; }
        public string CompanyName { get; set; }

        public string DateSign { get; set; }
        public string DateSignXml { get; set; }

        public string SerialNumber { get; set; }
        public int? Slot { get; set; }
        public bool? SignHSM { get; set; }
        public bool isInvoice { get; set; }
        public List<InvoiceFor> invoiceFors { get; set; }
    }
    public class InvoiceFor
    {
        public string FilePathPdf { get; set; }
        public string PathFileXml { get; set; }
        public long InvoiceId { get; set; }
        public string DateSign { get; set; }
        public string DateFolder { get; set; }
    }
}
