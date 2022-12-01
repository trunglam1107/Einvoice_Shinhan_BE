using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ISignDetailRepository : IRepository<SIGNDETAIL>
    {
        /// <summary>
        /// Get an IEnumerable City 
        /// </summary>
        /// <returns><c>IEnumerable City</c> if City not Empty, <c>null</c> otherwise</returns>
        SIGNDETAIL GetByInvoiceId(int invoiceId, bool isClientSign = false);

        SIGNDETAIL GetByAnnouncementId(int announcementId, bool isClientSign = false);
        void InsertMultipe(List<SIGNDETAIL> signDetails);
    }
}
