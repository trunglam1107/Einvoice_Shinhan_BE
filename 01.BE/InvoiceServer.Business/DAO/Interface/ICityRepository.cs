using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ICityRepository : IRepository<CITY>
    {
        /// <summary>
        /// Get an IEnumerable City 
        /// </summary>
        /// <returns><c>IEnumerable City</c> if City not Empty, <c>null</c> otherwise</returns>
        IEnumerable<CITY> GetList();

        CITY GetById(long id);
        CITY GetByName(string Name);
    }
}
