using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchInvoice : BaseConditionSearchModel
    {
        public long? InvoiceSampleId { get; set; }

        public string Symbol { get; set; }

        public List<int> InvoiceStatus { get; set; }

        public string InvoiceNo { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }        //search CIF

        public string PersonContact { get; set; }

        public string TaxCode { get; set; }

        public long? InvoiceSample { get; set; }

        public bool? PaymentStatus { get; set; }

        public string NumberAccout { get; set; }

        public bool IsInvoiceChange { get; set; }
        public bool AnnouncementSearch { get; set; }

        public long? Branch { get; set; }

        public bool? Converted { get; set; }

        public string Language { get; set; }

        public string RefNumber { get; set; }
        public string Teller { get; set; }
        public int? InvoiceNotiType { get; set; }
        public long? ImportType { get; set; }

        public string TaxVAT { get; set; }
        public string BranchID { get; set; }

        public decimal? FromInvoiceNo { get; set; }

        public decimal? ToInvoiceNo { get; set; }
        public ConditionSearchInvoice()
            : base()
        {

        }

        public ConditionSearchInvoice(string orderBy, string orderType)
            : this()
        {
            this.InvoiceStatus = new List<int>();
            string macthOrderBy;
            string macthOrderType;
            InvoiceSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? InvoiceSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }

        public ConditionSearchInvoice(string orderBy, string orderType, int? invoiceStatus)
            : this(orderBy, orderType)
        {
            if (invoiceStatus.HasValue)
            {
                if (invoiceStatus == 999)
                {
                    invoiceStatus = 1;
                }
                this.InvoiceStatus.Add(invoiceStatus.Value);
            }
        }

        public ConditionSearchInvoice(string orderBy, string orderType, List<int> invoiceStatus)
            : this(orderBy, orderType)
        {
            if (invoiceStatus != null && invoiceStatus.Count > 0)
            {
                this.InvoiceStatus = invoiceStatus;
            }
        }
    }
}
