using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
    public class EmployeeBusiness : BaseBusiness
    {
        #region Fields, Properties

        private readonly IEmployeeBO employeeBO;
        //private readonly IRoleOfUserBO roleOfUserBO;
        
        #endregion Fields, Properties

        #region Methods

        public EmployeeBusiness(IBOFactory boFactory)
        {

            this.employeeBO = boFactory.GetBO<IEmployeeBO>();
            //this.roleOfUserBO = boFactory.GetBO<IRoleOfUserBO>();
        }

        public List<EmployeeInfo> FillterUser(out int totalRecords, string name = null, string loginId = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
        {
            var condition = new ConditionSearchEmployee(this.CurrentUser, name, loginId, orderby, orderType);
            condition.ChildLevels = new string[] { RoleInfo.SALE_EMPLOYER };
            totalRecords = this.employeeBO.CountFillterEmployee(condition);
            return this.employeeBO.FillterEmployee(condition, skip, take).ToList();
        }

        public EmployeeInfo GetEmployeeInfo(int id)
        {
            return this.employeeBO.GetEmployeeInfo(id, this.CurrentUser.Company.Id, this.CurrentUser.RoleUser.Level);
        }

        public ResultCode CreateEmployee(EmployeeInfo userInfo)
        {
            if (userInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            userInfo.CompanyId = this.CurrentUser.Company.Id;
            return this.employeeBO.Create(userInfo); 
        }

        public ResultCode UpdateUser(int id, EmployeeInfo userInfo)
        {
            if (userInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.employeeBO.Update(id, userInfo);
        }

        public ResultCode DeleteUser(int id)
        {
            string roleOfCurrentUser = this.CurrentUser.RoleUser.Level;
            int? companyId = this.CurrentUser.Company.Id;
            return this.employeeBO.Delete(id,companyId, roleOfCurrentUser);
        }

        private bool IsCompanyOfUser(int companyId)
        {
            bool result = true;
            int? idCompanyOfUser = this.CurrentUser.Company.Id;
            if (!idCompanyOfUser.HasValue ||
                idCompanyOfUser.Value != companyId)
            {
                result = false;
            }

            return result;
        }

        #endregion
    }
}