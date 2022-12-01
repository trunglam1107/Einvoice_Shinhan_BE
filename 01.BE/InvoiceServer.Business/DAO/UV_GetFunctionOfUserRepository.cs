using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class UvGetFunctionOfUserRepository : GenericRepository<UV_GETFUNCTIONOFUSER>, IUVGetFunctionOfUserRepository
    {
        public UvGetFunctionOfUserRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<UV_GETFUNCTIONOFUSER> GetByUserSID(long UserSID, long? companySID)
        {
            return this.dbSet.Where(x => x.USERSID == UserSID)
                        .Where(x => x.COMPANYID == companySID);
        }
    }
}
