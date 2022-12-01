using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IRoleRepository : IRepository<ROLE>
    {
        /// <summary>
        /// Get list UserRole by level.
        /// </summary>
        /// <param name="level">The condition get UserRole.</param>
        /// <returns><c>list UserRole </c> if Levels on database, <c>null</c> otherwise</returns>

        ROLE GetById(long id);

        IEnumerable<ROLE> GetRolesByUserId(long userId);
    }
}
