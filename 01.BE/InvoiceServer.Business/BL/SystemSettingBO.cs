using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;

namespace InvoiceServer.Business.BL
{
    public class SystemSettingBO : ISystemSettingBO
    {
        #region Fields, Properties
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly IDbTransactionManager transaction;

        #endregion

        #region Contructor
        public SystemSettingBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
        }

        #endregion

        #region Methods
        public SystemSettingInfo GetSystemSettingInfo(long companyId)
        {
            return systemSettingRepository.FilterSystemSetting(companyId);
        }

        public IEnumerable<SystemSettingInfo> Filter(ConditionSearchSystemSetting condition, int skip = 0, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var projects = this.systemSettingRepository.FilterSystemSettings(condition).AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(skip).Take(take).ToList();
            return projects.Select(p => new SystemSettingInfo(p));
        }

        public SystemSettingInfo GetActiveSystemSetting(long CompanyID)
        {
            var systemSetting = this.systemSettingRepository.GetByCompany(CompanyID);
            var result = new SystemSettingInfo(systemSetting);
            return result;
        }

        public long Count(ConditionSearchSystemSetting condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.systemSettingRepository.FilterSystemSettings(condition).Count();
        }

        public ResultCode CreateOrUpdateSystemSetting(long companyId, SystemSettingInfo systemSettingInfo)
        {
            try
            {
                transaction.BeginTransaction();
                var setting = this.systemSettingRepository.GetByCompany(companyId);
                if (setting == default(SYSTEMSETTING))
                {
                    CreateSystemSetting(companyId, systemSettingInfo);
                }
                else
                {
                    UpdateSystemSetting(systemSettingInfo, setting);
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return ResultCode.NoError;
        }
        public string GetSlots()
        {
            string response = String.Empty;
            string UrlGetSlots = PathUtil.UrlCombine(WebConfigurationManager.AppSettings["UrlServerSign"], ApiSignInvoice.ActionServerSign, ApiSignInvoice.ActionGetSlot);

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlGetSlots);
                request.ContentType = "application/json";
                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                {
                    StreamReader responseReader = new StreamReader(webStream);
                    response = responseReader.ReadToEnd();
                }
            }
            catch
            {
                throw new BusinessLogicException(ResultCode.NotSlotHSM, "Không thể load cổng Server HSM");
            }

            return response;
        }

        public bool CheckSlots(int? slot, long companyId)
        {
            var slots = this.systemSettingRepository.CheckSlots(slot, companyId);
            if (slots != null)
                return false;
            return true;
        }
        #endregion Methods

        #region Private Methods
        private void CreateSystemSetting(long companyId, SystemSettingInfo systemSettingInfo)
        {
            var systemSetting = new SYSTEMSETTING()
            {
                ID = companyId,
            };
            systemSetting.CopyData(systemSettingInfo);

            this.systemSettingRepository.Insert(systemSetting);
        }

        private void UpdateSystemSetting(SystemSettingInfo systemSettingInfo, SYSTEMSETTING systemSetting)
        {
            systemSetting.CopyData(systemSettingInfo);

            systemSettingRepository.Update(systemSetting);
        }
        #endregion Private Methods
    }
}