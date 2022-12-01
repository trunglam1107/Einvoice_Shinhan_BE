using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class AnnouncementConvertedBoard
    {
        [JsonIgnore]
        public CompanyInfo Company { get; set; }

        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public string Title { get; set; }
        [JsonIgnore]
        public string LegalGrounds { get; set; }
        [JsonIgnore]
        public DateTime? AnnouncementDate { get; set; }
        [JsonIgnore]
        public long CompanyId { get; set; }
        [JsonIgnore]
        public string CompanyName { get; set; }
        [JsonIgnore]
        public string CompanyAddress { get; set; }
        [JsonIgnore]
        public string CompanyTel { get; set; }
        [JsonIgnore]
        public string CompanyTaxCode { get; set; }
        [JsonIgnore]
        public string CompanyRepresentative { get; set; }
        [JsonIgnore]
        public string CompanyPosition { get; set; }
        [JsonIgnore]
        public string ClientName { get; set; }
        [JsonIgnore]
        public string ClientAddress { get; set; }
        [JsonIgnore]
        public string ClientTel { get; set; }
        [JsonIgnore]
        public string ClientTaxCode { get; set; }
        [JsonIgnore]
        public string ClientRepresentative { get; set; }
        [JsonIgnore]
        public string ClientPosition { get; set; }
        [JsonIgnore]
        public long InvoiceId { get; set; }
        [JsonIgnore]
        public long RegisterTemplateId { get; set; }
        [JsonIgnore]
        public string Denominator { get; set; }
        [JsonIgnore]
        public string Symbol { get; set; }
        [JsonIgnore]
        public string InvoiceNo { get; set; }
        [JsonIgnore]
        public DateTime InvoiceDate { get; set; }
        [JsonIgnore]
        public decimal? TotalAmount { get; set; }
        [JsonIgnore]
        public string ServiceName { get; set; }
        [JsonIgnore]
        public string Reasion { get; set; }
        [JsonIgnore]
        public string BeforeContain { get; set; }
        [JsonIgnore]
        public string ChangeContain { get; set; }
        [JsonIgnore]
        public int? AnnouncementType { get; set; }
        [JsonIgnore]
        public int? AnnouncementStatus { get; set; }
        [JsonIgnore]
        public long? CreatedBy { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
        [JsonIgnore]
        public long? UpdatedBy { get; set; }
        [JsonIgnore]
        public DateTime UpdatedDate { get; set; }
        [JsonIgnore]
        public long? ApprovedBy { get; set; }
        [JsonIgnore]
        public DateTime? ApprovedDate { get; set; }
        [JsonIgnore]
        public long? ProcessBy { get; set; }
        [JsonIgnore]
        public DateTime? ProcessDate { get; set; }

        public AnnouncementConvertedBoard()
        {
        }

    }
}
