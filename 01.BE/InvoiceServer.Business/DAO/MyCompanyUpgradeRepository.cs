using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class MyCompanyUpgradeRepository : GenericRepository<MYCOMPANYUPGRADE>, IMyCompanyUpgradeRepository
    {
        public MyCompanyUpgradeRepository(IDbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Get an IEnumerable MyCompany 
        /// </summary>
        /// <returns><c>IEnumerable MyCompany</c> if MyCompany not Empty, <c>null</c> otherwise</returns>
        public IEnumerable<MYCOMPANYUPGRADE> GetList()
        {
            return dbSet.Where(p => p.COMPANYSID > 0);
        }

        /// <summary>
        /// Get an MyCompany by CompanySID.
        /// </summary>
        /// <param name="id">The condition get MyCompany.</param>
        /// <returns><c>Operator</c> if CompanySID on database, <c>null</c> otherwise</returns>
        public MYCOMPANYUPGRADE GetByCompanySId(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.COMPANYSID == id);
        }
        public MYCOMPANYUPGRADE GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }
    }


}
