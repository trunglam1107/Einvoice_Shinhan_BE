using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness'
    public class RoleBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness'
    {
        #region Fields, Properties

        private readonly IRoleBO roleBO;
        private readonly IUserLevelBO userLevelBO;
        private readonly ILoginUserBO loginUserBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.RoleBusiness(IBOFactory)'
        public RoleBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.RoleBusiness(IBOFactory)'
        {
            this.roleBO = boFactory.GetBO<IRoleBO>();
            this.userLevelBO = boFactory.GetBO<IUserLevelBO>();
            this.loginUserBO = boFactory.GetBO<ILoginUserBO>(new SessionConfig());
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.GetList(out long, string, string, string, int, int)'
        public IEnumerable<RoleGroupDetail> GetList(out long totalRecords, string key = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.GetList(out long, string, string, string, int, int)'
        {
            totalRecords = 0;
            var loginUser = this.loginUserBO.GetById(this.CurrentUser.Id, this.CurrentUser.Company.Id);
            if (loginUser == null)
                return new List<RoleGroupDetail>();
            var condition = new ConditionSearchRole(this.CurrentUser, key, RoleInfo.Id[loginUser.USERLEVEL.LEVELS], orderby, orderType);
            var roles = this.roleBO.GetList(condition);
            totalRecords = roleBO.CountList(condition);

            var roleDetails = roles.AsQueryable()
                    .OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc))
                    .Skip(skip)
                    .Take(take)
                    .ToList();

            return roleDetails.Select(r => new RoleGroupDetail(r));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.GetAllRole()'
        public IEnumerable<RoleGroupDetail> GetAllRole()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.GetAllRole()'
        {
            var loginUser = this.loginUserBO.GetById(this.CurrentUser.Id, this.CurrentUser.Company.Id);
            if (loginUser == null)
                return new List<RoleGroupDetail>();
            var condition = new ConditionSearchRole(this.CurrentUser, RoleInfo.Id[loginUser.USERLEVEL.LEVELS]);
            var roles = this.roleBO.GetList(condition);
            var roleDetails = roles.AsQueryable().OrderByDescending(x=>x.ID)
                    .ToList();

            return roleDetails.Select(r => new RoleGroupDetail(r));
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.GetRoleGroup(long)'
        public RoleGroupDetail GetRoleGroup(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.GetRoleGroup(long)'
        {
            var role = this.roleBO.GetRoleById(id);
            return new RoleGroupDetail(role);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.CreateOrEdit(RoleGroupDetail)'
        public void CreateOrEdit(RoleGroupDetail roleGroupDetail)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.CreateOrEdit(RoleGroupDetail)'
        {
            var role = this.userLevelBO.getByRoleSessionInfo(this.CurrentUser.RoleUser);
            if (role == null)
                return;

            this.roleBO.CreateOrEdit(roleGroupDetail, role, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.UpdateUsersInRole(UpdateUsersInRole)'
        public void UpdateUsersInRole(UpdateUsersInRole usersInRole)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.UpdateUsersInRole(UpdateUsersInRole)'
        {
            var userLevel = this.userLevelBO.getByRoleSessionInfo(this.CurrentUser.RoleUser);
            if (userLevel == null)
                return;

            this.ValidatUpdateRole(usersInRole);

            this.roleBO.UpdateUsersInRole(usersInRole, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.CreateUsersInRole(UpdateUsersInRole)'
        public void CreateUsersInRole(UpdateUsersInRole usersInRole)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.CreateUsersInRole(UpdateUsersInRole)'
        {
            var userLevel = this.userLevelBO.getByRoleSessionInfo(this.CurrentUser.RoleUser);
            if (userLevel == null)
                return;

            this.ValidateCreateRole(usersInRole);

            this.roleBO.CreateUsersInRole(usersInRole, userLevel, this.CurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.CreateOrEditUserRoles(AccountRoleInfo)'
        public void CreateOrEditUserRoles(AccountRoleInfo accountRoleInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.CreateOrEditUserRoles(AccountRoleInfo)'
        {
            var role = this.userLevelBO.getByRoleSessionInfo(this.CurrentUser.RoleUser);
            if (role == null)
                return;
            this.roleBO.CreateOrEditUserRoles(accountRoleInfo, this.CurrentUser);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.GetRolesByUserId(long)'
        public AccountRoleInfo GetRolesByUserId(long userId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.GetRolesByUserId(long)'
        {
            // Check nếu userId == 0 thì là tạo mới
            var user = new Data.DBAccessor.LOGINUSER();
            if (userId > 0)
            {
                user = this.loginUserBO.GetById(userId, null);
            }

            var listRole = this.GetList(out long totalRecord);
            var res = new AccountRoleInfo(user);
            foreach (var role in listRole)
            {
                var roleGroupView = new RoleGroupView(role);
                var count = user.ROLEs.Count(r => r.ID == role.Id);
                if (count > 0)
                {
                    roleGroupView.IsRoleOfUser = true;
                }
                else
                {
                    roleGroupView.IsRoleOfUser = false;
                }
                res.Roles.Add(roleGroupView);
            }
            return res;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleBusiness.Delete(long)'
        {
            return this.roleBO.Delete(id, CurrentUser);
        }

        #endregion

        #region Validate

        private void ValidateCreateRole(UpdateUsersInRole usersInRole)
        {
            if (string.IsNullOrWhiteSpace(usersInRole.Role.Name))
            {
                throw new BusinessLogicException(ResultCode.RoleNameIsNotEmptyOrWhitespace, $"Role name can not empty or whitespace");
            }

            var role = this.roleBO.GetRoleByNameAndLevelName(usersInRole.Role.Name, this.CurrentUser.Company.Id ?? 0, this.CurrentUser.RoleUser.Level);
            if (role != null)
            {
                throw new BusinessLogicException(ResultCode.CreateRoleNameDuplicate, $"Role with name {usersInRole.Role.Name} already exists!");
            }
        }

        private void ValidatUpdateRole(UpdateUsersInRole usersInRole)
        {

            if (string.IsNullOrWhiteSpace(usersInRole.Role.Name))
            {
                throw new BusinessLogicException(ResultCode.RoleNameIsNotEmptyOrWhitespace, $"Role name can not empty or whitespace");
            }

            var role = this.roleBO.GetRoleByNameAndLevelName(usersInRole.Role.Name, this.CurrentUser.Company.Id ?? 0, this.CurrentUser.RoleUser.Level);
            if (role != null && role.NAME.ToUpper() == usersInRole.Role.Name.ToUpper() && role.ID != usersInRole.Role.Id)
            {
                throw new BusinessLogicException(ResultCode.EditRoleNameDuplicate, $"Role with name {usersInRole.Role.Name} already exists!");
            }
        }
        #endregion
    }
}