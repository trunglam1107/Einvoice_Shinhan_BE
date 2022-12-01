using InvoiceServer.Data.DBAccessor;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class ReleaseAnnouncementRepository : GenericRepository<RELEASEANNOUNCEMENT>, IReleaseAnnouncementRepository
    {
        public ReleaseAnnouncementRepository(IDbContext context)
            : base(context)
        {
        }

        public RELEASEANNOUNCEMENT GetById(long id)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id);
        }

        public RELEASEANNOUNCEMENT GetById(long id, long companyId)
        {
            return this.dbSet.FirstOrDefault(p => p.ID == id && p.COMPANYID == companyId);
        }

        public RELEASEANNOUNCEMENT GetByAnnouncementId(long anouncementId)
        {
            var releaseInfo = from release in this.dbSet
                              join releaseDetail in this.context.Set<RELEASEANNOUNCEMENTDETAIL>()
                              on release.ID equals releaseDetail.RELEASEANNOUNCEMENTID
                              where releaseDetail.ANNOUNCEMENTID == anouncementId
                              select release;
            return releaseInfo.FirstOrDefault();
        }

        public long? GetCompanyIdByVerifiCode(string VerifiCod)
        {
            var releaseInfo = from release in this.dbSet
                              join releaseDetail in this.context.Set<RELEASEANNOUNCEMENTDETAIL>()
                              on release.ID equals releaseDetail.RELEASEANNOUNCEMENTID
                              where releaseDetail.VERIFICATIONCODE == VerifiCod
                              select release;
            //var releaseInfo = from invoice in this.context.Set<INVOICE>()
            //                  where invoice.VERIFICATIONCODE == VerifiCod
            //                  select invoice;

            return releaseInfo.FirstOrDefault().COMPANYID;
        }

    }
}
