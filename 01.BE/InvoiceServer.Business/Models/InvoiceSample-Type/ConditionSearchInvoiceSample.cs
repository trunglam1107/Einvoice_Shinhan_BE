using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchInvoiceSample
    {
        public string Key { get; private set; }
        public string ColumnOrder { get; set; }
        public string Order_Type { get; set; }
        public long? CompanyId { get; private set; }
        public ConditionSearchInvoiceSample(UserSessionInfo currentUser, string key, string orderBy, string orderType)
        {
            this.CompanyId = currentUser.Company.Id;
            string macthOrderBy;
            string macthOrderType;
            this.Key = key.DecodeUrl();
            InvoiceCompanyUseSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);

            macthOrderBy = macthOrderBy ?? InvoiceCompanyUseSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.Order_Type = macthOrderType;
        }
    }
}
