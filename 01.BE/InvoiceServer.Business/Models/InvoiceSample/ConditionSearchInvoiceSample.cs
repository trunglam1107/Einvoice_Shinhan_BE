using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchInvoiceSample : BaseConditionSearchModel
    {
        public string Key { get; set; }
        public string ColumnOrder1 { get; set; }
        public string ColumnOrder2 { get; set; }

        public ConditionSearchInvoiceSample(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy1;
            string macthOrderBy2;
            string macthOrderType;
            InvoiceSampleUseSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy1);
            InvoiceSampleUseSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy2);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy1 = macthOrderBy1 ?? InvoiceSampleUseSortColumn.OrderByColumnDefault1;
            macthOrderBy2 = macthOrderBy2 ?? InvoiceSampleUseSortColumn.OrderByColumnDefault2;
            macthOrderType = macthOrderType ?? OrderTypeConst.Asc;
            this.ColumnOrder1 = macthOrderBy1;
            this.ColumnOrder2 = macthOrderBy2;
            this.OrderType = macthOrderType;
        }
    }
}
