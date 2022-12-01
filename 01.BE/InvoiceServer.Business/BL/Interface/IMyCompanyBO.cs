using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IMyCompanyBO
    {
        CompanyInfo GetCompanyInfo(long id);

        /// <summary>
        /// Get info of the Company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userLevel"></param>
        /// <returns></returns>
        CompanyInfo GetCompanyInfo(long companyId, string userLevel);
        SmallSignatureInfor getByID(long id);
        IEnumerable<MyCompanyInfo> GetList();

        IEnumerable<MasterCompanyInfo> FilterCompany(ConditionSearchCompany condition);
        long CountFillterCompany(ConditionSearchCompany condition);

        ResultCode Create(CompanyInfo companyInfo, UserSessionInfo currentUser);

        ResultCode Update(long companyId, CompanyInfo companyInfo);

        ResultCode Delete(long id, string userLevel);

        MyCompanyInfo GetCompanyOfUser(long id, long? companyId);

        ResultCode UpdateMyCompany(long id, long? companyId, MyCompanyInfo companyInfo, string userLevel);

        IEnumerable<CompanyInfo> GetCompanyChill(long companyIDRoot);

        decimal MyCompanyUsing(long companyID);
        MYCOMPANY GetCompanyById(long id);
    }
}
