using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class InvoiceDateInfo
    {
        public DateTime? DateOfInvoice { get; private set; }

        public long CompanyId { get; set; }

        public string InvoiceNo { get; set; }

        public long RegisterTemplateId { get; private set; }

        public string Symbol { get; set; }

        public long Branch { get; set; }

        public InvoiceDateInfo(string invoiceDate, long registerTemplateId, string symbol)
        {
            this.DateOfInvoice = invoiceDate.DecodeUrl().ConvertDateTime();
            this.RegisterTemplateId = registerTemplateId;
            this.Symbol = symbol.DecodeUrl();
        }

        public InvoiceDateInfo(string invoiceDate, long registerTemplateId, string symbol, string invoiceNo) :
            this(invoiceDate, registerTemplateId, symbol)
        {
            this.InvoiceNo = invoiceNo.DecodeUrl();
        }
        public InvoiceDateInfo(string invoiceDate, long registerTemplateId, string symbol, long companyId, string invoiceNo) :
            this(invoiceDate, registerTemplateId, symbol)
        {
            this.CompanyId = companyId;
            this.InvoiceNo = invoiceNo;
        }

        public InvoiceDateInfo(DateTime invoiceDate, long registerTemplateId, string symbol, long companyId, string invoiceNo)
        {
            this.CompanyId = companyId;
            this.InvoiceNo = invoiceNo;
            this.DateOfInvoice = invoiceDate;
            this.RegisterTemplateId = registerTemplateId;
            this.Symbol = symbol;
        }

        public InvoiceDateInfo(string invoiceDate, long registerTemplateId, string symbol, long branch)
        {
            this.DateOfInvoice = invoiceDate.DecodeUrl().ConvertDateTime();
            this.RegisterTemplateId = registerTemplateId;
            this.Symbol = symbol.DecodeUrl();
            this.Branch = branch;
        }
    }
}
