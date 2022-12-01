using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ConditionReportDetailUse : BaseConditionSearchModel
    {
        public long? InvoiceSampleId { get; set; }
        public string Symbol { get; set; }
        public List<int> InvoiceStatus { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public DateTime? DateFromJob { get; set; }

        public DateTime? DateToJob { get; set; }
        public long? Branch { get; set; }
        public string CustomerCode { get; set; }
        public long? CurrencyId { get; set; }
        public long? TaxId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int IsMonth { get; set; }
        public string Language { get; set; }
        public int Time { get; set; }
        public int TimeNo { get; set; }

        public int TimesUpdate { get; set; }
        public string StatusSendTVan { get; set; }

        public List<long> listInvoiceId { get; set; }

        public bool? IsSkipTake { get; set; }

        public string Filename { get; set; }
        public int SBTHDulieu { get; set; }
        public ConditionReportDetailUse(string orderBy, string orderType)
            : base()
        {
            this.InvoiceStatus = new List<int>();
            string macthOrderBy;
            string macthOrderType;
            ReportDetailUseSortColumn.OrderByColumn.TryGetValue(orderBy.EmptyNull().ToUpperInvariant(), out macthOrderBy);
            OrderTypeConst.DcType.TryGetValue(orderType.EmptyNull().ToUpperInvariant(), out macthOrderType);
            macthOrderBy = macthOrderBy ?? ReportDetailUseSortColumn.OrderByColumnDefault;
            macthOrderType = macthOrderType ?? OrderTypeConst.Desc;
            this.ColumnOrder = macthOrderBy;
            this.OrderType = macthOrderType;
        }

        public ConditionReportDetailUse(string orderBy, string orderType, string dateTo)
            : this(orderBy, orderType)
        {
            this.DateTo = dateTo.DecodeUrl().ConvertDateTime();
            if (this.DateTo.HasValue)
            {
                this.DateTo = ((DateTime)this.DateTo).Date.AddDays(1).AddTicks(-1);
            }
        }

        public ConditionReportDetailUse(string orderBy, string orderType, int month, int year, int isMonth = 0, long? branch = null, string language = null)
            : this(orderBy, orderType)
        {
            this.Month = month;
            this.Year = year;
            this.IsMonth = isMonth;
            this.Branch = branch;
            this.Language = language;
            if (isMonth > 0)
            {
                BuildDateFormDateTo();
            }
            else
            {
                SetDateByPrecius();
            }
        }

        private void SetDateByPrecius()
        {

            switch (this.Month)
            {
                case 1:
                    this.DateFrom = DateTime.Parse(string.Format("{0}-{1}-01", this.Year, 1));
                    break;
                case 2:
                    this.DateFrom = DateTime.Parse(string.Format("{0}-{1}-01", this.Year, 4));
                    break;
                case 3:
                    this.DateFrom = DateTime.Parse(string.Format("{0}-{1}-01", this.Year, 7));
                    break;
                case 4:
                    this.DateFrom = DateTime.Parse(string.Format("{0}-{1}-01", this.Year, 10));
                    break;
                default:
                    this.DateFrom = DateTime.Now;
                    break;

            }

            this.DateTo = this.DateFrom.Value.AddMonths(3);
        }

        private void BuildDateFormDateTo()
        {
            
            this.DateFrom = new DateTime(this.Year, this.Month, 1);
            this.DateTo = this.DateFrom.Value.AddMonths(1);

            //Date for job
            //this.DateFromJob = new DateTime(this.Year, this.Month - 1, DateTime.Now.Day - 7);
            //this.DateFromJob = new DateTime(this.Year, this.Month - 1, 1);
            //this.DateToJob = DateTime.Now.AddDays(-7);
            //this.DateToJob = this.DateFromJob.Value.AddMonths(1);
        }
    }
}
