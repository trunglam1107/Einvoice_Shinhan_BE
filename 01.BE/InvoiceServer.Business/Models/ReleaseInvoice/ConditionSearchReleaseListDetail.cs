using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchReleaseListDetail
    {
        public long? ReleaseId { get; set; }

        public long CompanyId { get; private set; }

        public string ColumnOrder { get; set; }

        public string OrderType { get; set; }

        public ConditionSearchReleaseListDetail(UserSessionInfo currentUser, long? releaseId, string orderBy, string orderType)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }

            this.ReleaseId = releaseId;
            string macthOrderBy;
            string macthOrderType;
            InvoiceSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);

            macthOrderBy = macthOrderBy ?? InvoiceSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
