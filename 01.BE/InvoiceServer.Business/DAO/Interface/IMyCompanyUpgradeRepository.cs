using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IMyCompanyUpgradeRepository : IRepository<MYCOMPANYUPGRADE>
    {
        /// <summary>
        /// Get an IEnumerable MYCOMPANYUPGRADE 
        /// </summary>
        /// <returns><c>IEnumerable MYCOMPANYUPGRADE</c> if MYCOMPANYUPGRADE not Empty, <c>null</c> otherwise</returns>
        IEnumerable<MYCOMPANYUPGRADE> GetList();

        /// <summary>
        /// Get an MYCOMPANYUPGRADE by CompanySID.
        /// </summary>
        /// <param name="id">The condition get MyCompany.</param>
        /// <returns><c>Operator</c> if CompanySID on database, <c>null</c> otherwise</returns>
        MYCOMPANYUPGRADE GetByCompanySId(long id);
        MYCOMPANYUPGRADE GetById(long id);
    }
}
