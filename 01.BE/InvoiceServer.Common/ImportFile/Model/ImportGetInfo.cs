namespace InvoiceServer.Common.Models
{
    public class ImportGetInfo
    {
        public long CompanySID { get; set; }
        public string BranchId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyTaxCode { get; set; }
    }
}
