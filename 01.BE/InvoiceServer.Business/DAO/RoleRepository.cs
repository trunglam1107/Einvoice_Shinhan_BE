using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class RoleRepository : GenericRepository<ROLE>, IRoleRepository
    {
        public RoleRepository(IDbContext context)
            : base(context)
        {
        }

        // <summary>
        /// Get Role by id.
        /// </summary>
        /// <param name="level">The condition get UserRole.</param>
        /// <returns><c>list UserRole </c> if Levels on database, <c>null</c> otherwise</returns>

        public ROLE GetById(long id)
        {
            return dbSet.FirstOrDefault(p => p.ID == id && !(p.DELETED ?? false));
        }

        public IEnumerable<ROLE> GetRolesByUserId(long userId)
        {
            return this.context.Set<LOGINUSER>().Where(u => u.USERSID == userId)
                    .Where(r => !(r.DELETED ?? false))
                    .FirstOrDefault().ROLEs;
        }
    }
}
