namespace InvoiceServer.Business.Models
{
    public class ConditionSearchCA : BaseConditionSearchModel
    {
        public string TaxCode { get; private set; }
        public int? Status { get; private set; }
        public long? CompanyID { get; private set; }
        public long? Branch { get; private set; }
        public ConditionSearchCA(string TaxCode, long? Branch)
        {
            this.TaxCode = TaxCode;
            this.Branch = Branch;
        }

    }
}
