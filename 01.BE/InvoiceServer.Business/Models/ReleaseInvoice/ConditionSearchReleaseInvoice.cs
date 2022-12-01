using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchReleaseInvoice : BaseConditionSearchModel
    {
        public long? InvoiceSampleId { get; set; }
        public long? InvoiceSymbol { get; set; }
        public int? InvoiceStatus { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string CustomerName { get; set; }
        public string TaxCode { get; set; }
        public long? InvoiceSample { get; set; }
        public string NumberAccout { get; set; }
        public bool IsInvoiceChange { get; set; }

        public ConditionSearchReleaseInvoice(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            ReleaseInvoiceSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? ReleaseInvoiceSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
