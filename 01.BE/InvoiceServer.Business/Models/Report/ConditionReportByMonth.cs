namespace InvoiceServer.Business.Models
{
    public class ConditionReportByMonth
    {
        public long CompanyId { get; private set; }
        public int Month { get; private set; }
        public int Year { get; private set; }

        public ConditionReportByMonth(UserSessionInfo currentUser, int month, int year)
        {
            if (currentUser.Company.Id.HasValue)
            {
                this.CompanyId = currentUser.Company.Id.Value;
            }

            this.Month = month;
            this.Year = year;
        }
    }
}
