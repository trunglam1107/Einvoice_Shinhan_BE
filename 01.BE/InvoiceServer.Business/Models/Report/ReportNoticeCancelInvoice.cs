using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReportNoticeCancelInvoice
    {
        public CompanyInfo Company { get; set; }
        public List<NoticeCancelInvoice> items { get; set; }
        public string Recipients { get; set; }
        public string TypeCanel { get; set; }
        public ReportNoticeCancelInvoice(CompanyInfo company)
        {
            this.Company = company;
        }
    }
}
