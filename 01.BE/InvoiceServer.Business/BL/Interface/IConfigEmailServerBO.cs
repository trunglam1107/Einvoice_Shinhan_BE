using InvoiceServer.Business.Models;
using InvoiceServer.Common;

namespace InvoiceServer.Business.BL
{
    public interface IConfigEmailServerBO
    {
        /// <summary>
        /// Search EmailActiveInfo by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        EmailServerInfo GetByCompanyId(long companyId);

        ResultCode Create(EmailServerInfo emailActiveInfo);
    }
}
