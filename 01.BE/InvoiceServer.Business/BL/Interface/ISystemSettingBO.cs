using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface ISystemSettingBO
    {
        SystemSettingInfo GetSystemSettingInfo(long companyId);

        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="filterCondition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<SystemSettingInfo> Filter(ConditionSearchSystemSetting condition, int skip = 0, int take = int.MaxValue);

        /// <summary>
        /// Count number clients by condition
        /// </summary>
        /// <param name="filterCondition"></param>
        /// <returns></returns>

        SystemSettingInfo GetActiveSystemSetting(long CompanyID);

        long Count(ConditionSearchSystemSetting condition);

        ResultCode CreateOrUpdateSystemSetting(long companyId, SystemSettingInfo systemSettingInfo);
        string GetSlots();
        bool CheckSlots(int? slot, long companyId);
    }
}
