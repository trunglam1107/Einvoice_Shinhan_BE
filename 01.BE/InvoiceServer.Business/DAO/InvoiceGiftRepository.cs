using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceGiftRepository : GenericRepository<INVOICEGIFT>, IInvoiceGiftRepository
    {
        public InvoiceGiftRepository(IDbContext context)
            : base(context)
        {
        }
        public IEnumerable<INVOICEGIFT> FilterInvoiceGift(long invoiceId)
        {
            return dbSet.Where(p => p.INVOICEID == invoiceId);
        }
        /// <summary>
        ///  Lấy danh sách invoicegift của danh sách hóa đơn
        /// </summary>
        /// <param name="invoiceIds"></param>
        /// <returns></returns>
        public List<INVOICEGIFT> GetINVOICEGIFTs( List<long> invoiceIds)
        {
            return dbSet.Where(p => invoiceIds.Contains(p.INVOICEID)).ToList();
        }
    }
}
