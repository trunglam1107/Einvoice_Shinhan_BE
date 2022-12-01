using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionReportSummary
    {
        public long? ContactType { get; set; }

        public long? CompanyId { get; set; }

        public long? CustomerId { get; set; }

        public string CustomerName { get; set; }
        public DateTime? DateFrom { get; private set; }
        public DateTime? DateTo { get; private set; }

        public ConditionReportSummary(UserSessionInfo currentUser, string dateFrom = null, string dateTo = null, int? contactType = null, long? customerId = null, string customerName = null)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }
            this.DateFrom = dateFrom.DecodeUrl().ConvertDateTime();
            this.DateTo = dateTo.DecodeUrl().ConvertDateTime();
            if (this.DateTo.HasValue)
            {
                this.DateTo = ((DateTime)this.DateTo).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(99999999);
            }
            this.ContactType = contactType;
            this.CustomerId = customerId;
            this.CustomerName = customerName.DecodeUrl();
        }

    }
}
