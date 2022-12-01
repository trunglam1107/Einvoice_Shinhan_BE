using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models.HistoryReportGeneral
{
    public  class FilterHistoryReportGeneralCondition : BaseConditionSearchModel
    {
        public FilterHistoryReportGeneralCondition()
        {

        }
        public FilterHistoryReportGeneralCondition(int monthOrQuarterNumber, bool isMonth, int year,
            int additionalTimes)
        {
            this.IsMonth = IsMonth;
            this.MonthOrQuarterNumber = monthOrQuarterNumber;
            this.Year = year;
            this.AdditionalTimes = additionalTimes;
        }

        public bool IsMonth { get; set; }
        public int MonthOrQuarterNumber { get; set; }

        public int Year { get; set; }

        public int AdditionalTimes { get; set; }

        public long? CompanyId { get; set; }

        public string  BranchID { get; set; }
        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int Status { get; set; }

        public int ReportType { get; set; }

        public FilterHistoryReportGeneralCondition(string orderBy, string orderType)
            : base()
        {
            string macthOrderBy;
            string macthOrderType;
            Common.Constants.HistoryReportGeneralInfo.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? GatewaylogManagementInfo.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }

    }
}
