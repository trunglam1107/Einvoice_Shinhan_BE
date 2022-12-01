using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class SearchReplacedInvoice : BaseConditionSearchModel
    {
        public long? InvoiceSampleId { get; set; }
        public string InvoiceSymbol { get; set; }
        public List<int> InvoiceSamples { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string CustomerName { get; set; }
        public string TaxCode { get; set; }
        public int? InvoiceType { get; set; }
        public string NumberAccout { get; set; }
        public long? Branch { get; set; }
        public string InvoiceNoReplaced { get; set; }
        public string CustomerCode { get; set; }
        public string Language { get; set; }
        public string FromInvoiceNo { get; set; }

        public string ToInvoiceNo { get; set; }
        public SearchReplacedInvoice(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            ReplacesInvoiceSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? ReplacesInvoiceSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }
    }
}
