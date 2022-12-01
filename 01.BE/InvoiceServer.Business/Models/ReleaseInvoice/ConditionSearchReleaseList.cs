using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchReleaseList : BaseConditionSearchModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? Status { get; set; }

        public ConditionSearchReleaseList(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            ReleaseListInvoiceSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? ReleaseListInvoiceSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
