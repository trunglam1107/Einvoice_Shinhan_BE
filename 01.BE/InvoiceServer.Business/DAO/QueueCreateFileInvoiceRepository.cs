using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class QueueCreateFileInvoiceRepository : GenericRepository<QUEUECREATEFILEINVOICE>, IQueueCreateFileInvoiceRepository
    {
        public QueueCreateFileInvoiceRepository(IDbContext context)
            : base(context)
        {
        }
        public IEnumerable<QUEUECREATEFILEINVOICE> FilterByStatus(List<long> status)
        {
            return this.dbSet.Where(p => status.Contains((p.PROCESSSTATUS ?? 0)) && !(p.CREATEDFILE ?? false));
        }

        public IEnumerable<QUEUECREATEFILEINVOICE> FilterByInvoicesId(List<long> invoicesId)
        {
            return this.dbSet.Where(p => invoicesId.Contains((p.INVOICEID ?? 0)));
        }
    }
}
