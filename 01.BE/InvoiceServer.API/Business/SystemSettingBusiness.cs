using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness'
    public class SystemSettingBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness'
    {
        #region Fields, Properties

        private readonly ISystemSettingBO systemSettingBO;

        #endregion Fields, Properties

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.SystemSettingBusiness(IBOFactory)'
        public SystemSettingBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.SystemSettingBusiness(IBOFactory)'
        {
            this.systemSettingBO = boFactory.GetBO<ISystemSettingBO>();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.FillterSystemSetting(out long, string, string, int, int)'
        public List<SystemSettingInfo> FillterSystemSetting(out long totalRecords, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.FillterSystemSetting(out long, string, string, int, int)'
        {
            var condition = new ConditionSearchSystemSetting(this.CurrentUser, orderby, orderType);
            totalRecords = this.systemSettingBO.Count(condition);
            return this.systemSettingBO.Filter(condition, skip, take).ToList();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.GetSystemSettingDetail(long)'
        public SystemSettingInfo GetSystemSettingDetail(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.GetSystemSettingDetail(long)'
        {
            var systemSettings = this.systemSettingBO.GetSystemSettingInfo(companyId);
            var i = 1;
            systemSettings.ListRowNumber = new List<int>();
            while (i < 30)
            {
                string value = WebConfigurationManager.AppSettings["RowNumber_" + i];
                if (value != null)
                {
                    systemSettings.ListRowNumber.Add(int.Parse(value));
                    i++;
                }
                else
                {
                    break;
                }

            }
            return systemSettings;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.CreateOrUpdateSystemSetting(long, SystemSettingInfo)'
        public ResultCode CreateOrUpdateSystemSetting(long companyId, SystemSettingInfo systemSettingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.CreateOrUpdateSystemSetting(long, SystemSettingInfo)'
        {
            if (systemSettingInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.systemSettingBO.CreateOrUpdateSystemSetting(companyId, systemSettingInfo);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.GetSlots()'
        public string GetSlots()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.GetSlots()'
        {
            return this.systemSettingBO.GetSlots();
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.CheckSlots(int?, long)'
        public bool CheckSlots(int? slot, long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingBusiness.CheckSlots(int?, long)'
        {
            return this.systemSettingBO.CheckSlots(slot, companyId);
        }
        #endregion
    }
}