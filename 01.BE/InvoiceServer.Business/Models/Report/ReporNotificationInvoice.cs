namespace InvoiceServer.Business.Models
{
    public class ReporNotificationInvoice
    {
        public CompanyInfo Company { get; private set; }
        public UseInvoiceInfo NoticeInvoice { get; set; }

        public ReporNotificationInvoice(CompanyInfo companyInfo)
        {
            this.Company = companyInfo;
        }
    }
}
