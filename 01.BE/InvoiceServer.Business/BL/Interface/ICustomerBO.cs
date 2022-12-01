using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface ICustomerBO
    {
        /// <summary>
        /// Get info of the Sale
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userLevel"></param>
        /// <returns></returns>
        CustomerInfo GetCustomerInfo(long companyId);

        CustomerInfo GetCustomerInfoForBB(long companyId);

        IEnumerable<CustomerInfo> FilterCustomer(ConditionSearchCustomer condition);
        long CountFillterCustomer(ConditionSearchCustomer condition);

        ResultCode Create(CustomerInfo customerInfo);

        ResultCode Update(long id, CustomerInfo customerInfo);

        ResultCode Delete(long id, long companyId, string userLevel);

        IEnumerable<CustomerInfo> FilterCustomerByContructType(ConditionSearchCustomer condition, ContractType contractType, int skip = int.MinValue, int take = int.MaxValue);
        long CountFilterCustomerByContructType(ConditionSearchCustomer condition, ContractType contractType);

        IEnumerable<Branch> GetBranchs();

        IEnumerable<BranchLevel> GetListBranchs(long companyId, int isGetAll);

        IEnumerable<BranchLevel> GetListBranchs_V2(long companyId, int isGetAll);

        IEnumerable<BranchLevel> GetListBranchs_2(long companyId);
        IEnumerable<BranchLevel> GetListAllBranchs();
        IEnumerable<BranchLevel> GetListBranchsOnly(long companyId, int isGetAll);
        ResultImportSheet ImportData(string fullPathFile, long companyId);
        ExportFileInfo DownloadDataCustomer(ConditionSearchCustomer condition);
    }
}
