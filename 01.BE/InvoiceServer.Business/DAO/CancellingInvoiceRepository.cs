using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class CancellingInvoiceRepository : GenericRepository<CANCELLINGINVOICE>, ICancellingInvoiceRepository
    {
        public CancellingInvoiceRepository(IDbContext context)
            : base(context)
        {
        }

        public CANCELLINGINVOICE GetById(int id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public IEnumerable<CANCELLINGINVOICE> FIlterInvoice(int invoiceId, int companyId)
        {
            return this.dbSet.Where(p => p.INVOICEID == invoiceId && p.INVOICE.COMPANYID == companyId);
        }
    }
}
