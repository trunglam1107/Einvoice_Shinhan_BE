using InvoiceServer.Data.Utils;
using System;

namespace InvoiceServer.Business.Models
{
    public class AnnouncementDownload
    {
        [DataConvert("Id")]
        public long ContractId { get; set; }

        [DataConvert("CustomerId")]
        public long CustomerId { get; set; }

        [DataConvert("No")]
        public string ContractNo { get; set; }

        [DataConvert("PaymentDate")]
        public DateTime ContractDate { get; set; }

        public CompanyInfo CompanyInfo { get; set; }

        public CustomerInfo Customer { get; set; }

        public Decimal NumberInvoice { get; set; }

        public string ItemInvoiceRegister { get; set; }

        public bool CompanySign { get; set; }

        public bool ClientSign { get; set; }

        public long Id { get; set; }
        public string Title { get; set; }
        public string LegalGrounds { get; set; }
        public DateTime AnnouncementDate { get; set; }
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyTel { get; set; }
        public string CompanyTaxCode { get; set; }
        public string CompanyRepresentative { get; set; }
        public string CompanyPosition { get; set; }
        public string ClientName { get; set; }
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
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public long? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public long? ProcessBy { get; set; }
        public DateTime? ProcessDate { get; set; }
        public string Minutesno { get; set; }
        public string CurrencyCode { get; set; }

        //
        public string VerificationCode { get; set; }

        public AnnouncementDownload()
        {

        }
        public AnnouncementDownload(object srcObject)
            : this()
        {
            if (srcObject != null)
            {
                DataObjectConverter.Convert<object, AnnouncementDownload>(srcObject, this);
            }
        }
    }
}
