using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchCustomer : BaseConditionSearchModel
    {
        public string CustomerName { get; set; }
        public string Delegate { get; set; }
        public string TaxCode { get; set; }
        public string Tel { get; set; }
        public int? Status { get; set; }
        public long? Branch { get; set; }

        public string BranchId { get; set; }

        public ConditionSearchCustomer(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            CustomerSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? CustomerSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
