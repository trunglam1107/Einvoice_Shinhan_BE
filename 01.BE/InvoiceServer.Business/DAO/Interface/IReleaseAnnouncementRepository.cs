using InvoiceServer.Data.DBAccessor;

namespace InvoiceServer.Business.DAO
{
    public interface IReleaseAnnouncementRepository : IRepository<RELEASEANNOUNCEMENT>
    {
        RELEASEANNOUNCEMENT GetById(long id);

        RELEASEANNOUNCEMENT GetById(long id, long companyId);

        RELEASEANNOUNCEMENT GetByAnnouncementId(long anouncementId);

        long? GetCompanyIdByVerifiCode(string VerifiCod);

    }
}
