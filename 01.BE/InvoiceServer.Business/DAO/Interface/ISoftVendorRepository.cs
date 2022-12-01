using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public interface ISoftVendorRepository : IRepository<SOFTVENDOR>
    {
        /// <summary>
        /// Get an IEnumerable City 
        /// </summary>
        /// <returns><c>IEnumerable City</c> if City not Empty, <c>null</c> otherwise</returns>
        SOFTVENDOR GetSoftVenderInfo();
    }
}
