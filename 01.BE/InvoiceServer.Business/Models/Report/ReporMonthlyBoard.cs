using InvoiceServer.Common.Constants;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.Models
{
    public class ReporMonthlyBoard
    {
        [JsonProperty("company")]
        public CompanyInfo Company { get; set; }

        [JsonProperty("items")]
        public List<ReportListInvoices> items { get; set; }

        [JsonProperty("month")]
        public int Month { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("totalBeforeTax")]
        public decimal TotalBeforeTax
        {
            get
            {
                if (this.items.Count > 0)
                {
                    return this.items.Where(p => p.IsSummary && p.Code != TaxCodes.noTax).Sum(p => (p.TotalByTax ?? 0));
                }
                else
                {
                    return 0;
                }
            }
        }

        [JsonProperty("totalTax")]
        public decimal TotalOfTax
        {
            get
            {
                if (this.items.Count > 0)
                {
                    return this.items.Where(p => p.IsSummary && p.Code != TaxCodes.noTax).Sum(p => (p.TotalTaxByTax ?? 0));
                }
                else
                {
                    return 0;
                }
            }
        }

        [JsonProperty("total")]
        public decimal Total
        {
            get
            {
                if (this.items.Count > 0)
                {
                    return this.items.Where(p => p.IsSummary).Sum(p => (p.TotalByTax ?? 0));
                }
                else
                {
                    return 0;
                }
            }
        }

        public ReporMonthlyBoard(CompanyInfo company)
        {
            this.Company = company;
        }
    }
}
