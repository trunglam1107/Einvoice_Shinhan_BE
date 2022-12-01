using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchProduct
    {
        public string productCode { get; private set; }
        public string ColumnOrder { get; set; }
        public string OrderType { get; set; }
        public string productName { get; private set; }
        public long? CompanyId { get; private set; }

        public ConditionSearchProduct(UserSessionInfo currentUser, string productCode, string productName, string orderBy, string orderType)
        {
            this.productCode = productCode.DecodeUrl();
            this.productName = productName.DecodeUrl();
            this.CompanyId = currentUser.Company.Id;

            string macthOrderBy;
            string macthOrderType;
            ProductInfoAction.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);

            macthOrderBy = macthOrderBy ?? ProductInfoAction.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
