using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchAutoNumber : BaseConditionSearchModel
    {
        public ConditionSearchAutoNumber(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            AutoNumberSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? AutoNumberSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
