using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    [Serializable]
    public class InvoiceExportModel
    {
        public InvoicePrintInfo InvoicePrintInfo { get; set; }
        public InvoicePrintModel InvoicePrintModel { get; set; }
        public List<STATISTICAL> InvoiceStatisticals { get; set; }

        public List<INVOICEGIFT> InvoiceGift { get; set; }

        public PrintConfig PrintConfig { get; set; }

        public int NumberLineOfPage { get; set; }

        public int NumberRecord { get; set; }

        public string DetailInvoice { get; set; }

        public decimal LocationSignLeft { get; set; }

        public decimal LocationSignButton { get; set; }

        public int NumberCharacterOfLine { get; set; }

        public bool IsShowOrder { get; set; }

        public string HeaderInvoice { get; set; }

        public string FooterInvoice { get; set; }

        public UserSessionInfo UserSessionInfo { get; set; }

        public SystemSettingInfo SystemSetting { get; set; }

        public string TemplateFileName { get; set; }

        public InvoiceExportModel()
        {
            this.IsShowOrder = false;
            this.HeaderInvoice = "";
            this.FooterInvoice = "";
            this.UserSessionInfo = null;
            this.SystemSetting = null;
        }
    }
}
