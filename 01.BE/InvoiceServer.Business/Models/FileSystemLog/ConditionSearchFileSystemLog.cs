using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchFileSystemLog
    {
        public string ColumnOrder { get; set; }
        public string OrderType { get; set; }
        public DateTime? DateFrom { get; private set; }
        public DateTime? DateTo { get; private set; }

        public ConditionSearchFileSystemLog(string dateFrom, string dateTo, string orderBy, string orderType)
        {
            string macthOrderBy;
            string macthOrderType;
            FileSystemLogSort.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);

            macthOrderBy = macthOrderBy ?? FileSystemLogSort.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Asc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
            this.DateFrom = dateFrom.DecodeUrl().ConvertDateTime();
            this.DateTo = dateTo.DecodeUrl().ConvertDateTime();
        }
    }
}
