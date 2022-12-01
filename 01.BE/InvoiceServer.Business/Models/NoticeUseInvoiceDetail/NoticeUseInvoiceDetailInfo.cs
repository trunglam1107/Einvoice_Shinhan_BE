using System;

namespace InvoiceServer.Business.Models
{
    public class NoticeUseInvoiceDetailInfo
    {
        public long Id { get; set; }
        public Nullable<long> NotificationId { get; set; }
        public Nullable<long> RegistertemplateId { get; set; }
        public string PREFIX { get; set; }
        public string SUFFIX { get; set; }
        public string CODE { get; set; }
        public string NUMBERFROM { get; set; }
        public string NUMBERTO { get; set; }
    }
}
