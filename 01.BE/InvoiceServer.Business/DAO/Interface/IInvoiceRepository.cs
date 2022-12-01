using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceRepository : IRepository<INVOICE>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<InvoiceMaster> FilterInvoice(ConditionSearchInvoice condition);

        IEnumerable<InvoiceMaster> FilterInvoiceAnnoun(ConditionSearchInvoice condition);

        IEnumerable<ReleaseInvoiceInfo> GetListInvoiceNo(List<ReleaseInvoiceInfo> releaseInvoiceInfo);
        ReleaseInvoiceInfo GetInvoiceReleaseInfoById(long invoiceId);
        IEnumerable<ReleaseInvoiceInfo> GetAllInvoiceApproved();
        IEnumerable<InvoiceMaster> FilterInvoiceV2(ConditionSearchInvoice condition);
        IQueryable<INVOICE> GetAllInvoiceNewShinhan(long companyId, bool isOrg = true);
        IQueryable<INVOICE> GetAllInvoiceCreatedFileShinhan(long companyId, bool isOrg);
        IEnumerable<ReleaseInvoiceInfo> GetInvoiceApprovedByBranch(long branch);

        IEnumerable<InvoiceMaster> FilterInvoiceCreated(ConditionSearchInvoice condition, int? flgCreate = 0);

        IEnumerable<InvoiceMaster> FillterInvoiceConverted(ConditionSearchInvoice condition);
        long CountV2(ConditionSearchInvoice condition);

        IEnumerable<ReplacedInvoice> FilterReplacedInvoice(SearchReplacedInvoice condition);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="flgFill">
        /// 0: Filter, 1: FillterCreated, 2: FillterInvoiceConverted, 3: FillterInvoiceOfClient
        /// </param>
        /// <returns></returns>
        long Count(ConditionSearchInvoice condition);
        long CountCreated(ConditionSearchInvoice condition, int? flgCreate);
        long CountConverted(ConditionSearchInvoice condition);
        long CountReplace(SearchReplacedInvoice condition);
        int GetNumberInvoiceUsed(ConditionReportUse condition);

        INVOICE GetById(long id);

        INVOICE GetInvoiceBySymbolReleaseddateInvoiceno(DateTime releaseddate, decimal invoiceno , string symbol, long? clientid);
        INVOICE GetByIdOfSign(long Id);
        INVOICE GetById(long id, long companyId);

        IEnumerable<INVOICE> FilterInvoice(long companyId, long templateId, string symbol);
        string GetMaxInvoiceNo(long companyId, long templateId, string symbol);

        decimal? GetMaxInvoiceNo2(long companyId, long templateId, string symbol);

        IEnumerable<INVOICE> FilterInvoice(ReleaseGroupInvoice condition);

        IEnumerable<ReportInvoiceDetail> FillterListInvoice(ConditionReportDetailUse condition);

        IEnumerable<ReportInvoiceDetail> FillterListInvoiceGroupBy(ConditionReportDetailUse condition);
        IEnumerable<ReportInvoiceDetail> FillterListInvoiceSummary(ConditionReportDetailUse condition);
        IQueryable<ReportInvoiceDetail> FillterListInvoiceSummaryViewReport(ConditionReportDetailUse condition);

        IQueryable<ReportInvoiceDetail> FillterListInvoiceSummaryViewReport_Auto(ConditionReportDetailUse condition);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="flgFill">0: FillterListInvoice, 1: FillterListInvoiceGroupBy</param>
        /// <returns></returns>
        long CountList(ConditionReportDetailUse condition, int flgFill = 0);

        IEnumerable<INVOICE> FilterInvoice(ConditionReportUse condition);
        IEnumerable<INVOICE> FilterInvoiceReleased(ConditionReportUse condition);
        IEnumerable<INVOICE> FilterInvoiceCanceled(ConditionReportUse condition);
        IEnumerable<INVOICE> FilterInvoiceDeleted(ConditionReportUse condition);
        long GetNumberInvoiceReleased(ConditionReportUse condition);
        long GetNumberInvoiceReleased(ConditionInvoiceUse condition);
        long GetNumberInvoiceCanceled(ConditionReportUse condition);
        string GetMinNo(ConditionReportUse condition);
        string GetMaxNo(ConditionReportUse condition);


        SendEmailRelease GetSendInvoiceInfo(long id);

        string GetMaxInvoiceNo(long companyId, long notificationUseInvoiceDetailId, DateTime? dateTo);

        IQueryable<ReportListInvoices> FilterInvoice(ConditionInvoiceBoard condition, IList<int> invoiceStatus);
        IEnumerable<ReportListInvoices> FilterInvoiceGroupByTaxId(ConditionInvoiceBoard condition, IList<int> invoiceStatus, int skip = 0, int take = 0);

        IEnumerable<INVOICE> GetInvoiceUsed();
        /// <summary>
        /// Count summary invoice
        /// </summary>
        /// <param name="flgInvoice">
        /// 0: GetInvoiceOfCompany, 1: GetInvoiceUsed
        /// </param>
        /// <param name="Id"></param>
        /// <returns></returns>
        long CountInvoice(int flgInvoice = 0, long? Id = 0);
        IEnumerable<INVOICE> GetInvoiceOfCompany(long companyId);

        decimal GetNumInvoiceOfCompany(long companyId);
        bool ContainClient(long clientId);

        bool GetInvoiceByClient(long clientId);
        InvoiceReleaseInfo GetInvoiceReleaseInfo(long id);

        SendEmailVerificationCode GetVerificationCodeInfomation(InvoiceVerificationCode invoiceVerificationCode);
        SendEmailVerificationCode GetVerificationCodeInfomation(string VerificationCode, long companyId);
        InvoicePrintInfo GetInvoicePrintInfo(long invoiceId);
        /// <summary>
        /// Láy danh sách mininvociedata
        /// </summary>
        /// <returns></returns>
        List<MINVOICE_DATA> GetAllMINVOICE_DATA();

        IEnumerable<DateTime> DateOfInvoiceCanUse(InvoiceDateInfo invoiceDateInfo);

        INVOICE GetMaxInvoice(long companyId, long registerId, string symbol, DateTime? dateFrom);

        IEnumerable<InvoiceDebts> GetOpenningDebts(ConditionSearchInvoiceDebts condition);
        IEnumerable<InvoiceDebts> GetHappeningDebts(ConditionSearchInvoiceDebts condition);
        INVOICE ResearchInvoice(string verificationCode);
        PTInvoice PTSearchInvoice(string verificationCode);
        IEnumerable<InvoiceMaster> FilterInvoiceOfClient(ConditionSearchInvoiceOfClient condition);

        InvoiceClientSign GetInvoiceOfClient(long invoiceId);

        IEnumerable<INVOICE> GetListInvoiceChild(long id);

        bool ContainProduct(long productId);

        DateTime? MaxReleaseDate(InvoiceInfo invoiceInfo);

        string MinInvoiceNo(InvoiceInfo invoiceInfo);
        IEnumerable<INVOICE> GetInvoiceSignYDay(long CompanyId, long RegisterTemplateId, string Symbol, long InvoiceId, List<long> invoiceIDs);

        IEnumerable<INVOICE> GetListInvoiceByClient(long ClientId);

        INVOICE GetInvoiceByRefNumber(string RefNumber);

        INVOICE GetInvoiceForAdjustmentMonthly(string CIF_NO,string yearmonth, long? CURRENCYID, long? COMPANYID, string VAT_RATE, string REPORT_CLASS);
        IQueryable<INVOICE> GetListInvoiceApproveNo();
        IEnumerable<ReleaseInvoiceInfo> ListInvoiceCreated(ConditionSearchInvoice condition, int? flgCreate = 0);
        long CountInvoiceCreated(ConditionSearchInvoice condition, int? flgCreate = 0);
        IEnumerable<INVOICE> GetListInvoiceSendMail();
        long GetNumberInvoiceReleasedSummary(ConditionReportUse condition);
        IQueryable<INVOICE> GetAllQueryable();
        IEnumerable<ReleaseInvoiceInfo> GetListInvoiceNeedDelete();

        IEnumerable<InvoiceMaster> FilterSubstitute(ConditionSearchInvoice condition);
        long CountSubstitute(ConditionSearchInvoice condition);

        InvoiceInfo GetByInvoiceNo(ConditionSearchInvoice condition);
        long FilterInvoiceNotRelease(ConditionReportUse condition);
        long FilterInvoiceSubstitute(ConditionReportUse condition);
        long FilterInvoiceAdjustment(ConditionReportUse condition);
        bool CheckInvoiceDeleted(List<long> ids);
        bool CheckSendMailStatus(long id);
        IQueryable<INVOICE> GetInvoiceNotRelease(DateTime invoiceRelease);
        IQueryable<INVOICE> GetInvoiceNotReleaseForApprove(DateTime invoiceRelease);

        long CountFillterListInvoiceSummaryViewReport(ConditionReportDetailUse condition);
        decimal CountInvoiceNotRelease(long companyId);
        decimal CountInvoiceAdjustment(long companyId);
        decimal CountInvoiceCancel(long companyId);
        decimal CountInvoiceSubstitute(long companyId);
        decimal CountInvoiceDeleted(long companyId);

        IEnumerable<ReportInvoiceDetail> FillterListInvoiceSummaryDataXML(ConditionReportDetailUse condition);
        void SubGetInvoicePrintInfo(InvoicePrintInfo invoicePrintInfo);
        /// <summary>
        /// Lấy danh sách hóa đơn
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        List<INVOICE> GetByIds(List<long> ids);
        List<InvoicePrintModel> GetInvoicePrintCreateFile(long companyid);

        List<InvoicePrintModel> GetInvoicePrintPersonal(long companyid);
        bool UpdateStatusAfterSign(List<InvoicePrintModel> invoiceModels, string pathSign);
        List<InvoicePrintModel> GetInvoiceSendMails(long companyid);

        void UpdateNoForCompany(long? companyId);

        bool Approve_Invoice(string day, long? companyId, string symbol,long? invoiceid);

        List<CompanySymbolInfo> GetAllSymbol();

        decimal? GetBSLThu(ConditionReportDetailUse condition);
        List<MINVOICE_DATA> ListErrorMinvoice(string messgaeCode);
        INVOICE GetByNoSymbol(string symbol, decimal invoiceNo);
        bool checkBeforeSign(long companyId, string symbol);
        InvoicePrintModel GetInvoicePrint(long invoiceid);
        decimal CountInvoiceRelease(long companyId);
        IEnumerable<InvoiceMaster> FilterInvoiceSP(ConditionSearchInvoice condition);

        IEnumerable<InvoiceMaster> FilterInvoiceSP_Replace(ConditionSearchInvoice condition);

        List<InvoicePrintInfo> SearchInvoiceReplace_Info(long id);
        List<HistoryReportItem> GetInvoiceInfoBTH(long hisId);

        IQueryable<InvoiceCheckDaily> FilterInvoiceCheckDaily(ConditionSearchCheckNumberInvoice condition);
    }
}
