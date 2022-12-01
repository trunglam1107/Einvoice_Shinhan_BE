using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ReleaseAnnouncementDetailRepository : GenericRepository<RELEASEANNOUNCEMENTDETAIL>, IReleaseAnnouncementDetaiRepository
    {
        public ReleaseAnnouncementDetailRepository(IDbContext context)
            : base(context)
        {
        }

        public IEnumerable<AnnouncementsRelease> FilteAnnouncementRelease(long releaseAnnouncementId, long companyId, List<int> releaseStatus)
        {
            var announcementsRelease = from releaseDetail in this.dbSet
                                       join announcement in this.context.Set<ANNOUNCEMENT>()
                                       on releaseDetail.ANNOUNCEMENTID equals announcement.ID
                                       join registerTempalate in this.context.Set<REGISTERTEMPLATE>()
                                       on announcement.REGISTERTEMPLATEID equals registerTempalate.ID
                                       join tempalate in this.context.Set<INVOICETEMPLATE>()
                                       on registerTempalate.INVOICETEMPLATEID equals tempalate.ID
                                       where releaseDetail.RELEASEANNOUNCEMENTID == releaseAnnouncementId && releaseDetail.RELEASEANNOUNCEMENT.COMPANYID == companyId
                                       && releaseStatus.Contains(releaseDetail.RELEASEANNOUNCEMENT.STATUS ?? 0)
                                       select new AnnouncementsRelease()
                                       {
                                           AnnouncementId = (releaseDetail.ANNOUNCEMENTID ?? 0),
                                           Signed = (releaseDetail.SIGNED ?? false),
                                           InvoiceId = announcement.INVOICEID,
                                           CompanySignDate = (announcement.COMPANYSIGNDATE ?? DateTime.Now),
                                           //Printed = (releaseDetail.PRINTED ?? false),
                                           //LocationSignLeft = tempalate.LOCATIONSIGNLEFT,
                                           //LocationSignRight = (tempalate.LOCATIONSIGNRIGHT ?? 0),
                                           //LocationSignButton = tempalate.LOCATIONSIGNBUTTON,

                                       };
            return announcementsRelease;
        }

        public RELEASEANNOUNCEMENTDETAIL FilteReleaseAnnouncementDetail(long releaseId, long announcementId)
        {
            return this.dbSet.FirstOrDefault(p => p.RELEASEANNOUNCEMENTID == releaseId && p.ANNOUNCEMENTID == announcementId);
        }
        public RELEASEANNOUNCEMENTDETAIL GetReleaseAnnouncementDetail(long announcementId)
        {
            return this.dbSet.FirstOrDefault(p => p.ANNOUNCEMENTID == announcementId);
        }
    }
}
