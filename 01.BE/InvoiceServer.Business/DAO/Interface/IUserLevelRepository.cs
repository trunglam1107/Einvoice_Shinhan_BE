using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IUserLevelRepository : IRepository<USERLEVEL>
    {
        /// <summary>
        /// Get list UserRole by level.
        /// </summary>
        /// <param name="level">The condition get UserRole.</param>
        /// <returns><c>list UserRole </c> if Levels on database, <c>null</c> otherwise</returns>
        IEnumerable<USERLEVEL> FillterUserLevel(string level);

        USERLEVEL FillterByLevel(string level);

        USERLEVEL GetById(long id);
    }
}
