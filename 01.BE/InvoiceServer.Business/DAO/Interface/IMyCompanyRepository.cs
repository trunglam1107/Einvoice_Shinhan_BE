using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IMyCompanyRepository : IRepository<MYCOMPANY>
    {
        /// <summary>
        /// Get an IEnumerable MyCompany 
        /// </summary>
        /// <returns><c>IEnumerable MyCompany</c> if MyCompany not Empty, <c>null</c> otherwise</returns>
        IEnumerable<MYCOMPANY> GetList();

        /// <summary>
        /// Get an MyCompany by CompanySID.
        /// </summary>
        /// <param name="id">The condition get MyCompany.</param>
        /// <returns><c>Operator</c> if CompanySID on database, <c>null</c> otherwise</returns>
        MYCOMPANY GetById(long id);

        /// <summary>
        ///  Lấy danh sách companies 
        /// </summary>
        /// <param name="ids">danh sách id của company</param>
        /// <returns></returns>
        List<MYCOMPANY> GetByIds(List<long> ids);

        /// <summary>
        /// Search Company by the filter conditions
        /// </summary>
        /// <param name="filterCondition"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>       
        IEnumerable<MYCOMPANY> FilterCompany(ConditionSearchCompany condition);

        IEnumerable<MYCOMPANY> FilterCustomer(ConditionSearchCustomer condition);

        IEnumerable<MYCOMPANY> FilterCustomer(ConditionSearchCustomer condition, string customerType);

        bool ContainEmail(string email);
        bool ContainTaxCode(string taxCode);

        bool ContainEmail(long companyId, string email, string levelCustomer);
        bool ContainTaxCode(long companyId, string taxCode, string levelCustomer);

        IEnumerable<ReportCustomer> GetReportReportCustomer(ConditionReportSummary condition);
        IEnumerable<ReportCustomer> GetReportReportCompany(ConditionReportSummary condition);

        MYCOMPANY GetHO();

        MYCOMPANY GetHODiff(long companyIdDiff);

        IEnumerable<MYCOMPANY> GetBranchs();

        IEnumerable<BranchLevel> GetListBranchs(long companyId, int isGetAll);

        IEnumerable<BranchLevel> GetListBranchs_V2(long companyId, int isGetAll);

        IEnumerable<BranchLevel> GetListBranchsOnly(long companyId, int isGetAll);
        IEnumerable<BranchLevel> GetListBranchs_2(long companyId);

        IEnumerable<BranchLevel> GetListAllBranchs();
        IEnumerable<MYCOMPANY> GetCompanyChill(long companyIDRoot);
        decimal MyCompanyUsing(long companyID);

        MYCOMPANY GetByTaxCode(string taxCode);
        MYCOMPANY GetByNameTaxCode(string taxCode, string customerName);
        MYCOMPANY GetByName(string customerName);

        MYCOMPANY GetByBranchId(string branchId);

        IEnumerable<MYCOMPANY> GetListByCompanyId(long? companyId);
    }
}
