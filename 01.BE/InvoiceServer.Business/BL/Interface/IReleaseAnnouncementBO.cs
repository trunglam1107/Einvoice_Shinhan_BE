using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IReleaseAnnouncementBO
    {
        ServerSignAnnounResult SignAnnounFile(ReleaseAnnouncementMaster releaseAnnouncement, UserSessionInfo currentUser);

        ServerSignAnnounResult SignAnnounFileMultipleCancel(ReleaseAnnouncementMaster releaseAnnouncement, UserSessionInfo currentUser);

        Result Create(ReleaseAnnouncementMaster info, ReleaseAnnouncementStatus status = ReleaseAnnouncementStatus.New);

        IEnumerable<AnnouncementsRelease> FilterReleaseAnnouncement(long releaseId, long companyId, List<int> releaseStatus);

        RELEASEANNOUNCEMENT GetReleaseAnnouncementByAnnouncementId(long announcementId);

        ResultCode UpdateStatusReleaseAnnouncementDetail(long releaseId, StatusReleaseAnnouncement statusReleaseAnnouncement, UserSessionInfo currentUser);

        ResultCode Update(long id, long companyId, StatusRelease statusRelease);

        RELEASEANNOUNCEMENTDETAIL GetReleaseAnnouncementDetail(long AnnouId);
    }
}
