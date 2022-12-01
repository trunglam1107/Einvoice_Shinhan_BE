using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace InvoiceServer.Business.BL
{
    public interface IInvoiceBO
    {
        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<InvoiceMaster> Filter(ConditionSearchInvoice condition);

        IEnumerable<InvoiceMaster> FilterAnnoun(ConditionSearchInvoice condition);

        IEnumerable<InvoiceMaster> FilterCreated(ConditionSearchInvoice condition, int? flgCreate = 0);

        IEnumerable<ReleaseInvoiceInfo> GetListInvoiceNo(List<ReleaseInvoiceInfo> releaseInvoiceInfo);

        IEnumerable<ReleaseInvoiceInfo> GetListInvoiceApproved(long branch);
        IQueryable<INVOICE> GetListInvoiceCreatedFileShinhan(long companyId, bool isOrg);
        IQueryable<INVOICE> GetListInvoiceNewShinhan(long companyId, bool isOrg);

        void JobApproveItemShinhan_Old(List<INVOICE> invoices, MYCOMPANY Company);

        void JobApproveNew(string day, long? companyId, string symbol,long? invoiceid);
        void UpdateNoForCompany(long? companyId);
        /// <summary>
        /// Count number clients by condition
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        long Count(ConditionSearchInvoice condition);
        long CountV2(ConditionSearchInvoice condition);
        IEnumerable<InvoiceMaster> FilterV2(ConditionSearchInvoice condition);

        long CountCreated(ConditionSearchInvoice condition, int? flgCreate = 0);
        IEnumerable<ReleaseInvoiceInfo> FilterListCreated(ConditionSearchInvoice condition, int? flgCreate = 0);

        IEnumerable<InvoiceMaster> FilterInvoice(ConditionSearchInvoiceOfClient condition, int skip = 0, int take = int.MaxValue);

        IEnumerable<InvoiceMaster> FillterInvoiceConverted(ConditionSearchInvoice condition);
        INVOICE GetCurrentInvoice(long id, long companyId);
        InvoiceInfo GetById(long id);
        /// <summary>
        /// Count number clients by condition
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        long CountInvoice(ConditionSearchInvoiceOfClient condition);

        long CountInvoiceConverted(ConditionSearchInvoice condition);

        /// <summary>
        /// Add new client
        /// </summary>
        /// <param name="client">client info</param>
        /// <returns></returns>
        ResultCode Create(InvoiceInfo info);

        ReleaseInvoiceMaster Substitute(InvoiceInfo info);

        ReleaseInvoiceMaster Adjust(InvoiceInfo info);

        ReleaseInvoiceMaster AdjustJob(InvoiceInfo info, DateTime? releaseddate);

        ResultApproved JobApproveInvoice(ApprovedInvoice approveInvoice, UserSessionInfo currentUser);
        void JobApproveItemShinhan(List<INVOICE> invoices);
        /// <summary>
        /// Update client info
        /// </summary>
        /// <param name="client">client info to update</param>
        /// <returns></returns>
        ResultCode Update(long id, long? companyId, InvoiceInfo info);

        ResultCode UpdateStatus(long id, long? companyId, int status);

        ResultCode UpdateNote(long id, InvoiceNote invoiceNote);

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
        InvoiceInfo GetInvoiceInfo(long id, long? companyId, bool isInvoice = true);

        InvoicePrintInfo GetInvoiceData(long id);

        List<InvoiceStatistical> GetListStatisticals(long id);

        List<InvoiceGift> GetListGift(long id);

        InvoiceNo RegistInvoiceNo(long companyId, long templateId, string symbol);

        ExportFileInfo PrintView(long id);

        ExportFileInfo GetXmlFile(long id);
        ExportFileInfo DownloadStatistical(long id);
        ExportFileInfo DownloadGift(long id);
        IEnumerable<ReplacedInvoice> FilterReplacedInvoice(SearchReplacedInvoice condition);

        long CountReplacedInvoice(SearchReplacedInvoice condition);

        IEnumerable<ReportInvoiceDetail> FillterListInvoice(ConditionReportDetailUse condition);

        IEnumerable<ReportInvoiceDetail> FillterListInvoiceGroupBy(ConditionReportDetailUse condition);

        long CountFillterListInvoice(ConditionReportDetailUse condition);

        long CountFillterListInvoiceGroupBy(ConditionReportDetailUse condition);

        ExportFileInfo PrintViewSituationInvoiceUsage(ConditionReportUse condition, CompanyInfo company);

        ExportFileInfo PrintViewSituationInvoiceUsageXML(ConditionReportUse codition, CompanyInfo company);

        ReportInvoiceUsing ViewSituationInvoiceUsage(ConditionReportUse codition, CompanyInfo company);

        ReportSituationInvoiceUsage ViewSituationInvoiceUsageSixMonth(ConditionReportUse codition, CompanyInfo company);

        ReportGeneralInvoice ViewGeneralInvoiceUsage(ConditionReportDetailUse condition);

        ResultImportSheet ImportData(UploadInvoice dataUpload, string fullPathFile, long companyId);
        ResultImportSheet ImportDataInvoiceReplace(UploadInvoice dataUpload, string fullPathFile, long companyId);
        ResultImportSheet ImportDataInvoiceAdjusted(UploadInvoice dataUpload, string fullPathFile, long companyId);
        ExportFileInfo DownloadInvoiceXML(long id);

        InvoiceReleaseInfo GetInvoiceReleaseInfo(long id);

        decimal GetTotalInvoiceUse();

        decimal GetTotalInvoiceOfCompanyUse(long companyId);

        decimal GetTotalInvoiceOfCancelling(long companyId);

        ResultApproved ApproveInvoice(ApprovedInvoice approveInvoice, UserSessionInfo currentUser);

        ResultApproved ApproveInvoiceNew(ApprovedInvoice approveInvoice, UserSessionInfo currentUser);
        bool CheckPermission(ApprovedInvoice approveInvoice, UserSessionInfo currentUser);

        long CreateVerificationCode(InvoiceVerificationCode invoiceVerificationCode);
        SendEmailVerificationCode GetEmailVerificationCodeInfo(InvoiceVerificationCode invoiceVerificationCode);
        long CreateEmailActive(long companyId, SendEmailVerificationCode receiverInfo);
        ExportFileInfo ExportInvoiceToPdf(long id, bool isSwitch = false);

        ExportFileInfo SwitchInvoice(long id);

        ResultValidDate DateInvoiceCanUse(InvoiceDateInfo invoiceDateInfo, bool isNew = true);

        ExportFileInfo ResearchInvoice(string verificationCode);

        ExportFileInfo DownloadReportCombine(ConditionReportDetailUse condition);

        ExportFileInfo DownloadReportInvoiceDetail(ConditionReportDetailUse condition);

        ExportFileInfo DownloadReportInvoiceDetailGroupBy(ConditionReportDetailUse condition);
        ExportFileInfo DownloadReportInvoiceDataSummary(ConditionReportDetailUse condition);
        InvoiceClientSign GetInvoiceOfClient(long invoiceId);

        ExportFileInfo ExportInvoiceConverted(ConditionSearchInvoice condition);

        bool Verify(string file);

        InvoiceReleaseDate MaxReleaseDate(InvoiceInfo invoiceInfo);

        InvoiceNo MinInvoiceNo(InvoiceInfo invoiceInfo);

        InvoiceNo MaxInvoiceNo(InvoiceInfo invoiceInfo);

        ResultApproved RevertApprovedInvoice(ApprovedInvoice approvedInvoice, UserSessionInfo currentUser);
        void UpdateStatusSendMail(InvoiceVerificationCode invoiceVerificationCode);
        string ImportXmlInvoice(string invoice);
        List<InvoiceGift> ReadGiftFile(string pathFile);
        List<InvoiceDetailInfo> ReadDetailFile(string pathFile);
        ANNOUNCEMENT GetAnnouncement(long id, int announcementType, long? companyId);

        ResultCode ImportInvoiceFile(bool isJob);
        BIDCImportFromAPIOutput BIDCImportInvoiceExternalApi(XmlDocument xmlDocument);

        long CountInvoiceCreated(ConditionSearchInvoice condition, int? flgCreate = 0);
        ResultCode UpdateAdjustFileRecipt(InvoiceInfo info);
        ResultCode UpdateCancelFileRecipt(InvoiceInfo info);

        ReportSituationInvoiceUsage ViewSituationInvoiceUsageSummary(ConditionReportUse codition, CompanyInfo company);

        IEnumerable<InvoiceMaster> FilterSubstitute(ConditionSearchInvoice condition);
        long CountSubstitute(ConditionSearchInvoice condition);
        InvoiceInfo GetByInvoiceNo(ConditionSearchInvoice condition);
        ExportFileInfo ExportExcel(ConditionSearchInvoice condition);
        ExportFileInfo ExportExcelAdjust(SearchReplacedInvoice condition, int status);
        ExportFileInfo ExportExcelInvoices(ConditionSearchInvoice condition);

        ExportFileInfo ExportExcelCkInvoiceNumber(ConditionSearchCheckNumberInvoice conditionSrc);
        FooterConfig GetConfigInfo();
        void UpdataInvoiceStatus(InvoiceInfo invoice, int status);

        ExportFileInfo ReportInvoiceDataSummaryXML(ConditionReportDetailUse condition);

        ReportHistoryXml SendMQTXml(ConditionReportDetailUse condition, HISTORYREPORTGENERAL history, SIGNATURE signature);

        List<ReportHistoryXml> ListReportHistoryXml(ConditionReportDetailUse condition, SIGNATURE signature);

        List<ReportHistoryXml> ListReportHistoryXml_Auto(ConditionReportDetailUse condition, SIGNATURE signature);

        int GetInvoiceUsed(ConditionReportUse codition);

        decimal CountInvoiceNotRelease(long companyId);

        decimal CountInvoiceAdjustment(long companyId);

        decimal CountInvoiceCancel(long companyId);

        decimal CountInvoiceSubstitute(long companyId);

        decimal CountInvoiceDeleted(long companyId);

        decimal CountInvoiceRelease(long companyId);

        List<InvoicePrintModel> GetInvoicePrintCreateFile(long companyid);
        List<InvoicePrintModel> GetInvoicePrintPersonal(long companyid);
        ExportModelSumary ProcessInvoicePrint(List<InvoicePrintModel> invoicePrints, MYCOMPANY myconpany, INVOICETEMPLATE template);
        void ProcessPdfs(List<InvoiceExportModel> invoiceExportPdfModels);
        void ProcessXmls(List<InvoiceExportXmlModel> invoiceExportXmlModels);
        void SignXmls(List<InvoicePrintModel> invoiceModels, bool isPer = false);
        void UpdateStatusAfterSign(List<InvoicePrintModel> invoiceModels);
        void SendMailJob(long companyId);
        bool checkBeforeSign(long companyId, string symbol);
        List<CompanySymbolInfo> listSymbol();
        //void JobApproveCuPhuong();
        ExportFileInfo DownloadHisItem(long hisId);
        ReportGeneralInvoice GetBSLThu(ConditionReportDetailUse condition);
        ResultCode processXmlError(string messageCode);
        IEnumerable<InvoiceMaster> FilterSP(ConditionSearchInvoice condition);
        IEnumerable<InvoiceMaster> FilterSP_Replace(ConditionSearchInvoice condition);
        INVOICETEMPLATE GetTemplateNoTracking(long id);

        IEnumerable<InvoiceCheckDaily> FilterInvoiceCheckDaily(ConditionSearchCheckNumberInvoice condition);

        int GetExpirceDateToken(long companyId);
    }
}
