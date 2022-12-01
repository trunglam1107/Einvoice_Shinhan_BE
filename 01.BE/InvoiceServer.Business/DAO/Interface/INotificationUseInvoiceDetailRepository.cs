using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface INotificationUseInvoiceDetailRepository : IRepository<NOTIFICATIONUSEINVOICEDETAIL>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> Filter(long invoiceUseId);

        NOTIFICATIONUSEINVOICEDETAIL GetById(long id);

        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> FilterInvoiceSymbol(long companyId, long invoiceTemplateId);

        bool ContainSymbol(long companyId, string code);

        IEnumerable<NoticeCancelInvoice> FilterCancelInvoice(ConditionReportCancelInvoice codition);

        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetAllNotificationUseInvoiceDetails(long RegisterTemplatesId, string symbol, DateTime? dateTo);

        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNotificationUseInvoiceDetails(long companyId, long RegisterTemplatesId, string symbol, DateTime? dateTo);
        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNotificationUseInvoiceDetailsRpUs(long companyId, long RegisterTemplatesId, string symbol, DateTime? dateFrom, DateTime? dateTo);

        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNotificationUseInvoiceDetails(long companyId, long RegisterTemplatesId, string symbol);

        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNoticeUseInvoiceDetailOfCompany(long companyId);

        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetAllNoticeUseInvoiceDetail();

        IEnumerable<RegisterTemplateCancelling> FilterNotificationUseInvoiceDetailsUse(long companyId, long invoiceSampleId);

        IEnumerable<RegisterTemplateReport> FilterNotificationUseInvoiceDetailsUseReport(long companyId, long? branch, DateTime dateFrom, DateTime? dateTo);

        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNumberInvoiceRegisterBetweenDate(long companyId, long registerId, string symbol, DateTime? dateFrom, DateTime? dateTo);
        IEnumerable<NOTIFICATIONUSEINVOICEDETAIL> GetNumberInvoiceRegisterBetweenDateReport(long companyId, long registerId, string symbol, DateTime? dateFrom, DateTime? dateTo);
        NOTIFICATIONUSEINVOICEDETAIL GetByCompanyId(long companyId, long registerTemplatesId);
        List<NoticeUseInvoiceDetailInfo> GetByRelease(long releaseId);
        bool UsedByInvoices(long NotiId);

    }
}
