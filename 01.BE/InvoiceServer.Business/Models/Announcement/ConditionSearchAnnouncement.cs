using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ConditionSearchAnnouncement : BaseConditionSearchModel
    {
        public string Denominator { get; set; }

        public string Symbol { get; set; }

        public List<int> AnnouncementStatus { get; set; }

        public string InvoiceNo { get; set; }

        public string ClientName { get; set; }

        public string ClientTaxCode { get; set; }

        public int? AnnouncementType { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public long? Branch { get; set; }
        public string MinutesNo { get; set; }
        public string CustomerCode { get; set; }
        public string Language { get; set; }

        public ConditionSearchAnnouncement(string orderBy, string orderType)
            : base()
        {
            this.AnnouncementStatus = new List<int>();
            string macthOrderBy;
            string macthOrderType;
            Announcementsortcolumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? Announcementsortcolumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }

        public ConditionSearchAnnouncement(string orderBy, string orderType, int? announcementStatus)
            : this(orderBy, orderType)
        {
            if (announcementStatus.HasValue)
            {
                this.AnnouncementStatus.Add(announcementStatus.Value);
            }
        }
    }
}
