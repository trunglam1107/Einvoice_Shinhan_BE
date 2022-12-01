using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ITaxRepository : IRepository<TAX>
    {
        /// <summary>
        /// Get list UserRole by level.
        /// </summary>
        /// <param name="level">The condition get UserRole.</param>
        /// <returns><c>list UserRole </c> if Levels on database, <c>null</c> otherwise</returns>
        IEnumerable<TAX> GetList();

        TAX GetTax(decimal value);

        TAX GetById(long id);

        TAX GetTax(string name);

        TAX GetTaxBYCODE(string code);

    }
}
