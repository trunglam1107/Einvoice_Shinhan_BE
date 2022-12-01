using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchAdjustInvoice
    {
        public long? InvoiceSampleId { get; private set; }
        public int? InvoiceSymbol { get; private set; }
        public int? InvoiceStatus { get; private set; }
        public string InvoiceNo { get; private set; }
        public DateTime? DateFrom { get; private set; }
        public DateTime? DateTo { get; private set; }
        public string CustomerName { get; private set; }
        public string TaxCode { get; private set; }
        public int? InvoiceSample { get; private set; }
        public string ColumnOrder { get; set; }
        public string OrderType { get; set; }
        public long CompanyId { get; private set; }
        public string NumberAccout { get; private set; }

        public ConditionSearchAdjustInvoice(UserSessionInfo currentUser, long? invoiceSampleId, int? invoiceSymbol, int? invoiceSample, string invoiceNo)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }

            this.InvoiceSampleId = invoiceSampleId;
            this.InvoiceSymbol = invoiceSymbol;
            this.InvoiceSample = invoiceSample;
            this.InvoiceNo = invoiceNo.DecodeUrl();
        }
    }
}
