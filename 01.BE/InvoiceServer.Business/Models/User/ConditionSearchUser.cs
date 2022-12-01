using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchUser : BaseConditionSearchModel
    {
        public string Keyword { get; set; }
        public long? RoleId { get; set; }
        public string[] ChildLevels { get; set; }
        public string LanguageCode { get; set; }
        public long? Branch { get; set; }
        public string CustomerCode { get; set; }
        public string TaxCode { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public string BranchId { get; set; }
        public ConditionSearchUser(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            LoginUserInfo.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? LoginUserInfo.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
