using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceConcludeDetailRepository : GenericRepository<INVOICERELEASESDETAIL>, IInvoiceConcludeDetailRepository
    {
        public InvoiceConcludeDetailRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<INVOICERELEASESDETAIL> FilterInvoiceConcludeDetail(long invoiceConclude)
        {
            return this.GetInvoiceConcludeActive().Where(p => p.INVOICECONCLUDEID == invoiceConclude);
        }

        private IQueryable<INVOICERELEASESDETAIL> GetInvoiceConcludeActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }

        public INVOICERELEASESDETAIL GetById(long id)
        {
            return GetInvoiceConcludeActive().FirstOrDefault(p => p.ID == id);
        }

    }
}
