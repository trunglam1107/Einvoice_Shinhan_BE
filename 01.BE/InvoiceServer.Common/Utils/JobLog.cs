using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Common
{
    public class JobLog
    {
        [ThreadStatic] public static int TotalInvoice = 0;

        /// <summary>
        /// Đếm số hóa đơn ký thành công
        /// </summary>private IList<Item> _myList;
        [ThreadStatic] private static IList<InvoiceJobInfo> invoiceSuccess;
        public static IList<InvoiceJobInfo> InvoiceSuccess
        {
            get { return invoiceSuccess ?? (invoiceSuccess = new List<InvoiceJobInfo>()); }
            set { invoiceSuccess = value; }
        }

        /// <summary>
        /// Đếm số hóa đơn ký lỗi
        /// </summary>
        [ThreadStatic] private static IList<InvoiceJobInfo> invoiceFail;
        public static IList<InvoiceJobInfo> InvoiceFail
        {
            get { return invoiceFail ?? (invoiceFail = new List<InvoiceJobInfo>()); }
            set { invoiceFail = value; }
        }

        public static void AddSuccess(long id, string no, string refNumber, string description)
        {
            if (InvoiceSuccess == null)
                InvoiceSuccess = new List<InvoiceJobInfo>();

            if (!InvoiceSuccess.Any(x => x.Id == id))
            {
                var invoiceJobInfo = new InvoiceJobInfo() { Id = id, No = no, RefNumber = refNumber, Description = description };
                InvoiceSuccess.Add(invoiceJobInfo);
            }
        }

        public static void AddFail(long id, string no, string refNumber, string description)
        {
            if (InvoiceFail == null)
                InvoiceFail = new List<InvoiceJobInfo>();

            if (!InvoiceFail.Any(x => x.Id == id))
            {
                var invoiceJobInfo = new InvoiceJobInfo() { Id = id, No = no, RefNumber = refNumber, Description = description };
                InvoiceFail.Add(invoiceJobInfo);
            }
        }

        public static void Clear()
        {
            TotalInvoice = 0;

            if (InvoiceSuccess == null)
                InvoiceSuccess = new List<InvoiceJobInfo>();
            else
                InvoiceSuccess.Clear();

            if (InvoiceFail == null)
                InvoiceFail = new List<InvoiceJobInfo>();
            else
                InvoiceFail.Clear();
        }
    }

    public class InvoiceJobInfo
    {
        public long Id { get; set; }
        public string No { get; set; }
        public string RefNumber { get; set; }
        public string Description { get; set; }
    }
}
