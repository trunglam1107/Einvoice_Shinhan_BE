using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchHoldInvoice
    {
        public long? RegisterTemplateId { get; private set; }
        public string Symbol { get; private set; }
        public DateTime? DateInvoice { get; private set; }
        public string ColumnOrder { get; set; }
        public string OrderType { get; set; }
        public long? CompanyId { get; private set; }
        public ConditionSearchHoldInvoice(UserSessionInfo currentUser, string registerTemplateId = null, string symbol = null, string dateInvoice = null, string orderBy = null, string orderType = null)
        {
            this.CompanyId = currentUser.Company.Id;
            string macthOrderBy;
            string macthOrderType;

            long regisTemId = 0;
            if (long.TryParse(registerTemplateId, out regisTemId))
            {
                this.RegisterTemplateId = regisTemId;
            }
            this.Symbol = symbol.DecodeUrl();
            DateTime dateInv = Convert.ToDateTime(dateInvoice);
            if (dateInv != default(DateTime))
            {
                this.DateInvoice = dateInv;
            }

            HoldInvoiceSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);

            macthOrderBy = macthOrderBy ?? HoldInvoiceSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Asc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
