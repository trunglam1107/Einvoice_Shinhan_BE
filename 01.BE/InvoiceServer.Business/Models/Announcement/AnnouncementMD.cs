using InvoiceServer.Common.Enums;
using System;

namespace InvoiceServer.Business.Models.Annoucement
{
    public class AnnouncementMD
    {
        public long id { get; set; }
        public string Title { get; set; }
        public string LegalGrounds { get; set; }
        public DateTime AnnouncementDate { get; set; }
        public long CompanyId { get; set; }
        public string BranchId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyNameAnnouncement { get; set; }
        public string CompanyNameMyCompany { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyAddressAnnouncement { get; set; }
        public string CompanyAddressMyCompany { get; set; }
        public string CompanyTel { get; set; }
        public string CompanyTelAnnouncement { get; set; }
        public string CompanyTelMyCompany { get; set; }
        public string CompanyTaxCode { get; set; }
        public string CompanyTaxCodeAnnouncement { get; set; }
        public string CompanyTaxCodeMyCompany { get; set; }
        public string CompanyRepresentative { get; set; }
        public string CompanyPosition { get; set; }
        public string ClientName { get; set; }
        public string InvoiceCode { get; set; }
        public string ClientAddress { get; set; }
        public string ClientTel { get; set; }
        public string ClientTaxCode { get; set; }
        public string ClientRepresentative { get; set; }

        public string ClientPosition { get; set; }
        public long InvoiceId { get; set; }

        public long RegisterTemplateId { get; set; }
        public string Denominator { get; set; }
        public string Symbol { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string ServiceName { get; set; }
        public string Reasion { get; set; }
        public string BeforeContain { get; set; }
        public string ChangeContain { get; set; }
        public int AnnouncementType { get; set; }
        public int AnnouncementStatus { get; set; }

        public long CreatedBy { get; set; }
        public long UpdatedBy { get; set; }

        public long? ApprovedBy { get; set; }

        public long? ProcessBy { get; set; }
        public AnnouncementStatus UpdateStatus { get; set; }
        public string VerificationCode { get; set; }
        public bool? CompanySign { get; set; }
        public DateTime? CompanySignDate { get; set; }
        public string CompanySignature { get; set; }
        public string DateCompanySign { get; set; }
        public bool? ClientSign { get; set; }
        public DateTime? ClientSignDate { get; set; }
        public string DateClientSign { get; set; }
        public string ClientSignature { get; set; }
        public string Signature { get; set; }
        public string MinutesNo { get; set; }
        public string EmailClient { get; set; }
        public SystemSettingInfo SystemSetting { get; set; }
        public string CurrencyCode { get; set; }
    }

    public enum EnumAnnouncementActionType
    {

        ADJUSTMENT = 1,// DIEUCHINH,
        SUBSTITUTE = 2, //THAYTHE,
        CANCEL = 3,//HUY,
        ADDNEW = 4,
        DUYET,
        UPADTE
    }
}
