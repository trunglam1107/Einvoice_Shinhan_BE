using InvoiceServer.Data.DBAccessor;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class SoftVendorRepository : GenericRepository<SOFTVENDOR>, ISoftVendorRepository
    {
        public SoftVendorRepository(IDbContext context)
            : base(context)
        {
        }

        public SOFTVENDOR GetSoftVenderInfo()
        {
            return this.dbSet.FirstOrDefault();
        }
    }
}
