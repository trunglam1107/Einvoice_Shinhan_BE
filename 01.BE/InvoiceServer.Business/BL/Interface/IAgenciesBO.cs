using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IAgenciesBO
    {
        /// <summary>
        /// Get info of the Sale
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userLevel"></param>
        /// <returns></returns>
        AgenciesInfo GetAgenciesInfo(int companyId, string userLevel);

        IEnumerable<AgenciesInfo> FilterAgencies(ConditionSearchAgencies condition, int skip = int.MinValue, int take = int.MaxValue);
        int CountFillterAgencies(ConditionSearchAgencies condition);

        ResultCode Create(AgenciesInfo companyInfo);

        ResultCode Update(int companyId, AgenciesInfo companyInfo);

        ResultCode Delete(int id, int companyId, string userLevel);
    }
}
