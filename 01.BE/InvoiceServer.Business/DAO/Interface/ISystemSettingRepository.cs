using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ISystemSettingRepository : IRepository<SYSTEMSETTING>
    {

        /// <summary>
        /// Search Company by the filter conditions
        /// </summary>
        /// <param name="filterCondition"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<SYSTEMSETTING> FilterSystemSettings(ConditionSearchSystemSetting condition);

        SystemSettingInfo FilterSystemSetting(long companyId);

        SystemSettingInfo FilterSystemSettingJob();

        SYSTEMSETTING GetByCompany(long companyId);

        SYSTEMSETTING CheckSlots(int? slot, long companyId);
    }
}
