using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IReleaseInvoiceBO
    {

        Result ReleaseInvoice(ReleaseGroupInvoice info);
        /// <summary>
        /// Add new client
        /// </summary>
        /// <param name="client">client info</param>
        /// <returns></returns>
        Result Create(ReleaseInvoiceMaster info, ReleaseInvoiceStatus status = ReleaseInvoiceStatus.New);

        ResultCode Update(long id, long companyId, StatusRelease statusRelease);

        RELEASEINVOICE GetReleaseInvoiceById(long id);

        ResultCode UpdateStatusReleaseInvoiceDetail(long releaseId, StatusReleaseInvoice statusReleaseInvoice, UserSessionInfo currentUser);

        IEnumerable<ReleaseInvoices> FilterReleaseInvoice(ConditionSearchReleaseInvoice condition);

        IEnumerable<ReleaseListInvoice> FilterReleaseInvoice(ConditionSearchReleaseList condition);

        long CountReleaseInvoice(ConditionSearchReleaseInvoice condition);

        IEnumerable<SendEmailRelease> SendNotificeReleaseInvoice(IList<int> status);

        void UpdateProcessSendEmail(IList<ResultRelease> results);

        RELEASEINVOICE FilterInvoice(long id, IList<int> status);

        long CountFilterReleaseInvoice(ConditionSearchReleaseList condition);

        IEnumerable<InvoicesReleaseDetail> ReleaseInvoiceDetail(ConditionSearchReleaseListDetail condition, int skip = int.MinValue, int take = int.MaxValue);

        long CountReleaseInvoiceDetail(ConditionSearchReleaseListDetail condition);

        RELEASEINVOICE GetReleaseInvoiceByInvoiceId(long id);

        long? GetCompanyIdByVerifiCode(string VerifiCod);

        string GeneratorInvoiceNoFromBD(long companyId, long templateId, string symbol, int? status);
        void JobSignFile(ReleaseInvoiceMaster releaseInvoice, UserSessionInfo currentUser);
        ServerSignResult SignFile(ReleaseInvoiceMaster releaseInvoice, UserSessionInfo currentUser);
        ServerSignResult SignFileManual(ReleaseInvoiceMaster releaseInvoice, UserSessionInfo currentUser);
        INVOICE GetInvoiceCheckSign(long invoiceId);
        bool CheckSignedInvoice(INVOICE invoice, List<long> invoiceIDs);
        void SendMailByMonth(EmailServerInfo emailServerInfo);
        EmailServerInfo GetEmailServerInfo();
        IEnumerable<InvoicesReleaseDetail> GetInvoiceByReleaseId(long id);
        RELEASEINVOICE InsertReleaseInvoice(ReleaseInvoiceMaster info, ReleaseInvoiceStatus status);
        void SendMailSino();
        void JobSignShinhan(List<INVOICE> invoices, SIGNATURE signature, string companyName);
        InvoiceMaster IscheckResponeCQT(long id);
        ServerSignResult IscheckCodeCQT(long id);
        ServerSignResult IscheckRepsoneTwanWithNotCode(long id);
    }
}
