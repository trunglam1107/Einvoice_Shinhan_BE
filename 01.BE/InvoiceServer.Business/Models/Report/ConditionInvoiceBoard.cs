using System;

namespace InvoiceServer.Business.Models
{
    public class ConditionInvoiceBoard
    {
        public long CompanyId { get; set; }
        public long? InvoiceSampleId { get; set; }
        public DateTime DateFrom { get; private set; }
        public DateTime DateTo { get; private set; }
        public string Language { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }
        public int IsMonth { get; set; }
        public long? Branch { get; set; }
        public long TaxID { get; set; }

        public ConditionInvoiceBoard(UserSessionInfo currentUser, long? invoiceSampleId, int month, int year, int isMonth = 0, long? branch = null, string language = null)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }
            this.Branch = branch;
            this.Language = language;

            this.InvoiceSampleId = invoiceSampleId;
            this.IsMonth = isMonth;
            this.Year = year;
            this.Month = month;

            if (IsMonth > 0)
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
            this.DateTo = this.DateFrom.AddMonths(3);
        }

        private void BuildDateFormDateTo()
        {
            this.DateFrom = new DateTime(this.Year, this.Month, 1);
            this.DateTo = this.DateFrom.AddMonths(1);
        }
    }
}
