using InvoiceServer.Data.DBAccessor;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class RoleFunctionRepository : GenericRepository<ROLEFUNCTION>, IRoleFunctionRepository
    {
        public RoleFunctionRepository(IDbContext context)
            : base(context)
        {
        }

        // <summary>
        /// Get list UserRole by level.
        /// </summary>
        /// <param name="level">The condition get UserRole.</param>
        /// <returns><c>list UserRole </c> if Levels on database, <c>null</c> otherwise</returns>

        public ROLEFUNCTION GetById(long id)
        {
            return dbSet.FirstOrDefault(p => p.ID == id);
        }

        public ROLEFUNCTION GetRoleOfUser(long roleFunctionId, long userSID)
        {
            return this.dbSet.FirstOrDefault(r => r.FUNCTIONID == roleFunctionId
                    && r.ROLE.LOGINUSERs.Select(u => u.USERSID).Contains(userSID));
        }

    }
}
