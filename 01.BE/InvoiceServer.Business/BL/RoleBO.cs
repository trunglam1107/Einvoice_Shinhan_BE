using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class RoleBO : IRoleBO
    {
        #region Fields, Properties

        private readonly IRoleRepository roleRepository;
        private readonly ILoginUserRepository loginUserRepository;
        private readonly IDbTransactionManager transaction;
        private readonly IUserLevelRepository userLevelRepository;

        #endregion

        #region Contructor

        public RoleBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.roleRepository = repoFactory.GetRepository<IRoleRepository>();
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.userLevelRepository = repoFactory.GetRepository<IUserLevelRepository>();
        }

        #endregion

        #region Methods
        public IEnumerable<ROLE> GetList(ConditionSearchRole condition)
        {
            var res = this.roleRepository.GetAll()
                .Where(r => r.USERLEVELSID == condition.UserLevelSID)
                .Where(r => !(r.DELETED ?? false));
            if (!string.IsNullOrEmpty(condition.Key))
            {
                res = res.Where(r => r.NAME.ToUpper().Contains(condition.Key.ToUpper()));
            }

            return res;
        }

        public long CountList(ConditionSearchRole condition)
        {
            return this.GetList(condition)
                .Count();
        }

        public ROLE GetRoleById(long id)
        {
            return this.roleRepository.GetById(id);
        }

        public ROLE GetRoleByName(string name, long companySID)
        {
            return this.roleRepository.GetAll()
                .FirstOrDefault(r => r.NAME?.ToUpper() == name?.ToUpper() && !(r.DELETED ?? false));
        }

        public ROLE GetRoleByNameAndLevelName(string name, long companySID, string levelName)
        {
            return this.roleRepository.GetAll()
                .FirstOrDefault(r => r.NAME?.ToUpper() == name?.ToUpper() && !(r.DELETED ?? false) && r.USERLEVEL.LEVELS == levelName);
        }

        public void CreateOrEdit(RoleGroupDetail roleGroupDetail, USERLEVEL userLevel, UserSessionInfo currentUser)
        {
            try
            {
                transaction.BeginTransaction();
                if (roleGroupDetail.Id == 0)
                {

                    var userLever = new ROLE()
                    {
                        NAME = roleGroupDetail.Name,
                        USERLEVELSID = userLevel.USERLEVELSID,
                        CREATEDDATE = DateTime.Now,
                        CREATEDBY = currentUser.Id,
                    };
                    this.roleRepository.Insert(userLever);
                }
                else
                {
                    var existedRole = roleRepository.GetById(roleGroupDetail.Id);
                    existedRole.NAME = roleGroupDetail.Name;
                    existedRole.UPDATEDDATE = DateTime.Now;
                    existedRole.UPDATEDBY = currentUser.Id;
                    this.roleRepository.Update(existedRole);
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void CreateOrEditUserRoles(AccountRoleInfo accountRoleInfo, UserSessionInfo currentUser)
        {
            if (accountRoleInfo == null)
            {
                return;
            }
            try
            {
                long levelSellEmpl = GetUserLevel(RoleInfo.BRANCH).USERLEVELSID;

                transaction.BeginTransaction();
                if (accountRoleInfo.UserSID == 0)
                {
                    //Create
                    CreateUserRole(accountRoleInfo, levelSellEmpl);
                }
                else
                {
                    // Edit
                    UpdateUserRole(accountRoleInfo);
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateUsersInRole(UpdateUsersInRole usersInRole, UserSessionInfo currentUser)
        {
            var role = this.roleRepository.GetById(usersInRole.Role.Id);
            if (role == null)
            {
                return;
            }

            try
            {
                transaction.BeginTransaction();

                // Update Role
                role.NAME = usersInRole.Role.Name;
                role.UPDATEDDATE = DateTime.Now;
                role.UPDATEDBY = currentUser.Id;

                // Update RoleFunction
                role.LOGINUSERs.Clear();
                foreach (var user in usersInRole.UsersInRole)
                {
                    var loginUser = this.loginUserRepository.GetById(user.UserSID);
                    if (loginUser != null)
                    {
                        role.LOGINUSERs.Add(loginUser);
                    }
                }
                this.roleRepository.Update(role);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void CreateUsersInRole(UpdateUsersInRole usersInRole, USERLEVEL userLevel, UserSessionInfo currentUser)
        {
            var role = new ROLE();
            try
            {
                transaction.BeginTransaction();

                // Create Role
                role.NAME = usersInRole.Role.Name;
                role.CREATEDDATE = DateTime.Now;
                role.CREATEDBY = currentUser.Id;

                role.USERLEVELSID = userLevel.USERLEVELSID;


                // Create RoleFunction
                role.LOGINUSERs.Clear();
                foreach (var user in usersInRole.UsersInRole)
                {
                    var loginUser = this.loginUserRepository.GetById(user.UserSID);
                    if (loginUser != null)
                    {
                        role.LOGINUSERs.Add(loginUser);
                    }
                }
                this.roleRepository.Insert(role);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IEnumerable<ROLE> GetRolesByUserId(long userId)
        {
            return this.roleRepository.GetRolesByUserId(userId);
        }

        private void CreateUserRole(AccountRoleInfo accountRoleInfo, long levelSellEmpl)
        {
            bool isExitUserId = this.loginUserRepository.ContainUserId(accountRoleInfo.UserID);
            if (isExitUserId)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceUserId,
                                  string.Format("Create User failed because user with UserId = [{0}] is exist.", accountRoleInfo.UserID));
            }

            bool isExitEmail = this.loginUserRepository.ContainEmail(accountRoleInfo.Email);
            if (isExitEmail)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceEmail,
                                  string.Format("Create User failed because user with Email = [{0}] is exist.", accountRoleInfo.Email));
            }

            var loginUser = new LOGINUSER();
            loginUser.CopyData(accountRoleInfo);
            loginUser.COMPANYID = accountRoleInfo.CompanyId;
            loginUser.USERLEVELSID = levelSellEmpl;
            loginUser.CREATEDDATE = DateTime.Now;

            ResultCode errorCode;
            string errorMessage;
            if (!loginUser.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            if (loginUser.ROLEs == null)
            {
                loginUser.ROLEs = new List<ROLE>();
            }

            foreach (var roleInfo in accountRoleInfo.Roles)
            {
                if (roleInfo.IsRoleOfUser)
                {
                    var role = this.roleRepository.GetById(roleInfo.Id);
                    if (role != null)
                    {
                        loginUser.ROLEs.Add(role);
                    }
                }
            }

            loginUserRepository.Insert(loginUser);
        }

        private void UpdateUserRole(AccountRoleInfo accountRoleInfo)
        {

            var user = this.loginUserRepository.GetById(accountRoleInfo.UserSID);

            bool isExitEmail = this.loginUserRepository.ContainEmail(accountRoleInfo.Email);
            if (isExitEmail && user.EMAIL != accountRoleInfo.Email)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceEmail,
                                  string.Format("Create User failed because user with Email = [{0}] is exist.", accountRoleInfo.Email));
            }
            user.UPDATEDDATE = DateTime.Now;
            user.USERNAME = accountRoleInfo.UserName;
            user.EMAIL = accountRoleInfo.Email;
            user.PASSWORD = accountRoleInfo.Password;
            user.MOBILE = accountRoleInfo.Mobile;
            user.ISACTIVE = accountRoleInfo.IsActive;
            user.COMPANYID = accountRoleInfo.CompanyId;

            ResultCode errorCode;
            string errorMessage;
            if (!user.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            user.ROLEs.Clear();
            foreach (var roleInfo in accountRoleInfo.Roles)
            {
                if (roleInfo.IsRoleOfUser)
                {
                    var role = this.roleRepository.GetById(roleInfo.Id);
                    if (role != null)
                    {
                        user.ROLEs.Add(role);
                    }
                }
            }
            this.loginUserRepository.Update(user);
        }

        public ResultCode Delete(long id, UserSessionInfo currentUser)
        {
            return DeleteRole(id, currentUser);
        }

        private USERLEVEL GetUserLevel(string level)
        {
            var userLevel = this.userLevelRepository.FillterUserLevel(level).FirstOrDefault();
            if (userLevel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return userLevel;
        }

        private ResultCode DeleteRole(long id, UserSessionInfo currentUser)
        {
            var role = GetRoleById(id);
            if (role == null || role.DELETED == true)
            {
                throw new BusinessLogicException(ResultCode.RoleCanNotDelete, "This Role can not delete!");
            }

            role.DELETED = true;
            role.DELETEDDATE = DateTime.Now;
            role.DELETEDBY = currentUser.Id;

            return this.roleRepository.Update(role) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        #endregion
    }
}