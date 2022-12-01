using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class KeyStoreRepository : GenericRepository<KEYSTORE>, IKeyStoreRepository
    {
        public KeyStoreRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<KEYSTORE> GetList(long companyId)
        {
            return this.dbSet.Where(p => !(p.DELETED ?? false) && p.COMPANYID == companyId);
        }

        public KEYSTORE GetKeyStoreUse(long companyId)
        {
            return this.dbSet.OrderByDescending(p => p.CREATEDDATE).FirstOrDefault(p => !(p.DELETED ?? false) && p.COMPANYID == companyId);
        }
    }
}
