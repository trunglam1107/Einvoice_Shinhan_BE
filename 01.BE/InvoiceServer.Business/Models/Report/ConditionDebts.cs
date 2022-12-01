using InvoiceServer.Common.Extensions;
using System;
namespace InvoiceServer.Business.Models
{
    public class ConditionDebts
    {
        public long CompanyId { get; private set; }
        public DateTime? DateFrom { get; private set; }
        public DateTime? DateTo { get; private set; }

        public UserSessionInfo CurrentUser { get; set; }

        public string TaxtCode { get; set; }
        public ConditionDebts(UserSessionInfo currentUser, string dateFrom, string dateTo, string taxtCode)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }

            this.CurrentUser = currentUser;
            this.DateFrom = dateFrom.DecodeUrl().ConvertDateTime();
            this.DateTo = dateTo.DecodeUrl().ConvertDateTime();
            this.TaxtCode = taxtCode.DecodeUrl();
        }
    }
}
