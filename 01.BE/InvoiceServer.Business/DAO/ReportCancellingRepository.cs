using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ReportCancellingRepository : GenericRepository<REPORTCANCELLING>, IReportCancellingRepository
    {
        public ReportCancellingRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<REPORTCANCELLING> FilterAllReportCancelling()
        {
            return this.dbSet;
        }

        public IEnumerable<REPORTCANCELLING> FilterReportCancelling(long companyId)
        {
            return this.dbSet.Where(p => p.COMPANYID == companyId);
        }

        public REPORTCANCELLING GetById(long id, long companyId)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id && p.COMPANYID == companyId);
        }
    }
}
