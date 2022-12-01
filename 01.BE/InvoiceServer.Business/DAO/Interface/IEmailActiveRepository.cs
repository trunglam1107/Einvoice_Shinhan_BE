using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IEmailActiveRepository : IRepository<EMAILACTIVE>
    {
        /// <summary>
        /// Get an IEnumerable EmailActive 
        /// </summary>
        /// <returns><c>IEnumerable EmailActive</c> if City not Empty, <c>null</c> otherwise</returns>
        IEnumerable<EmailActiveInfo> Filter(ConditionSearchEmailActive condition);
        long Count(ConditionSearchEmailActive condition);
        EMAILACTIVE GetById(long id);
        void InsertMailInvoice(EmailInfo emailInfo);
        void AddRange(List<EMAILACTIVE> emailActives);
        long? checkEmailExists(long refId);
        long GetRefIdByEmailActiveId(long emailActiveId);
        List<long> GetRefIdsByEmailActiveId(long emailActiveId);
        Dictionary<long, EmailActiveInfo> GetListInvoiceNoDateFromInvoice(List<long> listEmailId);
        Dictionary<long, EmailActiveInfo> GetListInvoiceNoDateFromAnnouncement(List<long> listEmailId);
    }
}
