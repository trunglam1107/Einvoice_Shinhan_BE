using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface INoticeUseInvoiceBO
    {


        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<NotificeUseInvoiceMaster> Filter(ConditionSearchUseInvoice condition);

        /// <summary>
        /// Count number clients by condition
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        long Count(ConditionSearchUseInvoice condition);

        /// <summary>
        /// Add new client
        /// </summary>
        /// <param name="client">client info</param>
        /// <returns></returns>
        ResultCode Create(UseInvoiceInfo info);

        /// <summary>
        /// Update client info
        /// </summary>
        /// <param name="client">client info to update</param>
        /// <returns></returns>
        ResultCode Update(long id, long? companyId, UseInvoiceInfo info);
        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetListNotificationUseDetail(long notificationUseId);

        ResultCode UpdateStatus(long id, long? companyId, InvoiceReleasesStatus status, bool isUnApproved = false);

        /// <summary>
        /// Delete Client By Update Deleted Flag = true
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        ResultCode Delete(long id, long? companyId);

        /// <summary>
        /// Get the client info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        UseInvoiceInfo GetNoticeUseInvoiceInfo(long id, long? companyId, bool isView = false);

        decimal GetStartNumberInvoiceCompanyUse(long companyId, long registerTemplateId, string symbol);

        ExportFileInfo DownloadNotificationXML(long id, CompanyInfo company);
        ExportFileInfo DownloadNotificationPDF(long id, CompanyInfo company);


        IEnumerable<long> GetListCompany();
        List<MYCOMPANY> GetListCompanyShinhan();
        /// <summary>
        /// Danh sách companies cần tào files
        /// </summary>
        /// <returns></returns>
        List<MYCOMPANY> GetListCompanyNeedCreateFiles();

        /// <summary>
        /// Lấy danh sách companies cần ký
        /// </summary>
        /// <returns></returns>
        List<MYCOMPANY> GetListCompanyNeedSignFiles();

        /// <summary>
        /// Lấy danh sách công ty có hóa đơn cần approved
        /// </summary>
        /// <returns></returns>
        List<MYCOMPANY> GetListCompanyNeedApproved();
        List<MYCOMPANY> GetListCompanySendMails();
        List<CompanySymbolInfo> GetSymbolCompanyForSignJob();
    }
}
