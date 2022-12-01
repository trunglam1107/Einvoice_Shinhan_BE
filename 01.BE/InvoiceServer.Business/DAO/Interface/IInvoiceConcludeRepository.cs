using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceConcludeRepository : IRepository<INVOICERELEAS>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<InvoiceReleasesMaster> FilterInvoiceConclude(ConditionSearchInvoiceReleases condition);

        long CountInvoiceConclude(ConditionSearchInvoiceReleases condition);

        INVOICERELEAS GetById(long id);
        List<InvoiceReleasesDetailInfo> GetByNotification(long notiID);
    }
}