using InvoiceServer.Business.Cache;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Account;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Common.Utils;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class LoginUserBO : ILoginUserBO
    {
        #region Fields, Properties
        // <summary>
        /// Last userId in the table
        /// </summary>
        private readonly ILoginUserRepository loginUserRepository;
        private readonly IRepositoryFactory repoFactory;
        private readonly Permission permission;
        private readonly IRoleRepository roleRepository;
        private readonly IDbTransactionManager transaction;
        private readonly IConfigEmailServerRepository configEmailServerRepository;
        private readonly IUVGetFunctionOfUserRepository uV_GetFunctionOfUserRepository;
        private readonly ISystemSettingRepository systemSettingRepository;
        private readonly IMyCompanyRepository myCompanyRepository;
        private readonly TimeSpan tokenResetPasswordExpire;
        private readonly bool createTokenRandom;
        private readonly PrintConfig printConfig;
        #endregion

        #region Contructor

        public LoginUserBO(IRepositoryFactory repoFactory, SessionConfig webConfig)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.myCompanyRepository = repoFactory.GetRepository<IMyCompanyRepository>();
            this.loginUserRepository = repoFactory.GetRepository<ILoginUserRepository>();
            this.roleRepository = repoFactory.GetRepository<IRoleRepository>();
            this.repoFactory = repoFactory;
            this.permission = new Permission(repoFactory);
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            SessionConfig config = webConfig;

            this.configEmailServerRepository = repoFactory.GetRepository<IConfigEmailServerRepository>();
            this.uV_GetFunctionOfUserRepository = repoFactory.GetRepository<IUVGetFunctionOfUserRepository>();
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
            this.createTokenRandom = config.CreateTokenRandom;
            this.tokenResetPasswordExpire = config.SessionResetPasswordExpire;
        }
        public LoginUserBO(IRepositoryFactory repoFactory, SessionConfig webConfig, PrintConfig printConfig)
            : this(repoFactory, webConfig)
        {
            this.printConfig = printConfig;
        }
        #endregion

        #region LoginUserBO Methods
        public IEnumerable<AccountDetail> FillterUser(ConditionSearchUser condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var loginUsers = this.loginUserRepository.FillterUser(condition).AsQueryable()
                .OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();

            return loginUsers.Select(p => new AccountDetail(p));
        }

        public long CountFillterUser(ConditionSearchUser condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.loginUserRepository.FillterUser(condition).Count();
        }
        public IEnumerable<AccountDetailLogin> FillterUserOutRole(ConditionSearchUser condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var loginUsers = this.loginUserRepository.FillterUserOutRole(condition).AsQueryable()
                .OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();

            return loginUsers.Select(p => new AccountDetailLogin(p));
        }

        public AccountInfo GetAccountInfo(long id, long? companyId, string level)
        {
            var currentLoginUser = GetById(id, companyId);
            AccountInfo accountInfo = new AccountInfo(currentLoginUser);
            accountInfo.Roles = this.permission.GetPermissionOfUser(currentLoginUser);
            return accountInfo;
        }

        public ResultCode Create(AccountInfo userInfo)
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
            catch
            {
                transaction.Rollback();
                throw;
            }

            return ResultCode.NoError;
        }

        public ResultCode Update(long id, AccountInfo userInfo)
        {
            if (userInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            try
            {
                transaction.BeginTransaction();
                var currentUser = GetById(id, userInfo.CompanyId);
                currentUser = UpdateLoginUser(currentUser, userInfo);
                this.permission.UpdatePermission(currentUser, userInfo.Roles);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return ResultCode.NoError;
        }

        public ResultCode ChangePassword(long id, PasswordInfo passowrdInfo)
        {
            if (passowrdInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode errorCode;
            string errorMessage;
            if (!passowrdInfo.IsReset && !passowrdInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            var currentUser = GetById(id, null);
            if (!passowrdInfo.IsReset && !currentUser.PASSWORD.IsEquals(passowrdInfo.CurrentPassword))
            {
                throw new BusinessLogicException(ResultCode.CountryAdminMgtPasswordInvalid,
                                    string.Format("Change password faild because passwrod not match with current password = [{0}].", passowrdInfo.CurrentPassword));
            }

            var currentTime = DateTime.Now;
            currentUser.PASSWORD = passowrdInfo.NewPassword;
            currentUser.UPDATEDDATE = currentTime;
            currentUser.LASTCHANGEDPASSWORDTIME = currentTime;
            return this.loginUserRepository.Update(currentUser) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public ResultCode ChangeLanguage(long id, string language)
        {
            if (language == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var currentUser = GetById(id, null);
            if (language != currentUser.LANGUAGE)
            {
                currentUser.LANGUAGE = language;
                this.loginUserRepository.Update(currentUser);
            }
            return ResultCode.NoError;
        }

        public ResultCode Delete(long id, long? companyId, string roleOfUserDelete)
        {
            var currentUser = GetById(id, null);
            var isUsing = this.loginUserRepository.LoginUserUsing(id);
            if (isUsing > 0)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtIsReferrenced, $"Store procedure SP_LOGINUSER_USING return code: {isUsing}{Environment.NewLine}USERSID delete: {id}");
            }

            try
            {
                transaction.BeginTransaction();
                currentUser.ROLEs.Clear();
                this.loginUserRepository.Delete(currentUser);
                UserSessionCache.Instance.RemoveSessionByUserId(id);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return ResultCode.NoError;
        }

        public ResultCode UpdateCurrentUser(LOGINUSER loginUser)
        {
            return this.loginUserRepository.Update(loginUser) ? ResultCode.NoError : ResultCode.UnknownError;
        }

        public LOGINUSER GetById(long id, long? companyId)
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

        public AccountViewModel GetCustomerById(long id, long? companyId)
        {
            LOGINUSER currentUser = this.loginUserRepository.GetById(id);
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

            if (!currentUser.IsNullOrEmpty())
            {
                return new AccountViewModel()
                {
                    UserSID = currentUser.USERSID,
                    UserName = currentUser.USERNAME,
                    UserID = currentUser.USERID,
                    CompanyId = currentUser.USERSID,
                    Email = currentUser.EMAIL,
                    CreatedDate = currentUser.CREATEDDATE,
                    UpdatedDate = currentUser.UPDATEDDATE,
                    LastAccessedTime = currentUser.LASTACCESSEDTIME,
                    LastChangedpasswordTime = currentUser.LASTCHANGEDPASSWORDTIME,
                };
            }
            return new AccountViewModel();
        }
        public ResultImportSheet ImportData(string fullPathFile, long companyId, UserSessionInfo userSessionInfo)
        {

            var rowError = new ListError<ImportRowError>();
            var listResultImport = new ResultImportSheet();
            ImportExcel importExcel = new ImportExcel(fullPathFile, ImportAccount.SheetNames);
            DataTable dtClients = importExcel.GetBySheetName(ImportAccount.SheetName, ImportAccount.ColumnImport, ref rowError);
            if (companyId != 1)
            {
                foreach (DataRow row in dtClients.Rows)
                {
                    if (int.Parse(row["ChiNhanh"].ToString().Substring(0, 2).Replace("-", "")) != companyId)
                    {
                        row.Delete();
                    }
                }
                dtClients.AcceptChanges();
            }

            if (rowError.Count > 0)
            {
                listResultImport.RowError.AddRange(rowError);
                listResultImport.ErrorCode = ResultCode.ImportFileFormatInvalid;
                listResultImport.Message = MsgApiResponse.DataInvalid;
                return listResultImport;
            }

            if (dtClients == null || dtClients.Rows.Count == 0)
            {
                throw new BusinessLogicException(ResultCode.FileUploadImportEmpty, "File upload Import Empty");
            }
            try
            {
                transaction.BeginTransaction();
                listResultImport = ProcessData(dtClients);
                if (listResultImport.ErrorCode == ResultCode.NoError)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new BusinessLogicException(ResultCode.UnknownError, ex.Message);
            }

            return listResultImport;
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

        private LOGINUSER GetUserByEmail(string email)
        {
            var currentUser = this.loginUserRepository.GetByEmail(email);
            if (currentUser == null)
            {
                throw new BusinessLogicException(ResultCode.LoginEmailNotExist,
                                   string.Format("Email does not exist: {0}", email));
            }

            return currentUser;
        }

        private MYCOMPANY GetCompanyById(long companyId)
        {
            var company = this.repoFactory.GetRepository<IMyCompanyRepository>().GetById(companyId);
            if (company == null)
            {
                throw new BusinessLogicException(ResultCode.NotFoundResourceId,
                                   string.Format("Get MyCompany info failed because Country with CompanySID =[{0}] not found.", companyId));
            }

            return company;
        }

        private LOGINUSER CreateLoginUser(AccountInfo userInfo, MYCOMPANY companyInfo)
        {
            bool isExitUserId = this.loginUserRepository.ContainUserId(userInfo.UserID);
            if (isExitUserId)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceUserId,
                                  string.Format("Create User failed because user with UserId = [{0}] is exist.", userInfo.UserID));
            }

            bool isExitEmail = this.loginUserRepository.ContainEmail(userInfo.Email);
            if (isExitEmail)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceEmail,
                                  string.Format("Create User failed because user with Email = [{0}] is exist.", userInfo.Email));
            }

            var loginUserInfo = new LOGINUSER();
            var userLevel = GetUserLevel(RoleInfo.BRANCH);
            loginUserInfo.CopyData(userInfo);
            loginUserInfo.COMPANYID = companyInfo.COMPANYSID;
            loginUserInfo.USERLEVELSID = userLevel.USERLEVELSID;
            loginUserInfo.CREATEDDATE = DateTime.Now;

            ResultCode errorCode;
            string errorMessage;
            if (!loginUserInfo.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            loginUserRepository.Insert(loginUserInfo);
            loginUserInfo.USERLEVEL = userLevel;
            return loginUserInfo;
        }

        private LOGINUSER UpdateLoginUser(LOGINUSER currentUser, AccountInfo userInfo)
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
            var userLevel = GetUserLevel(RoleInfo.BRANCH);
            currentUser.USERLEVELSID = userLevel.USERLEVELSID;
            currentUser.UPDATEDDATE = currentTime;

            ResultCode errorCode;
            string errorMessage;
            if (!currentUser.IsValid(out errorCode, out errorMessage))
            {
                throw new BusinessLogicException(errorCode, errorMessage);
            }

            this.loginUserRepository.Update(currentUser);
            currentUser.USERLEVEL = userLevel;
            return currentUser;
        }

        public IEnumerable<AccountViewModel> GetListAccount(ConditionSearchUser condition)
        {
            var rs = this.loginUserRepository.GetListAccount(condition);
            return rs;
        }
        public long CountListAccount(ConditionSearchUser condition)
        {
            long rs = this.loginUserRepository.CountListAccount(condition);
            return rs;
        }

        public List<AccountViewModel> DownloadDataAccount(ConditionSearchUser condition)
        {
            var rs = GetListAccount(condition).ToList();
            return rs;
        }

        public ResultCode UpdatePasswordStatus(long id)
        {
            LOGINUSER loginUser = loginUserRepository.GetAccountById(id);
            loginUser.PASSWORDSTATUS = (int)PasswordStatus.NotEncript;

            var isUpdateSuccess = loginUserRepository.Update(loginUser);
            if (isUpdateSuccess)
            {

                return ResultCode.NoError;
            }
            return ResultCode.InvoiceIssuedNotUpdate;
        }
        public UserSessionInfo ResetPassword(ResetPassword resetPasswordInfo)
        {
            // Validate data
            if (resetPasswordInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode resultCode;
            string errorDetail;
            if (!resetPasswordInfo.IsValid(out resultCode, out errorDetail))
            {
                throw new BusinessLogicException(resultCode, errorDetail);
            }

            var loginUser = GetUserByEmail(resetPasswordInfo.Email);
            UpdatePasswordStatus(loginUser.USERSID);
            UserSessionInfo userSessionInfo = GetSessionInfo(loginUser);
            userSessionInfo.EmailServer = GetEmailServer();
            UserSessionCache.Instance.SaveUserSession(userSessionInfo, this.tokenResetPasswordExpire);

            return userSessionInfo;
        }
        private UserSessionInfo GetSessionInfo(LOGINUSER loginUser)
        {
            string token = this.createTokenRandom ? Guid.NewGuid().ToString("N") : loginUser.USERID;
            UserSessionInfo userSessionInfo = new UserSessionInfo(loginUser, loginUser.USERLEVEL, loginUser.MYCOMPANY, token);
            var roleOfUser = this.uV_GetFunctionOfUserRepository.GetByUserSID(loginUser.USERSID, loginUser.COMPANYID);
            userSessionInfo.RoleUser.Permissions = GetPermissions(roleOfUser);
            userSessionInfo.EmailServer = GetEmailServer();
            userSessionInfo.SystemSetting = this.systemSettingRepository.FilterSystemSetting(loginUser.COMPANYID ?? 0);
            userSessionInfo.Language = loginUser.LANGUAGE;
            return userSessionInfo;
        }
        public EmailServerInfo GetEmailServer()
        {
            var emailServer = this.configEmailServerRepository.GetByCompany();
            if (emailServer == null)
            {
                return null;
            }

            return new EmailServerInfo(emailServer);
        }
        private List<string> GetPermissions(IEnumerable<UV_GETFUNCTIONOFUSER> roleOfUsers)
        {
            List<string> permissionOfUser = new List<string>();
            if (roleOfUsers == null)
            {
                return permissionOfUser;
            }

            roleOfUsers.ForEach(p =>
            {

                if (p.ACTION.IsNotNullOrEmpty())
                {
                    foreach (var item in p.ACTION.ToArray())
                    {
                        permissionOfUser.Add(string.Format("{0}_{1}", p.FUNCTIONNAME, item));
                    }
                }
            });

            return permissionOfUser;
        }
        private ResultImportSheet ProcessData(DataTable dtDistintc)
        {
            List<AccountImport> list = new List<AccountImport>();
            ResultImportSheet listResultImport = new ResultImportSheet();
            int rowIndex = 1;
            foreach (DataRow row in dtDistintc.Rows)
            {
                if (CheckRowIsEmpty(row))
                {
                    rowIndex++;
                    continue;
                }
                AccountImport model = new AccountImport()
                {
                    STT = row[ImportAccount.STT].ToString(),
                    Branch = row[ImportAccount.Branch].ToString(),
                    AccountName = row[ImportAccount.AccountName].ToString(),
                    Username = row[ImportAccount.Username].ToString(),
                    Phone = row[ImportAccount.Phone].ToString(),
                    Email = row[ImportAccount.Email].ToString(),
                    RoleName = row[ImportAccount.RoleName].ToString(),
                    ActiveDate = row[ImportAccount.ActiveDate].ToString(),
                    ExpireDate = row[ImportAccount.ExpireDate].ToString(),
                    Status = row[ImportAccount.Status].ToString(),
                };
                list.Add(model);
                listResultImport.RowError.AddRange(Validate(row, rowIndex));
                rowIndex++;
            }
            listResultImport.RowError.AddRange(Excute(list));
            if (listResultImport.RowError.Count == 0)
            {
                listResultImport.RowSuccess = (rowIndex - 2);
                listResultImport.ErrorCode = ResultCode.NoError;
                listResultImport.Message = MsgApiResponse.ExecuteSeccessful;
            }
            else
            {
                listResultImport.ErrorCode = ResultCode.ImportDataNotSuccess;
                listResultImport.Message = MsgApiResponse.DataInvalid;
            }

            return listResultImport;
        }
        private bool CheckRowIsEmpty(DataRow row)
        {
            foreach (var column in ImportAccount.ColumnImport)
            {
                if (!row[column].IsNullOrWhitespace())
                {
                    return false;
                }
            }

            return true;
        }
        private ImportRowError ValidDataIsNull(DataRow row, string colunName, int rowIndex)
        {
            ImportRowError errorColumn = null;
            try
            {
                if (!row[colunName].IsNullOrWhitespace())
                {
                    return errorColumn;
                }

                return new ImportRowError(ResultCode.ImportDataIsEmpty, colunName, rowIndex, "");
            }
            catch
            {
                return errorColumn;
            }
        }
        private ListError<ImportRowError> ValidDataDoesExits(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            string branch = ImportAccount.Branch;
            MYCOMPANY getByBranchID = this.myCompanyRepository.GetByBranchId(row[branch].ToString().Substring(0, row[branch].ToString().IndexOf('-')).Replace("-", ""));
            if (getByBranchID == null)
            {
                listError.Add(new ImportRowError(ResultCode.ImportColumnIsNotExist, ImportAccount.Branch, rowIndex, row[ImportAccount.Branch].ToString()));
            }
            string role = ImportAccount.RoleName;
            if (this.roleRepository.GetById(int.Parse(row[role].ToString().Substring(0, row[role].ToString().IndexOf('-')).Replace("-", ""))) == null && int.Parse(row[role].ToString().Substring(0, 2)) != 0)
            {
                listError.Add(new ImportRowError(ResultCode.ImportColumnIsNotExist, ImportAccount.RoleName, rowIndex, row[ImportAccount.RoleName].ToString()));
            }
            return listError;
        }

        private ListError<ImportRowError> Validate(DataRow row, int rowIndex)
        {
            var listError = new ListError<ImportRowError>();
            ImportRowError check = ValidDataIsNull(row, ImportAccount.Username, rowIndex);
            listError.Add(check);
            listError.AddRange(ValidDataDoesExits(row, rowIndex));
            return listError;
        }


        private ListError<ImportRowError> Excute(List<AccountImport> inpData)
        {
            var listError = new ListError<ImportRowError>();
            int rowIndex = 1;
            MYCOMPANY getCompany = new MYCOMPANY();
            string branchId = null;
            string newPassword = null;
            foreach (AccountImport data in inpData)
            {
                try
                {
                    var loginUserInfo = new LOGINUSER();
                    branchId = data.Branch.Substring(0, data.Branch.IndexOf('-')).Replace("-", "").IsNumeric() ? int.Parse(data.Branch.Substring(0, data.Branch.IndexOf('-'))).ToString() : "1";
                    getCompany = this.myCompanyRepository.GetByBranchId(branchId);
                    if (getCompany != null)
                    {
                        loginUserInfo.USERID = data.Username.Trim();
                        loginUserInfo.USERNAME = data.AccountName.Trim();
                        loginUserInfo.EMAIL = data.Email.Trim();
                        newPassword = data.Username.Trim() + "@123";
                        loginUserInfo.PASSWORD = newPassword;
                        loginUserInfo.USERLEVELSID = 1;
                        loginUserInfo.COMPANYID = getCompany.COMPANYSID;
                        loginUserInfo.LANGUAGE = null;
                        if (data.Status.Trim().Equals("0"))
                        {
                            loginUserInfo.ISACTIVE = data.Status.Trim().Equals("0");
                        }
                        else
                        {
                            loginUserInfo.ISACTIVE = data.Status.Trim().Equals("1");
                        }

                        loginUserInfo.DELETED = null;
                        loginUserInfo.CREATEDDATE = String.IsNullOrEmpty(data.ActiveDate.Trim()) ? DateTime.Now : Convert.ToDateTime(DateTime.ParseExact(data.ActiveDate.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture));
                        loginUserInfo.UPDATEDDATE = null;
                        loginUserInfo.ROLEs = this.addRoleList((data.RoleName.Substring(0, data.RoleName.IndexOf('-')).Replace("-", "").IsNumeric() ? int.Parse(data.RoleName.Substring(0, data.RoleName.IndexOf('-'))) : 1));
                        loginUserInfo.LASTACCESSEDTIME = null;
                        loginUserInfo.LASTCHANGEDPASSWORDTIME = null;
                        loginUserInfo.TERMOFUSE = null;
                        loginUserInfo.MOBILE = data.Phone.Trim();
                        loginUserInfo.CLIENTID = null;
                        loginUserInfo.PASSWORDSTATUS = null;
                        loginUserInfo.USERLEVEL = GetUserLevel(RoleInfo.BRANCH);
                        LOGINUSER thisLogin = this.loginUserRepository.getByUserID(loginUserInfo.USERID);
                        if (thisLogin != null)
                        {
                            if (this.loginUserRepository.getAccountByUserID(loginUserInfo.USERID))
                            {
                                loginUserInfo.USERSID = thisLogin.USERSID;
                            }
                        }
                        
                    }


                    listError.AddRange(InsertClient(loginUserInfo, getRoleList((data.RoleName.Substring(0, 2).Replace("-", "").IsNumeric() ? int.Parse(data.RoleName.Substring(0, 2)) : 1)), rowIndex, (data.RoleName.Substring(0, data.RoleName.IndexOf('-')).Replace("-", "").IsNumeric() ? int.Parse(data.RoleName.Substring(0, data.RoleName.IndexOf('-'))) : 1)));
                    rowIndex++;

                }
                catch (Exception)
                {
                    listError.Add(new ImportRowError(ResultCode.DataInvalid, ImportAccount.ActiveDate, rowIndex, null));
                }
            }
            return listError;

        }
        private ListError<ImportRowError> InsertClient(LOGINUSER login, RoleGroupView role, int rowIndex, int userSessionInfo)
        {
            var listError = new ListError<ImportRowError>();
            bool isUSERID = this.loginUserRepository.ContainUserId(login.USERID);

            if (isUSERID && login.USERSID != 0)
            {
                try
                {
                    LOGINUSER update = this.loginUserRepository.GetAccountById(login.USERSID);
                    update.USERID = login.USERID;
                    update.USERNAME = login.USERNAME;
                    update.MOBILE = login.MOBILE;
                    update.EMAIL = login.EMAIL;
                    update.PASSWORD = login.USERID.Equals("admin") ? "admin" : login.PASSWORD;
                    update.ROLEs.Clear();
                    var roless = this.roleRepository.GetById(role.Id);
                    if (roless != null)
                    {
                        update.ROLEs.Add(roless);
                    }
                    update.ISACTIVE = login.ISACTIVE;
                    update.COMPANYID = login.COMPANYID;
                    update.ACTIVATIONDATE = login.ACTIVATIONDATE;
                    update.EFFECTIVEDATE = login.EFFECTIVEDATE;
                    this.loginUserRepository.Update(update);
                    AccountInfo info = new AccountInfo()
                    {
                        CompanyId = login.COMPANYID,
                        Email = login.EMAIL,
                        Mobile = login.MOBILE,
                        Password = login.PASSWORD,
                        UserID = login.USERID,
                        UserName = login.USERNAME,
                        UserSID = login.USERSID
                    };
                    var log = this.permission.GetPermissionOfUser(login);
                    info.Roles = log;
                    var currentUser = GetById(login.USERSID, info.CompanyId);
                    this.permission.UpdatePermission(currentUser, info.Roles);

                }
                catch (Exception)
                {
                    listError.Add(new ImportRowError(ResultCode.ImportColumnIsNotExist, ImportAccount.Branch, rowIndex, login.MYCOMPANY.ToString()));
                    transaction.Rollback();
                }
            }
            else
            {
                try
                {

                    AccountRoleInfo info = new AccountRoleInfo()
                    {
                        CompanyId = login.COMPANYID,
                        Email = login.EMAIL,
                        Mobile = login.MOBILE,
                        Password = login.PASSWORD,
                        UserID = login.USERID,
                        UserName = login.USERNAME,
                        UserSID = login.USERSID,
                        IsActive = login.ISACTIVE ?? false
                    };
                    this.CreateUserRole(info, userSessionInfo);


                }
                catch (Exception ex)
                {
                    listError.Add(new ImportRowError(ResultCode.ImportDataIsExisted, ImportAccount.Email, rowIndex, login.EMAIL.ToString()));

                }
            }
            if (listError.Count > 0)
            {
                return listError;
            }
            return listError;
        }
        private void CreateUserRole(AccountRoleInfo accountRoleInfo, long id)
        {
            bool isExitUserId = this.loginUserRepository.ContainUserId(accountRoleInfo.UserID);
            if (isExitUserId)
            {
                throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceUserId,
                                  string.Format("Create User failed because user with UserId = [{0}] is exist.", accountRoleInfo.UserID));
            }

            //bool isExitEmail = this.loginUserRepository.ContainEmail(accountRoleInfo.Email);
            //if (isExitEmail)
            //{
            //    throw new BusinessLogicException(ResultCode.UserAccountMgtConflictResourceEmail,
            //                      string.Format("Create User failed because user with Email = [{0}] is exist.", accountRoleInfo.Email));
            //}

            var loginUser = new LOGINUSER();
            loginUser.CopyData(accountRoleInfo);
            loginUser.COMPANYID = accountRoleInfo.CompanyId;
            loginUser.USERLEVELSID = 1;
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
            var role = this.roleRepository.GetById(id);
            if (role != null)
            {
                loginUser.ROLEs.Add(role);
            }
            loginUserRepository.Insert(loginUser);
        }

        private RoleGroupView getRoleList(long id)
        {
            var result = this.roleRepository.GetAll().ToList();
            var filter = (from rol in result
                          where rol.ID == id
                          select rol).FirstOrDefault();

            RoleGroupView role = new RoleGroupView()
            {
                Id = filter.ID,
                Name = filter.NAME,
                IsRoleOfUser = true
            };


            return role;
        }
        private ICollection<ROLE> addRoleList(long id)
        {
            var result = this.roleRepository.GetAll();
            return (from rol in result
                    where rol.ID == id
                    select rol).ToList();
        }

        public ExportFileInfo DownloadClientAccount(ConditionSearchUser condition)
        {
            var dataRport = new ClientAccountExport();
            var clients = this.loginUserRepository.GetListAccount(condition);
            dataRport.Items = clients.Select(p => new AccountViewModel(p)).ToList();

            ExportClientAccountView export = new ExportClientAccountView(dataRport, printConfig);
            return export.ExportFile();
        }
        #endregion

    }
}