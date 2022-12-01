using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public interface ISignatureRepository : IRepository<SIGNATURE>
    {
        /// <summary>
        /// Get an IEnumerable City 
        /// </summary>
        /// <returns><c>IEnumerable City</c> if City not Empty, <c>null</c> otherwise</returns>
        IEnumerable<SIGNATURE> GetList(long companyId);

        SIGNATURE GetSignatureByCompany(SignatureInfo signatureInfo);
        SIGNATURE GetByCompany(long companyId);
        SIGNATURE GetSignatureInfo(long id);
        IEnumerable<SIGNATURE> FilterCA(ConditionSearchCA condition);
        IEnumerable<SIGNATURE> GetListByPassword(string password);
        SIGNATURE GetBySlot(int slot, string cert);
        List<SIGNATURE> GetByCompanies(List<long> companyIds);
        IQueryable<SIGNATURE> GetSignatureActive();
    }
}
