using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IReportCancellingRepository : IRepository<REPORTCANCELLING>
    {
        /// <summary>
        /// Get an IEnumerable City 
        /// </summary>
        /// <returns><c>IEnumerable City</c> if City not Empty, <c>null</c> otherwise</returns>
        IEnumerable<REPORTCANCELLING> FilterAllReportCancelling();

        IEnumerable<REPORTCANCELLING> FilterReportCancelling(long companyId);

        REPORTCANCELLING GetById(long id, long companyId);
    }
}
