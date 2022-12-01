using InvoiceServer.Data.DBAccessor;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ConfigEmailServerRepository : GenericRepository<CONFIGEMAILSERVER>, IConfigEmailServerRepository
    {
        public ConfigEmailServerRepository(IDbContext context)
            : base(context)
        {
        }

        public CONFIGEMAILSERVER GetByCompany(long companyId)
        {
            return this.dbSet.FirstOrDefault();
        }
        public CONFIGEMAILSERVER GetByCompany()
        {
            return this.dbSet.FirstOrDefault();
        }
    }
}
