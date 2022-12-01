using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class InvoiceStatisticalRepository : GenericRepository<STATISTICAL>, IInvoiceStatisticalRepository
    {
        public InvoiceStatisticalRepository(IDbContext context)
            : base(context)
        {
        }
        public IEnumerable<STATISTICAL> FilterInvoiceStatistical(long invoiceId)
        {
            return dbSet.Where(p => p.INVOICEID == invoiceId);
        }
        public List<STATISTICAL> GetSTATISTICALs(List<long> invoiceIds)
        {
            return dbSet.Where(p => invoiceIds.Contains(p.INVOICEID)).ToList();
        }
    }
}
