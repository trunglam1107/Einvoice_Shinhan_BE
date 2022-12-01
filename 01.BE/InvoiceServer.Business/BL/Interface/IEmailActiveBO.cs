using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IEmailActiveBO
    {
        /// <summary>
        /// Search EmailActiveInfo by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<EmailActiveInfo> Filter(ConditionSearchEmailActive condition);

        long CountFilter(ConditionSearchEmailActive condition, int skip = 0, int take = int.MaxValue);

        EmailActiveInfo GetEmailActive(long companyId, long id);

        ResultCode SendEmail(long id, EmailActiveInfo emailActiveInfo, EmailServerInfo emailServerInfo);

        ResultCode SendEmail(long id, EmailServerInfo emailServerInfo);
        ResultCode SendEmailByMonth(long id, List<SendEmailVerificationCode> emailVerificationCodeInfo, EmailServerInfo emailServerInfo);
        ResultCode SendEmailByMonth(long id, List<SendEmailVerificationCode> emailVerificationCodeInfo);
        ResultCode SendEmailDaily(long id, SendEmailVerificationCode emailVerificationCodeInfo, bool isJob = false);
        ResultCode SendEmailAnoumentDaily(long id, SendEmailVerificationCodeAnnoun emailVerificationCodeInfo);
        ResultCode SendEmailProvideAccount(ClientAddInfo clientAddInfo, bool isReSend = false);
        ResultCode SendEmailSino(List<SendEmailVerificationCode> emailVerificationCodeInfos);
        ResultCode SendEmailDailySino(long emailActiveId, SendEmailVerificationCode emailVerificationCodeInfos);
        ResultCode SendEmails(SendMultiMail emailActives, EmailServerInfo emailServerInfo);
        void SendEmailForJobShinhan(SendEmailVerificationCode emailVeriCodes);
        long? checkSentEmail(long refId);
        ResultCode SendEmailResetPassword(ClientAddInfo clientAddInfo);
        bool JobSendMails(InvoicePrintModel printModel, string pathZip, long emailActiveId);
    }
}
