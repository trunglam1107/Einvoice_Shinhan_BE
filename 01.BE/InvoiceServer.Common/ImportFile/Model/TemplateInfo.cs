using System;

namespace InvoiceServer.Common.Models
{
    public class TemplateInfo
    {
        public long CompanyId { get; set; }
        public long RegisterTemplateId { get; set; }
        public string Code { get; set; }
        public string Symbol { get; set; }
        public DateTime? UsedDate { get; set; }
    }
}
