
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceGiftRepository : IRepository<INVOICEGIFT>
    {
        IEnumerable<INVOICEGIFT> FilterInvoiceGift(long invoiceId);
        /// <summary>
        ///  Lấy danh sách invoicegift của danh sách hóa đơn
        /// </summary>
        /// <param name="invoiceIds"></param>
        /// <returns></returns>
        List<INVOICEGIFT> GetINVOICEGIFTs(List<long> invoiceIds);
    }
}
