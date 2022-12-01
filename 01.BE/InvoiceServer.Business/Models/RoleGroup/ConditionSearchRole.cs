using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchRole
    {
        public string Key { get; private set; }
        public long UserLevelSID { get; private set; }
        public string ColumnOrder { get; set; }
        public string OrderType { get; set; }
        public UserSessionInfo CurrentUser { get; private set; }

        public ConditionSearchRole(UserSessionInfo currentUser, string key, long userLevel, string orderBy, string orderType)
        {
            this.CurrentUser = currentUser;
            this.UserLevelSID = userLevel;
            string macthOrderBy;
            string macthOrderType;
            this.Key = key.DecodeUrl();
            RoleSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);

            macthOrderBy = macthOrderBy ?? RoleSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Asc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
        public ConditionSearchRole(UserSessionInfo currentUser,long userLevel)
        {
            this.CurrentUser = currentUser;
            this.UserLevelSID = userLevel;

        }
    }
}
