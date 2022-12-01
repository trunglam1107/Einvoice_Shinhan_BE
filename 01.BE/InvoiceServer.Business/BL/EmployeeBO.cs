using InvoiceServer.Business.Cache;
using InvoiceServer.Common.Constants;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class EmployeeBO : IEmployeeBO
    {
        #region Fields, Properties
        // <summary>
        /// Last userId in the table
        /// </summary>
        private static Logger logger = new Logger();
        private readonly ILoginUserRepository loginUserRepository;
        private readonly IRepositoryFactory repoFactory;
        private readonly Permission permission;
        private readonly IDbTransactionManager transaction;

        private ResultCode errorCode;
        private string errorMessage;
        #endregion

        #region Contructor

        public EmployeeBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.NotNull(repoFactory, "repoFactory");
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.repoFactory = repoFactory;
            this.permission = new Permission(repoFactory);
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
        }
        #endregion

        #region LoginUserBO Methods
        public IEnumerable<EmployeeInfo> FillterEmployee(ConditionSearchEmployee condition, int skip = int.MinValue, int take = int.MaxValue)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var loginUsers = this.loginUserRepository.FillterEmployee(condition).AsQueryable()
                .OrderBy(condition.Order_By, condition.Order_Type.Equals(OrderType.Desc)).Skip(skip).Take(take).ToList();

            return loginUsers.Select(p => new EmployeeInfo(p));
        }

        public int CountFillterEmployee(ConditionSearchEmployee condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.loginUserRepository.FillterEmployee(condition).Count();
        }

        public EmployeeInfo GetEmployeeInfo(int id, int? companyId, string level)
        {
            var currentEmployee = GetById(id, companyId);
            EmployeeInfo employeeInfo = new EmployeeInfo(currentEmployee);
            employeeInfo.Roles = this.permission.GetPermissionOfUser(currentEmployee);
            return employeeInfo;
        }

        public ResultCode Create(EmployeeInfo userInfo)
        {
            if (userInfo == null || !userInfo.CompanyId.HasValue)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            try
            {
                transaction.BeginTransaction();
                var companyOfUser = GetCompanyById(userInfo.CompanyId.Value);
                var loginUser = CreateLoginUser(userInfo, companyOfUser);
                this.permission.CreatePermission(loginUser, userInfo.Roles);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }

            return ResultCode.NoError;
        }

        public ResultCode Update(int id, EmployeeInfo userInfo)
        {
            try
            {
                transaction.BeginTransaction();
                var currentUser = GetById(id, userInfo.CompanyId);
                currentUser = UpdateLoginUser(currentUser, userInfo);
                this.permission.UpdatePermission(currentUser, userInfo.Roles);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            return ResultCode.NoError;

        }

        public ResultCode Delete(int id, int? companyId, string roleOfUserDelete)
        {
            var currentUser = GetById(id, companyId);
            if (!currentUser.USERLEVEL.LEVELS.IsEquals(RoleInfo.SALE))
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtNotPermissionDelete,
                                 "Delete User failed because current user not permission deleted.");
            }
            currentUser.DELETED = true;
            currentUser.UPDATEDDATE = DateTime.Now;
            UserSessionCache.Instance.RemoveSessionByUserId(id);
            return loginUserRepository.Update(currentUser) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        #endregion LoginUserBO Methods

        #region Private methods
        private USERLEVEL GetUserLevel(string level)
        {
            var userLevel = this.repoFactory.GetRepository<IUserLevelRepository>().FillterUserLevel(level).FirstOrDefault();
            if (userLevel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return userLevel;
        }

        private USERLEVEL GetUserLevel(int Id)
        {
            var userLevel = this.repoFactory.GetRepository<IUserLevelRepository>().FirstOrDefault(p => p.USERLEVELSID == Id);
            if (userLevel == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return userLevel;
        }

        private MYCOMPANY GetCompanyById(int companyId)
        {
            var company = this.repoFactory.GetRepository<IMyCompanyRepository>().GetById(companyId);
            if (company == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId,
                                   string.Format("Get MyCompany info failed because Country with CompanySID =[{0}] not found.", companyId));
            }

            return company;
        }

        private LOGINUSER CreateLoginUser(EmployeeInfo employeeInfo, MYCOMPANY companyInfo)
        {
            bool isExitUserId = this.loginUserRepository.ContainUserId(employeeInfo.UserID);
            if (isExitUserId)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceUserId,
                                  string.Format("Create User failed because user with UserId = [{0}] is exist.", employeeInfo.UserID));
            }

            bool isExitEmail = this.loginUserRepository.ContainEmail(employeeInfo.Email);
            if (isExitEmail)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceEmail,
                                  string.Format("Create User failed because user with Email = [{0}] is exist.", employeeInfo.Email));
            }

            var loginUserInfo = new LOGINUSER();
            var userLevel = GetUserLevel(RoleInfo.SALE_EMPLOYER);
            loginUserInfo.CopyData(employeeInfo);
            loginUserInfo.COMPANYID = companyInfo.COMPANYSID;
            loginUserInfo.USERLEVELSID = userLevel.USERLEVELSID;
            loginUserInfo.CREATEDDATE = DateTime.Now;
            if (!loginUserInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            loginUserRepository.Insert(loginUserInfo);
            loginUserInfo.USERLEVEL = userLevel;
            return loginUserInfo;
        }

        private LOGINUSER UpdateLoginUser(LOGINUSER currentUser, EmployeeInfo userInfo)
        {
            if (!currentUser.USERID.IsEquals(userInfo.UserID)        // UserId is changed
                   && this.loginUserRepository.ContainUserId(userInfo.UserID))   // New UserId is existed
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceUserId,
                                string.Format("Update UserLogin with userId [{0}] exist", userInfo.UserID));
            }

            if (!currentUser.EMAIL.IsEquals(userInfo.Email)        // Email is changed
               && this.loginUserRepository.ContainEmail(userInfo.Email)) // New Email is existed
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceEmail,
                                string.Format("Update UserLogin with Email [{0}] exist", userInfo.Email));
            }

            var currentTime = DateTime.Now;
            currentUser.CopyData(userInfo);
            var userLevel = GetUserLevel(RoleInfo.SALE_EMPLOYER);
            currentUser.USERLEVELSID = userLevel.USERLEVELSID;
            currentUser.UPDATEDDATE = currentTime;
            if (!currentUser.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            this.loginUserRepository.Update(currentUser);
            currentUser.USERLEVEL = userLevel;
            return currentUser;
        }

        private LOGINUSER GetById(int id, int? companyId)
        {
            var currentUser = this.loginUserRepository.GetById(id);
            if (currentUser == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId,
                                  string.Format("Get LoginUser failed because LoginUser with ID =[{0}] not found.", id));
            }

            if (companyId.HasValue && companyId.Value != currentUser.COMPANYID)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid,
                                 string.Format("Get LoginUser failed because LoginUser with company =[{0}] not mapping.", id));
            }

            return currentUser;
        }

        #endregion
    }
}