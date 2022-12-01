using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchUseInvoice : BaseConditionSearchModel
    {
        public int? Status { get; set; }
        public long? Branch { get; set; }
        public DateTime? DateFrom { get; set; }

        private DateTime? dateTo;
        public DateTime? DateTo
        {
            get
            {
                return dateTo;
            }
            set
            {
                if (value.HasValue)
                {
                    this.dateTo = ((DateTime)value).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddTicks(99999999);
                }
            }
        }

        public ConditionSearchUseInvoice(string orderBy, string orderType, string status, string branch)
            : base()
        {
            if (status.IsEnumType<RecordStatus>())
            {
                this.Status = (int)status.ParseEnum<RecordStatus>();
            }
            if (branch.IsEnumType<RecordStatus>())
            {
                this.Branch = (int)branch.ParseEnum<RecordStatus>();
            }
            string macthOrderBy;
            string macthOrderType;
            NoticeUseInvoiceSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? NoticeUseInvoiceSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
