using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class UserLevelRepository : GenericRepository<USERLEVEL>, IUserLevelRepository
    {
        public UserLevelRepository(IDbContext context)
            : base(context)
        {
        }

        // <summary>
        /// Get list UserRole by level.
        /// </summary>
        /// <param name="level">The condition get UserRole.</param>
        /// <returns><c>list UserRole </c> if Levels on database, <c>null</c> otherwise</returns>
        public IEnumerable<USERLEVEL> FillterUserLevel(string level)
        {
            return dbSet.Where(p => p.LEVELS.Equals(level));
        }

        public USERLEVEL FillterByLevel(string level)
        {
            return dbSet.FirstOrDefault(p => p.LEVELS.Equals(level));
        }

        public USERLEVEL GetById(long id)
        {
            return dbSet.FirstOrDefault(p => p.USERLEVELSID == id);
        }
    }
}
