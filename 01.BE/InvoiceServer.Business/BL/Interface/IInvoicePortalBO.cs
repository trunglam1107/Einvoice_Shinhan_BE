using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Common;
using System.Collections;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL.Interface
{
    public interface IInvoicePortalBO
    {
        IEnumerable GetInvoiceByVeriCode(string VerificationCode);

        IEnumerable GetInvoiceByVeriCodeAndClientId(string VerificationCode, long ClientId, string DateFrom, string DateTo);

        IEnumerable SearchInvoiceByInvoiceNo(string InvoiceNo, string taxcode, string DateFrom, string DateTo);

        IEnumerable GetAnnounByVeriCode(string VerificationCode);

        IEnumerable GetAnnounByVeriCodeAndClientId(string VerificationCode, long ClientId, string DateFrom, string DateTo);

        IEnumerable GethAnnounByInvoiceNo(string InvoiceNo, long ClientID, string DateFrom, string DateTo);

        string GetToken(string VerificationCode, long companyID);

        string GetTokenByInvoiceCode(string InvoiceCode, long companyID, long ClientID);

        long? GetCompanyIdByInvoiceVerifiCode(string VerifiCod);

        long? GetCompanyIdByAnnounVerifiCode(string VerifiCod);

        ResultCode UpdataStatusReleaseDetailInvoice(long id);
        ResultCode AnnouncementUpdataStatusClientSign(long Id);

        ResultCode UpdateClientInfo(long id, ClientAddInfo client);

        IEnumerable<PTClientAccountInfo> GetListClientNotChangePass();


        IEnumerable<NotificationUsedInvoiceDetail> GetNotificationUseInvoiceDetails();

        long GetNumberInvoiceReleased(ConditionInvoiceUse condition);

        bool wasSendWarning(ConditionInvoiceUse condition);

        ResultCode SendEmailWarning(List<NotificationUsedInvoiceDetail> contentMail, string emailCompany, EmailServerInfo emailServerInfo);

        ResultCode UpdateStatusReceiveWarning(List<NotificationUsedInvoiceDetail> listItem);


        ExportFileInfo ExcelInvoiceNoClient(string InvoiceNo, long ClientID, string DateFrom, string DateTo);
        ExportFileInfo ExcelInvoiceByVeriCodeLogin(string InvoiceNo, long ClientID, string DateFrom, string DateTo);
    }
}
