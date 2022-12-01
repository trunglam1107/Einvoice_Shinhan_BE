using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionInvoiceUse
    {
        public long CompanyId { get; set; }

        public long RegisterTemplateId { get; set; }

        public string Symbol { get; set; }

        public DateTime? DateFrom { get; private set; }

        public DateTime? DateTo { get; private set; }

        public ConditionInvoiceUse()
        {

        }

        public ConditionInvoiceUse(long companyId, long registerTemplateId, string symbol)
        {
            this.CompanyId = companyId;
            this.RegisterTemplateId = registerTemplateId;
            this.Symbol = symbol;
        }
    }
}
