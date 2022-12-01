using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IAnnouncementRepository : IRepository<ANNOUNCEMENT>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<AnnouncementMaster> FilterAnnouncement(ConditionSearchAnnouncement condition);
        AnnouncementDownload GetAnnouncementPrintInfo(long announcementId, MYCOMPANY companyInfo);
        ANNOUNCEMENT GetByOnlyId(long id);
        ANNOUNCEMENT GetById(long id, long companyId);
        ANNOUNCEMENT GetAnnounApprovedByInvoiceId(long invoiceId);
        ANNOUNCEMENT GetByInvoiceId(long invoiceId, int announType, long? companyId);
        AnnouncementInfo CheckExistAnnoun(long invoiceId, bool invoiceType, int announType);
        bool CheckAnnount(long invoiceId, int type);
        bool getMinus(string MINUTESNO, long? id, long companyId);
        SendEmailVerificationCodeAnnoun GetVerificationCodeInfomation(AnnouncementVerificationCode announcementVerificationCode);
    }
}
