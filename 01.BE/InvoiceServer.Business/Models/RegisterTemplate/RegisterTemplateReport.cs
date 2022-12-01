using InvoiceServer.Common;

namespace InvoiceServer.Business.Models
{
    public class RegisterTemplateReport
    {
        public long? CompanyId { get; set; }

        public string InvoiceSampleName { get; set; }

        [StringTrimAttribute]
        public long RegisterTemplatesId { get; set; }

        [StringTrimAttribute]
        public string RegisterTemplatesCode { get; set; }

        [StringTrimAttribute]
        public string NotificationUseCode { get; set; }

        [StringTrimAttribute]
        public string TypeCodeInvoice { get; set; }

        public RegisterTemplateReport()
        {
        }
    }
}
