using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public class ReportSelfPrinted
    {
        public CompanyInfo Company { get; set; }
        public List<NoticePrintInvoice> items { get; set; }
        public int Precious { get; set; }
        public int Year { get; set; }
        public ReportSelfPrinted(CompanyInfo company)
        {
            this.Company = company;
        }
    }
}
