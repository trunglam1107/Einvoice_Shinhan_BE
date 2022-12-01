using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceConcludeDetailRepository : IRepository<INVOICERELEASESDETAIL>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<INVOICERELEASESDETAIL> FilterInvoiceConcludeDetail(long invoiceConclude);

        INVOICERELEASESDETAIL GetById(long id);
    }
}
