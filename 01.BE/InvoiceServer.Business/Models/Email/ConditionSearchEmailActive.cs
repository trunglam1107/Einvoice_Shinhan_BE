using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchEmailActive : BaseConditionSearchModel
    {
        public string InvoiceNo { get; set; }
        public DateTime? DateInvoice { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? Status { get; set; }
        public int? EmailType { get; set; }
        public string EmailTo { get; set; }
        public long? Branch { get; set; }
        public string Teller { get; set; }
        public string CustomerCode { get; set; }        //search CIF


        public ConditionSearchEmailActive(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            ActiveEmailSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? ActiveEmailSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
