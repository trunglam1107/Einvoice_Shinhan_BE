using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Annoucement;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IAnnouncementBO
    {
        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<AnnouncementMaster> Filter(ConditionSearchAnnouncement condition);

        long Count(ConditionSearchAnnouncement condition);

        ExportFileInfo ExportExcel(ConditionSearchAnnouncement condition);

        ExportFileInfo PrintView(long id, long companyId, bool isWord);

        ExportFileInfo PrintViewPdf(long id, long companyId);

        ResultApproved ApproveAnnouncement(ApprovedAnnouncement approveAnnouncement, UserSessionInfo currentUser);

        ResultApproved CancelApproveAnnouncement(ApprovedAnnouncement approveAnnouncement, UserSessionInfo currentUser);

        ResultCode Delete(long id);

        AnnouncementInfo GetAnnouncementInfo(long id, long companyId);

        AnnouncementsRelease GetAnnouncementOfClient(long id);

        AnnouncementInfo GetAnnouncementByInvoiceId(long invoiceId, int announType, long companyId);
        AnnouncementInfo CheckExistAnnoun(long invoiceId, bool invoiceType, int announType);
        bool CheckAnnount(long invoiceId, int type);
        ResultCode SearchCode(string ms, string kh, DateTime date, string num);

        ANNOUNCEMENT SaveMD(AnnouncementMD anInfo);

        ResultCode Update(AnnouncementMD anInfo);

        ResultCode UpdateAnnounceAfterInvoiceCancel(AnnouncementMD anInfo);

        string UpdateAnnouncementStatus(AnnouncementMD anInfo, UserSessionInfo userSessionInfo);

        AnnouncementMD LoadList(long id, int actiontype, bool isAnounId, UserSessionInfo userSessionInfo);

        InvoicePrintInfo GetInvoiceData(long id);

        InvoicePrintInfo GetInvoiceData_Replace(long id);
        ResultCode Remove(long announcementID, long updatedBy);

        string IsExit(long id, int announcementType);
        string IsExistAll(long id);
        long CreateVerificationCode(AnnouncementVerificationCode announcementVerificationCode);
        SendEmailVerificationCodeAnnoun GetVerificationCode(AnnouncementVerificationCode announcementVerificationCode);
        long CreateEmailActive(long companyId, SendEmailVerificationCodeAnnoun receiverInfo);

        long callCreateEmailActive(long companyId, SendEmailVerificationCodeAnnoun receiverInfo);
        bool checkExist(string code, long? id, long company);
    }
}
