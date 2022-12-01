using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchInvoiceReleases : BaseConditionSearchModel
    {
        public int? Status { get; private set; }
        public long? Branch { get; private set; }
        public string productName { get; private set; }

        public ConditionSearchInvoiceReleases(string orderBy, string orderType, string status, string branch)
            : base()
        {
            if (Enum.IsDefined(typeof(RecordStatus), status.ToInt(0)))
            {
                this.Status = (int)status.ParseEnum<RecordStatus>();
            }
            if (branch.IsEnumType<RecordStatus>())
            {
                this.Branch = (int)branch.ParseEnum<RecordStatus>();
            }
            string macthOrderBy;
            string macthOrderType;
            InvoiceReleasesSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? InvoiceReleasesSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
