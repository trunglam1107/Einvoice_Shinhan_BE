using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
    public class AgenciesBusiness : BaseBusiness
    {
        #region Fields, Properties

        private IAgenciesBO agenciesBO;

        #endregion Fields, Properties

        #region Contructor

        public AgenciesBusiness(IBOFactory boFactory)
        {
            this.agenciesBO = boFactory.GetBO<IAgenciesBO>();
        }

        #endregion Contructor

        #region Methods

        public IEnumerable<AgenciesInfo> FillterAgencies(out int totalRecords, string agenciesName = null, string mobile = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
        {
            var condition = new ConditionSearchAgencies(this.CurrentUser, agenciesName, mobile, orderType, orderby);
            totalRecords = this.agenciesBO.CountFillterAgencies(condition);
            var agencies = this.agenciesBO.FilterAgencies(condition, skip, take);
            return agencies;
        }

        public AgenciesInfo GetAgenciesInfo(int companyId)
        {
            return this.agenciesBO.GetAgenciesInfo(companyId, RoleInfo.SYSTEMADMIN);
        }

        public ResultCode Create(AgenciesInfo agenciesInfo)
        {
            if (agenciesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            agenciesInfo.SessionInfo = this.CurrentUser;
            agenciesInfo.CompanyId = GetCompanyIdOfUser();
            agenciesInfo.Level_Agencies = (CurrentUser.Company.LevelAgencies + 1);
            ResultCode result = this.agenciesBO.Create(agenciesInfo);
            return result;
        }

        public ResultCode Update(int companyId, AgenciesInfo agenciesInfo)
        {
            if (agenciesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            agenciesInfo.SessionInfo = this.CurrentUser;
            agenciesInfo.CompanyId = GetCompanyIdOfUser();
            return this.agenciesBO.Update(companyId, agenciesInfo);
        }

        public ResultCode Delete(int id)
        {
            string roleOfCurrentUser = this.CurrentUser.RoleUser.Level;
            int companyId = this.GetCompanyIdOfUser();
            return this.agenciesBO.Delete(id,companyId, roleOfCurrentUser);
        }

        #endregion
    }
}