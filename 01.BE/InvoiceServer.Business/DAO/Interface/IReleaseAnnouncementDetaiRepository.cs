using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IReleaseAnnouncementDetaiRepository : IRepository<RELEASEANNOUNCEMENTDETAIL>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<AnnouncementsRelease> FilteAnnouncementRelease(long releaseAnnouncementId, long companyId, List<int> releaseStatus);

        RELEASEANNOUNCEMENTDETAIL FilteReleaseAnnouncementDetail(long releaseId, long announcementId);
        RELEASEANNOUNCEMENTDETAIL GetReleaseAnnouncementDetail(long announcementId);
    }
}
