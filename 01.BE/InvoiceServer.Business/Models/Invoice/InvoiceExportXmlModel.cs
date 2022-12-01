using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    [Serializable]
    public class InvoiceExportXmlModel
    {
        public InvoicePrintInfo InvoicePrintInfo { get; set; }
        public InvoicePrintModel InvoicePrintModel { get; set; }
        public PrintConfig PrintConfig { get; set; }

        public SystemSettingInfo SystemSettingInfo { get; set; }

        public InvoiceExportXmlModel()
        {
        }
    }
}
