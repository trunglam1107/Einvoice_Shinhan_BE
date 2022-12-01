using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public interface IConfigEmailServerRepository : IRepository<CONFIGEMAILSERVER>
    {
        /// Get an config email server by company Id.
        /// </summary>
        /// <param name="id">The condition get email server.</param>
        /// <returns><c>Operator</c> if Id on database, <c>null</c> otherwise</returns>
        CONFIGEMAILSERVER GetByCompany(long companyId);
        CONFIGEMAILSERVER GetByCompany();
    }
}
