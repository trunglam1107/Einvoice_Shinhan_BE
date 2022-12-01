using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionReportCancelInvoice
    {
        public long CompanyId { get; set; }
        public long? InvoiceSampleId { get; set; }
        public DateTime? DateFrom { get; private set; }
        public DateTime? DateTo { get; private set; }
        public string Recipients { get; set; }
        public string TypeCancel { get; set; }

        public ConditionReportCancelInvoice(UserSessionInfo currentUser, long? invoiceSampleId, string dateFrom, string dateTo)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }

            this.Recipients = string.Empty;
            this.TypeCancel = string.Empty;
            this.InvoiceSampleId = invoiceSampleId;
            this.DateFrom = dateFrom.DecodeUrl().ConvertDateTime();
            this.DateTo = dateTo.DecodeUrl().ConvertDateTime();
        }
    }
}
