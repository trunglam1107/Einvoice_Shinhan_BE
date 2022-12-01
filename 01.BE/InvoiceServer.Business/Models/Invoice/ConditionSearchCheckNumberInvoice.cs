using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchCheckNumberInvoice : BaseConditionSearchModel
    {
        public long? Branch { get; set; }
        public string BranchID { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string ResultType { get; set; }
        public string ResultValue { get; set; }      
        
        public string Symbols { get; set; }
        public ConditionSearchCheckNumberInvoice()
            : base()
        {

        }

        public ConditionSearchCheckNumberInvoice(string orderBy, string orderType)
            : this()
        {            
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
