using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public interface IClientRepository : IRepository<CLIENT>
    {
        /// <summary>
        /// Get an IEnumerable Client 
        /// </summary>
        /// <returns><c>IEnumerable Client</c> if Client not Empty, <c>null</c> otherwise</returns>
        IEnumerable<CLIENT> GetList();

        /// <summary>
        /// Get an Client by ClientSID.
        /// </summary>
        /// <param name="id">The condition get Client.</param>
        /// <returns><c>Operator</c> if ClientSID on database, <c>null</c> otherwise</returns>
        CLIENT GetById(long id);

        /// <summary>
        /// Lấy danh sách clients
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<CLIENT> GetByIds(List<long?> ids);


        CLIENT GetByTaxCode(string taxCode);

        CLIENT GetByPersonalContactAndAddress(string personalContact, string address);

        /// <summary>
        /// Get list client by filter
        /// </summary>
        /// <param name="filterCondition"></param>
        /// <returns></returns>
        IQueryable<ClientDetail> Filter(ConditionSearchClient condition);

        List<ClientDetail> FilterByCIF(ConditionSearchClient condition);

        long Count(ConditionSearchClient filterCondition);

        long CountByCIF(ConditionSearchClient filterCondition);

        CLIENT FilterClient(long companyId, string taxCode, string email, string customerName);
        CLIENT FilterClient(string taxCode, string customerName);
        CLIENT FilterClient(long companyId, string taxCode, string customerName);
        CLIENT FilterClient(long companyId, string taxCode, string customerName, string address, int? isOrg);
        bool ContainEmail(string email, long companyId);

        bool ContainTax(string taxCode);

        bool ContainCustomerCode(string customerCode);

        int GetMaxCustomerCode(long clientId);
        bool MyClientUsing(long clientID);
        IQueryable<CLIENT> GetListSendByMonth();
        CLIENT GetByCustomerCode(string customerCode);

        ReceiverAccountActiveInfo GetClientAccountInfo(long clientId);
        IEnumerable<CLIENT> FilterClients(ConditionSearchClient condition);
        CLIENT GetByIdForSendMail(long id);
        List<ClientDetail> ClientListSuggestion(ConditionSearchClient condition);
        CLIENT GetByEmail(string email);

        PTClientAccountInfo GetClient(long? clientId);
    }
}
