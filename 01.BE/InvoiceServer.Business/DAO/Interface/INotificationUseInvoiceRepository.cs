using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface INotificationUseInvoiceRepository : IRepository<NOTIFICATIONUSEINVOICE>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<NotificeUseInvoiceMaster> Filter(ConditionSearchUseInvoice condition);

        long Count(ConditionSearchUseInvoice condition);

        NOTIFICATIONUSEINVOICE GetById(long id);

        UseInvoiceInfo GetNoticeUseInvoiceInfo(long id);

        IEnumerable<long> GetListCompanyUse();

        long GetTotalNumberRegisterBySuffix(string suffix);

        long GetTotalRegisterBySuffixExceptCurrentNotice(string suffix, long noticeId);
        List<MYCOMPANY> GetListCompanyUseShinhan();
        /// <summary>
        /// Danh sách companies cần tào files
        /// </summary>
        /// <returns></returns>
        List<MYCOMPANY> GetListCompanyNeedCreateFiles();

        /// <summary>
        /// Lấy danh sách companies đã tạo files
        /// </summary>
        /// <returns></returns>
        List<MYCOMPANY> GetListCompanyCreatedFiles();

        /// <summary>
        /// Lấy danh sách công ty có hóa đơn cần approved
        /// </summary>
        /// <returns></returns>
        List<MYCOMPANY> GetListCompanyNeedApproved();
        List<MYCOMPANY> GetListCompanySendMails();
        List<CompanySymbolInfo> GetSymbolCompanyForSign();
    }
}
