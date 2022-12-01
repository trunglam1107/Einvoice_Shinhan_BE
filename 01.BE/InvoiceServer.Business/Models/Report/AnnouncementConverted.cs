using InvoiceServer.Business.Utils;
using Newtonsoft.Json;
using System;

namespace InvoiceServer.Business.Models
{
    public class AnnoucementConverted
    {

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("legalGrounds")]
        public string LegalGrounds { get; set; }

        [JsonProperty("annoucementDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? AnnoucementDate { get; set; }

        [JsonProperty("companyId")]
        public long CompanyId { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("companyAddress")]
        public string CompanyAddress { get; set; }

        [JsonProperty("companyTel")]
        public string CompanyTel { get; set; }

        [JsonProperty("companyTaxCode")]
        public string CompanyTaxCode { get; set; }

        [JsonProperty("companyRepresentative")]
        public string CompanyRepresentative { get; set; }

        [JsonProperty("companyPosition")]
        public string CompanyPosition { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("clientAddress")]
        public string ClientAddress { get; set; }

        [JsonProperty("clientTel")]
        public string ClientTel { get; set; }

        [JsonProperty("clientTaxCode")]
        public string ClientTaxCode { get; set; }

        [JsonProperty("clientRepresentative")]
        public string ClientRepresentative { get; set; }

        [JsonProperty("clientPosition")]
        public string ClientPosition { get; set; }

        [JsonProperty("invoiceId")]
        public long InvoiceId { get; set; }

        [JsonProperty("registerTemplateId")]
        public long RegisterTemplateId { get; set; }

        [JsonProperty("denominator")]
        public string Denominator { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("invoiceNo")]
        public string InvoiceNo { get; set; }

        [JsonProperty("invoiceDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? InvoiceDate { get; set; }

        [JsonProperty("totalAmount")]
        public decimal? TotalAmount { get; set; }

        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty("reasion")]
        public string Reasion { get; set; }

        [JsonProperty("beforeContain")]
        public string BeforeContain { get; set; }

        [JsonProperty("changeContain")]
        public string ChangeContain { get; set; }

        [JsonProperty("annoucementType")]
        public int? AnnoucementType { get; set; }

        [JsonProperty("annoucementStatus")]
        public int? AnnoucementStatus { get; set; }

        [JsonProperty("createdBy")]
        public long? CreatedBy { get; set; }

        [JsonProperty("createdDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("updatedBy")]
        public long? UpdatedBy { get; set; }

        [JsonProperty("updatedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime UpdatedDate { get; set; }

        [JsonProperty("approvedBy")]
        public long? ApprovedBy { get; set; }

        [JsonProperty("approvedDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ApprovedDate { get; set; }

        [JsonProperty("processBy")]
        public long? ProcessBy { get; set; }

        [JsonProperty("processDate")]
        [JsonConverter(typeof(JsonDateConverterString))]
        public DateTime? ProcessDate { get; set; }

        public AnnoucementConverted()
        {
        }

    }
}
