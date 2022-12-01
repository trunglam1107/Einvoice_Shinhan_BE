using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchInvoiceDebts
    {
        public DateTime? DateFrom { get; private set; }

        public DateTime? DateTo { get; private set; }

        public long CompanyId { get; private set; }

        public bool IsPaymented { get; set; }

        public ConditionSearchInvoiceDebts(UserSessionInfo currentUser, DateTime? dateFrom, DateTime? dateTo, bool isPaymented = false)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }

            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.IsPaymented = isPaymented;
        }
    }
}
