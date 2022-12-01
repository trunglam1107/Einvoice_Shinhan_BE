using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchRegisterTemplate : BaseConditionSearchModel
    {
        public string Key { get; set; }
        public long Branch { get; set; }
        public string ColumnOrder1 { get; set; }
        public string ColumnOrder2 { get; set; }

        public ConditionSearchRegisterTemplate(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy1;
            string macthOrderBy2;
            string macthOrderType;
            RegisterTemplateUseSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy1);
            RegisterTemplateUseSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy2);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy1 = macthOrderBy1 ?? RegisterTemplateUseSortColumn.OrderByColumnDefault1;
            macthOrderBy2 = macthOrderBy2 ?? RegisterTemplateUseSortColumn.OrderByColumnDefault2;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder1 = macthOrderBy1;
            this.ColumnOrder2 = macthOrderBy2;
            this.OrderType = macthOrderType;
        }
    }
}
