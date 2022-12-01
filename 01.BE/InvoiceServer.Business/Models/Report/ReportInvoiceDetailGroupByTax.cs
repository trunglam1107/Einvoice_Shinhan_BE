namespace InvoiceServer.Business.Models
{
    public class ReportInvoiceDetailGroupByTax
    {
        public ReportInvoiceDetailGroupByTax()
        {
        }

        public int NumberTax0 { get; set; }
        public int NumberTax5 { get; set; }
        public int NumberTax10 { get; set; }
        public int NumberNoTax { get; set; }
        public decimal Amount0 { get; set; }
        public decimal Amount5 { get; set; }
        public decimal Amount10 { get; set; }
        public decimal AmountNoTax { get; set; }
        public decimal TaxAmount5 { get; set; }
        public decimal TaxAmount10 { get; set; }

        public string StrCountTax_0_5_10_NoTax
        {
            get
            {
                return $"{NumberTax0}_{NumberTax5}_{NumberTax10}_{NumberNoTax}";
            }
        }

        public decimal GetTaxAmount()
        {
            return TaxAmount5 + TaxAmount10;
        }

        public bool DiffTaxForTemplateOneTax(ReportInvoiceDetailGroupByTax other)
        {
            var thisTax = NumberTax0 != 0 ? 0
                        : NumberTax5 != 0 ? 5
                        : NumberTax10 != 0 ? 10
                        : -1;
            var otherTax = other.NumberTax0 != 0 ? 0
                        : other.NumberTax5 != 0 ? 5
                        : other.NumberTax10 != 0 ? 10
                        : -1;
            return thisTax != otherTax;
        }
    }
}
