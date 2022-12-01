using System;

namespace InvoiceServer.Business.Models
{
    public class SendEmailVerificationCode
    {
        public string VerificationCode { get; set; }
        public long EmailActiveId { get; set; }
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerEmail { get; set; }
        public string PersonContact { get; set; }
        public string InvoiceCode { get; set; }

        public string Symbol { get; set; }
        public string No { get; set; }
        public string InvoiceNo { get; set; }

        public DateTime InvoiceDate { get; set; }

        public decimal Total { get; set; }

        public decimal TotalTax { get; set; }

        public decimal Sum { get; set; }

        public string EmailTo { get; set; }

        public long InvoiceId { get; set; }

        public string NumberFormat { get; set; }

        public string ClientUser { get; set; }
        public string ClientPassword { get; set; }
        public bool IsOrg { get; set; }
        public string TaxCode { get; set; }
        public string Identity { get; set; }
        public long AnnouncementId { get; set; }
        public string AnnouncementTitle { get; set; }
        public int AnnouncementType { get; set; }
        public string Ression { get; set; }
        public string BeforeContain { get; set; }
        public string ChangeContain { get; set; }
        public SystemSettingInfo SystemSetting { get; set; }


        public SendEmailVerificationCode()
        {

        }
    }
}
