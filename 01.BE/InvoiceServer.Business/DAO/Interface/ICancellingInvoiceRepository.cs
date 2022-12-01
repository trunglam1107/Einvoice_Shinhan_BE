using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ICancellingInvoiceRepository : IRepository<CANCELLINGINVOICE>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        CANCELLINGINVOICE GetById(int id);

        IEnumerable<CANCELLINGINVOICE> FIlterInvoice(int invoiceId, int companyId);
    }
}
