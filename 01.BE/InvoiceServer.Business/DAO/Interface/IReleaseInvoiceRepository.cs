using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IReleaseInvoiceRepository : IRepository<RELEASEINVOICE>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<RELEASEINVOICE> FilterInvoice(ReleaseGroupInvoice condition);

        IEnumerable<ReleaseInvoices> FilterReleaseInvoice(ConditionSearchReleaseInvoice condition);

        RELEASEINVOICE GetById(long id);

        RELEASEINVOICE GetById(long id, long companyId);

        IEnumerable<RELEASEINVOICE> SendEmailNotifice(IList<int> status);

        RELEASEINVOICE FilterInvoice(long id, IList<int> status);

        IEnumerable<ReleaseListInvoice> FilterReleaseInvoice(ConditionSearchReleaseList condition);

        IEnumerable<InvoicesReleaseDetail> ReleaseInvoiceDetail(ConditionSearchReleaseListDetail condition);

        RELEASEINVOICE GetByInvoiceId(long invoiceId);

        long? GetCompanyIdByVerifiCode(string VerifiCod);

        IEnumerable<InvoicesReleaseDetail> GetInvoiceByReleasedId(long releaseId);
        IEnumerable<InvoiceMaster> getResponeCQT(long Id);
    }
}
