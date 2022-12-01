using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchInvoiceOfClient
    {
        public List<int> Status { get; private set; }

        public DateTime? DateFrom { get; private set; }

        public DateTime? DateTo { get; private set; }

        public long ClientId { get; private set; }

        public ConditionSearchInvoiceOfClient(UserSessionInfo currentUser, string dateFrom, string dateTo)
        {
            this.Status = new List<int>() { (int)InvoiceStatus.Released, (int)InvoiceStatus.Cancel };
            this.ClientId = currentUser.ClientId;
            this.DateFrom = dateFrom.DecodeUrl().ConvertDateTime();
            this.DateTo = dateTo.DecodeUrl().ConvertDateTime();
        }
    }
}
