using InvoiceServer.Common.Enums;
using System.Collections.Generic;

namespace InvoiceServer.Common
{
    public static class InvoiceProperties
    {
        public static string getInvoiceStatus(InvoiceStatus status, string language = "vi")
        {
            Dictionary<InvoiceStatus, string> statusVi = new Dictionary<InvoiceStatus, string>()
            {
                { InvoiceStatus.New, "Tạo mới"},
                { InvoiceStatus.Approved, "Đã duyệt"},
                { InvoiceStatus.Releaseding, "Đang chờ ký"},
                { InvoiceStatus.Released, "Đã ký"},
                { InvoiceStatus.Cancel, "Đã hủy"},
                { InvoiceStatus.Delete, "Đã xóa bỏ"},
            };
            Dictionary<InvoiceStatus, string> statusEn = new Dictionary<InvoiceStatus, string>()
            {
                { InvoiceStatus.New, "New"},
                { InvoiceStatus.Approved, "Approved"},
                { InvoiceStatus.Releaseding, "Releaseding"},
                { InvoiceStatus.Released, "Released"},
                { InvoiceStatus.Cancel, "Cancel"},
                { InvoiceStatus.Delete, "Delete"},
            };

            switch (language)
            {
                case "en":
                    return statusEn[status];
                default:
                    return statusVi[status];
            }

        }
    }
}
