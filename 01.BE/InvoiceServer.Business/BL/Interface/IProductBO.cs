using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IProductBO
    {
        /// <summary>
        /// Add new client
        /// </summary>
        /// <param name="client">client info</param>
        /// <returns></returns>
        ResultCode Create(ProductInfo productInfo);

        /// <summary>
        /// Update client info
        /// </summary>
        /// <param name="client">client info to update</param>
        /// <returns></returns>
        ResultCode Update(long id, ProductInfo productInfo);

        /// <summary>
        /// Delete Client By Update Deleted Flag = true
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        ResultCode Delete(long id, long? companyId);

        /// <summary>
        /// Get the client info
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        ProductInfo GetProductInfo(long id);

        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="filterCondition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<ProductInfo> Filter(ConditionSearchProduct condition, int skip = 0, int take = int.MaxValue);

        /// <summary>
        /// Count number clients by condition
        /// </summary>
        /// <param name="filterCondition"></param>
        /// <returns></returns>
        long Count(ConditionSearchProduct condition);
        bool MyProductUsing(long id);
        ResultImportSheet ImportData(string fullPathFile, long companyId);
        ExportFileInfo DownloadDataProduct(ConditionSearchProduct codition, CompanyInfo company);
    }
}
