using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IMyCompanyUpgradeBO
    {
        /// <summary>
        /// Get info of the Company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userLevel"></param>
        /// <returns></returns>
        CompanyUpgradeInfo getByID(long id);
        IEnumerable<MYCOMPANYUPGRADE> GetList();
        ResultCode Create(CompanyUpgradeInfo companyInfo, UserSessionInfo currentUser);
    }
}
