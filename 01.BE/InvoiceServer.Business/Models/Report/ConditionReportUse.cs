using InvoiceServer.Common.Extensions;
using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionReportUse
    {
        public long CompanyId { get; set; }
        public int Precious { get; private set; }
        public int Year { get; private set; }
        public long RegisterTemplateId { get; set; }
        public string Symbol { get; set; }
        public string Language { get; set; }

        public DateTime? DateFrom { get; private set; }
        public DateTime? DateTo { get; private set; }
        public bool isReport { get; set; }
        public int IsMonth { get; set; }
        public long? Branch { get; set; }

        public ConditionReportUse(UserSessionInfo currentUser, int precius, int year, int isMonth = 0, string branch = null, string language = null)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }

            if (branch == "undefined")
            {
                Branch = currentUser.Company.Id.Value;
            }
            else
            {
                this.Branch = branch.ToInt();
            }

            this.Language = language;
            this.Precious = precius;
            this.Year = year;
            this.IsMonth = isMonth;
            if (IsMonth > 0)
            {
                SetDateByMonth();
            }
            else
            {
                SetDateByPrecius();
            }
        }

        public ConditionReportUse(int precius, int year, int isMonth = 0, string language = null,long CompanyId = 0)
        {
            this.CompanyId = CompanyId;
            this.Precious = precius;
            this.Year = year;
            this.IsMonth = isMonth;
            this.Language = language;
            if (IsMonth > 0)
            {
                SetDateByMonth();
            }
            else
            {
                SetDateByPrecius();
            }
        }
        private void SetDateByPrecius()
        {

            switch (this.Precious)
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

        private void SetDateByMonth()
        {
            this.DateFrom = DateTime.Parse(string.Format("{0}-{1}-01", this.Year, this.Precious));
            this.DateTo = this.DateFrom.Value.AddMonths(1);
        }

    }
}
