using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IReleaseInvoiceDetaiRepository : IRepository<RELEASEINVOICEDETAIL>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<InvoicesRelease> FilteInvoiceRelease(long releaseInvoiceId, long companyId, List<int> releaseStatus);

        RELEASEINVOICEDETAIL GetById(long id);

        RELEASEINVOICEDETAIL FilteReleaseInvoiceDetail(long releaseId, long invoiceId);

        RELEASEINVOICEDETAIL FilteReleaseInvoiceDetail(long invoiceId);
        IEnumerable<RELEASEINVOICEDETAIL> GetListByReleaseInvoiceID(long releaseId);
        void InsertMultipe(List<RELEASEINVOICEDETAIL> releaseInvoiceDetails);

        /// <summary>
        ///  Lấy danh sách hóa đơn release detail
        /// </summary>
        /// <param name="invoiceIds"></param>
        /// <returns></returns>
        List<RELEASEINVOICEDETAIL> GetByInvoiceIds(List<long> invoiceIds);
    }
}
