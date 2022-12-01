using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Account;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public class LoginUserRepository : GenericRepository<LOGINUSER>, ILoginUserRepository
    {
        public LoginUserRepository(IDbContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Get an IEnumerable LoginUser 
        /// </summary>
        /// <returns><c>IEnumerable LoginUser</c> if LoginUser not Empty, <c>null</c> otherwise</returns>
        public IEnumerable<LOGINUSER> GetList()
        {
            return GetLoginUserActive();
        }

        /// <summary>
        /// Get an LoginUser by UserId, Password.
        /// </summary>
        /// <param name="userId">The condition get LoginUser.</param>
        /// <param name="password">The condition get LoginUser.</param>
        /// <returns><c>LoginUser</c> if id userId,password exist on database, <c>null</c> otherwise</returns>
        public LOGINUSER LoginLDAP(string userId)
        {
            var loginUser = dbSet.Where(p => !(p.DELETED ?? false))
                .FirstOrDefault(p => p.USERID == userId
                    && (p.ISACTIVE ?? false));
            return loginUser;
        }

        public LOGINUSER Login(string userId, string password)
        {
            var loginUser = dbSet.AsNoTracking().Where(p => !(p.DELETED ?? false))
                .FirstOrDefault(p => p.USERID == userId
                    //&& p.PASSWORD == password
                    && (p.ISACTIVE ?? false));
            return loginUser;
        }
        public LOGINUSER LoginAdmin(string userId, string password)
        {
            var loginUser = dbSet.AsNoTracking().Where(p => !(p.DELETED ?? false))
                .FirstOrDefault(p => p.USERID == userId && p.PASSWORD == password
                && (p.ISACTIVE ?? false));
            return loginUser;
        }
        public LOGINUSER JobLogin()
        {
            var loginUser = dbSet.Where(p => !(p.DELETED ?? false))
                .FirstOrDefault(p => p.USERID.ToLower() == "quarzt");
            return loginUser;
        }


        public LOGINUSER ClientLogin(string userId, string password)
        {
            return this.Login(userId, password);
            //var loginUser = dbSet.Where(p => !(p.DELETED ?? false)).FirstOrDefault(p => p.USERID == userId && p.PASSWORD == password && (p.ISACTIVE ?? false))
            //return loginUser
        }

        /// <summary>
        /// Get an LoginUser by UserSID.
        /// </summary>
        /// <param name="id">The condition get LoginUser.</param>
        /// <returns><c>Operator</c> if UserSID on database, <c>null</c> otherwise</returns>
        public LOGINUSER GetById(long id)
        {
            return GetLoginUserActive().FirstOrDefault(p => p.USERSID == id);
        }

        public LOGINUSER GetByUserId(long id)
        {
            return GetLoginUserActive().FirstOrDefault(p => p.USERID == id.ToString());
        }

        /// <summary>
        /// Get an LoginUser by Email.
        /// </summary>
        /// <param name="email">The condition get LoginUser.</param>
        /// <returns><c>LoginUser</c> if email on database, <c>null</c> otherwise</returns>
        public LOGINUSER GetByEmail(string email)
        {
            return GetLoginUserActive().FirstOrDefault(p => p.EMAIL.Equals(email));
        }

        public LOGINUSER GetUserByEmail(string email)
        {
            return GetLoginUserActive().FirstOrDefault(p => p.EMAIL.Equals(email) && p.CLIENTID == null);
        }

        public IEnumerable<AccountDetail> FillterUser(ConditionSearchUser condition)
        {
            var loginUsers = this.dbSet.Where(p => !(p.DELETED ?? false) && p.CLIENTID == null);

            if (condition.Branch.HasValue && condition.Branch > 0)
            {
                loginUsers = loginUsers.Where(p => p.COMPANYID == condition.Branch);
            }

            if (!string.IsNullOrWhiteSpace(condition.Keyword))
            {
                loginUsers = loginUsers.Where(p => (p.USERID.ToUpper().Contains(condition.Keyword.ToUpper())
                    || p.EMAIL.ToUpper().Contains(condition.Keyword.ToUpper())
                    || p.USERNAME.ToUpper().Contains(condition.Keyword.ToUpper())));
            }

            if (condition.RoleId.HasValue && condition.RoleId != 0)
            {
                loginUsers = loginUsers.Where(u => u.ROLEs.Select(r => r.ID).Contains((long)condition.RoleId));
            }

            var loginUserInfo = from loginUser in loginUsers
                                join myCompany in this.context.Set<MYCOMPANY>()
                                on loginUser.COMPANYID equals myCompany.COMPANYSID
                                select new AccountDetail
                                {
                                    UserSID = loginUser.USERSID,
                                    UserID = loginUser.USERID,
                                    UserName = loginUser.USERNAME,
                                    CompanyId = loginUser.COMPANYID,
                                    Email = loginUser.EMAIL,
                                    Mobile = loginUser.MOBILE,
                                    IsActive = (loginUser.ISACTIVE ?? false),
                                    CreatedDate = loginUser.CREATEDDATE,
                                    UserLevelSID = (loginUser.USERLEVELSID ?? 0),
                                    LevelCustomer = myCompany.LEVELCUSTOMER,
                                    CompanyName = myCompany.COMPANYNAME,
                                    BranchId = myCompany.BRANCHID,
                                    //TenQuyen = loginUser.ROLEs != null ? (loginUser.ROLEs.Select(p => p.ID).FirstOrDefault() + " - " + loginUser.ROLEs.Select(p => p.NAME).FirstOrDefault()) : ""
                                    TenQuyen = loginUser.ROLEs.Select(p => p.NAME).FirstOrDefault()
                                };

            if (!string.IsNullOrWhiteSpace(condition.BranchId))
            {
                loginUserInfo = loginUserInfo.Where(p => p.BranchId == condition.BranchId);
            }
            return loginUserInfo;
        }


        public IEnumerable<LOGINUSER> FillterUserOutRole(ConditionSearchUser condition)
        {
            var loginUsers = this.dbSet.Where(p => condition.ChildLevels.Contains(p.USERLEVEL.LEVELS) && !(p.DELETED ?? false));
            loginUsers = loginUsers.Where(p => p.ISACTIVE == true);
            if (!string.IsNullOrWhiteSpace(condition.Keyword))
            {
                loginUsers = loginUsers.Where(p => (p.USERID.ToUpper().Contains(condition.Keyword.ToUpper())
                    || p.EMAIL.ToUpper().Contains(condition.Keyword.ToUpper())
                    || p.USERNAME.ToUpper().Contains(condition.Keyword.ToUpper())));
            }

            if (condition.RoleId.HasValue)
            {
                loginUsers = loginUsers.Where(u => !u.ROLEs.Select(r => r.ID).Contains((long)condition.RoleId));
            }

            return loginUsers;
        }

        public IEnumerable<LOGINUSER> GetByIdCompany(long idCompany)
        {
            return GetLoginUserActive().Where(p => p.COMPANYID == idCompany);
        }

        /// <summary>
        /// Check an userId already exists in database
        /// </summary>
        /// <param name="userId">The condition check.</param>
        /// <returns><c>True</c> if userId exist on database, <c>false</c> otherwise</returns>
        public bool ContainUserId(string userId)
        {
            return Contains(p => !(p.DELETED ?? false) && p.USERID == userId);
        }

        public bool ContainUserIdSameCompany(string userId, long companyId)
        {
            return Contains(x => !(x.DELETED ?? false) && x.USERID == userId && x.COMPANYID == companyId);
        }

        /// <summary>
        /// Check an Email already exists in database
        /// </summary>
        /// <param name="email">The condition check.</param>
        /// <returns><c>True</c> if email exist on database, <c>false</c> otherwise</returns>
        public bool ContainEmail(string email)
        {
            bool result = true;
            if (!email.IsNullOrEmpty())
            {
                result = Contains(p => ((p.EMAIL ?? string.Empty).ToUpper()).Equals(email.ToUpper()) && !(p.DELETED ?? false));
            }
            return result;
        }

        public bool ContainEmailSameCompany(string email, long companyId)
        {
            bool result = true;
            if (!email.IsNullOrEmpty())
            {
                result = Contains(p => p.COMPANYID == companyId && ((p.EMAIL ?? string.Empty).ToUpper()).Equals(email.ToUpper()) && !(p.DELETED ?? false));
            }
            return result;
        }

        /// <summary>
        /// Check mail is exists in another user
        /// </summary>
        /// <param name="userSID"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool CheckMailEditUser(long userSID, string email)
        {
            var checkMail = this.dbSet.Where(lu => lu.USERSID != userSID
                        && (lu.EMAIL ?? string.Empty).ToUpper().Equals(email.ToUpper())
                        && !(lu.DELETED ?? false));
            return checkMail.Count() > 0;
        }

        /// <summary>
        /// Get an IEnumerable LoginUser by Role.
        /// </summary>
        /// <param name="roleName">The condition get LoginUser.</param>
        /// <returns><c>LoginUser</c> if roleName on database, <c>null</c> otherwise</returns>
        public IEnumerable<LOGINUSER> GetUsersByRoleName(string roleName)
        {
            return this.dbSet.Where(p => p.USERLEVEL.LEVELS.Equals(roleName));
        }

        private IQueryable<LOGINUSER> GetLoginUserActive()
        {
            return dbSet.Where(p => !(p.DELETED ?? false));
        }

        public LOGINUSER GetByUserId(string userId, long companyId)
        {
            return GetLoginUserActive().FirstOrDefault(p => p.USERID.Equals(userId) && p.COMPANYID == companyId);
        }
        public LOGINUSER getByUserID(string id)
        {
            return GetLoginUserActive().FirstOrDefault(p => p.USERID.Equals(id));
        }

        public LOGINUSER GetByUserIdAndEmail(string userId, string emails)
        {
            var loginUser = GetLoginUserActive().FirstOrDefault(p => p.USERID.Equals(userId)
                        && p.CLIENTID != null);
            if (loginUser == null)
                return null;
            var loginUserEmails = (loginUser.EMAIL ?? string.Empty).Split(new char[] { ',' }).AsEnumerable().Select(x => x.ToUpper()).ToList();
            var inputEmails = (emails ?? string.Empty).Split(new char[] { ',' }).AsEnumerable().Select(x => x.ToUpper()).ToList();

            foreach (var inputEmail in inputEmails)
            {
                if (!loginUserEmails.Contains(inputEmail))
                    return null;
            }
            return loginUser;
        }

        public LOGINUSER GetByUserId(string userId)
        {
            var loginUser = this.dbSet.Where(p => !(p.DELETED ?? false) && p.CLIENTID != null
                               && p.USERID.Equals(userId));
            return loginUser.FirstOrDefault();
        }

        public LOGINUSER GetByUserId(long id, long companyId)
        {
            return GetLoginUserActive().FirstOrDefault(p => p.COMPANYID == id && p.COMPANYID == companyId);
        }

        public bool ContainAccount(long companyId)
        {
            return Contains(p => p.COMPANYID == companyId && !(p.DELETED ?? false));
        }

        public LOGINUSER GetByClientId(long clientId)
        {
            return this.GetLoginUserActive().Where(l => l.CLIENTID == clientId).FirstOrDefault();
        }

        public decimal LoginUserUsing(long id)
        {
            var db = new DataClassesDataContext();
            var outputparam = new ObjectParameter("SP_RESULTOUTPUT", typeof(decimal));
            db.SP_LOGINUSER_USING(id, outputparam);
            var result = outputparam.Value.ToDecimal();
            return (decimal)result;
        }

        public IEnumerable<AccountViewModel> GetListAccount(ConditionSearchUser condition)
        {

            var loginUsers = this.dbSet.Where(p => !(p.DELETED ?? false) && p.CLIENTID != null);

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(x => x.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }
            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(x => x.TAXCODE.ToUpper().Contains(condition.TaxCode.ToUpper()));
            }
            //
            if (condition.UserName.IsNotNullOrEmpty())
            {
                loginUsers = loginUsers.Where(x => x.USERNAME.ToUpper().Contains(condition.UserName.ToUpper()));
            }
            if (condition.UserId.IsNotNullOrEmpty())
            {
                loginUsers = loginUsers.Where(x => x.USERID.ToUpper().Contains(condition.UserId.ToUpper()));
            }
            if (condition.Email.IsNotNullOrEmpty())
            {
                loginUsers = loginUsers.Where(x => x.EMAIL.ToUpper().Contains(condition.Email.ToUpper()));
            }
            bool isCurrentClient = OtherExtensions.IsCurrentClient();   // Các ngân hàng cần xử lý khách hàng vãng lai
            var rs = (from loginUser in loginUsers
                      join userlevel in this.context.Set<USERLEVEL>() on loginUser.USERLEVELSID equals userlevel.USERLEVELSID
                      join client in clients on loginUser.CLIENTID equals client.ID
                      let notCurrentClient = !isCurrentClient || client.CUSTOMERCODE != BIDCDefaultFields.CURRENT_CLIENT // không phải là khách hàng vãng lai
                      //from client in clientTemp.DefaultIfEmpty()
                      select new AccountViewModel
                      {
                          UserSID = loginUser.USERSID,
                          UserID = loginUser.USERID,
                          UserName = loginUser.USERNAME,
                          CompanyId = loginUser.COMPANYID,
                          Email = loginUser.EMAIL,
                          Password = loginUser.PASSWORD,
                          IsActive = loginUser.ISACTIVE ?? false,
                          CreatedDate = loginUser.CREATEDDATE,
                          UpdatedDate = loginUser.UPDATEDDATE,
                          LastAccessedTime = loginUser.LASTACCESSEDTIME,
                          LastChangedpasswordTime = loginUser.LASTCHANGEDPASSWORDTIME,
                          Mobile = loginUser.MOBILE,
                          //CompanyName = b.COMPANYNAME,
                          RoleName = userlevel.ROLENAME,
                          CustomerCode = client.CUSTOMERCODE,
                          TaxCode = notCurrentClient ? client.TAXCODE : DbFunctions.AsUnicode(""),
                      }).AsEnumerable();
            rs = rs.AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(condition.Skip).Take(condition.Take).ToList();
            return rs;
        }
        public long CountListAccount(ConditionSearchUser condition)
        {

            var loginUsers = this.dbSet.Where(p => !(p.DELETED ?? false) && p.CLIENTID != null);

            var clients = this.context.Set<CLIENT>().Where(p => !(p.DELETED ?? false));

            if (condition.CustomerCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(x => x.CUSTOMERCODE.ToUpper().Contains(condition.CustomerCode.ToUpper()));
            }
            if (condition.TaxCode.IsNotNullOrEmpty())
            {
                clients = clients.Where(x => x.TAXCODE.Contains(condition.TaxCode));
            }
            //
            if (condition.UserName.IsNotNullOrEmpty())
            {
                loginUsers = loginUsers.Where(x => x.USERNAME.ToUpper().Contains(condition.UserName.ToUpper()));
            }
            if (condition.UserId.IsNotNullOrEmpty())
            {
                loginUsers = loginUsers.Where(x => x.USERID.ToUpper().Contains(condition.UserId.ToUpper()));
            }
            if (condition.Email.IsNotNullOrEmpty())
            {
                loginUsers = loginUsers.Where(x => x.EMAIL.ToUpper().Contains(condition.Email.ToUpper()));
            }
            var rs = (from loginUser in loginUsers
                      join userlevel in this.context.Set<USERLEVEL>() on loginUser.USERLEVELSID equals userlevel.USERLEVELSID
                      join client in clients on loginUser.CLIENTID equals client.ID
                      //from client in clientTemp.DefaultIfEmpty()
                      select new AccountViewModel
                      {
                          UserSID = loginUser.USERSID,
                      }).AsEnumerable();
            return rs.Count();
        }

        public LOGINUSER GetAccountById(long id)
        {
            var rs = GetLoginUserActive().FirstOrDefault(x => x.USERSID == id);
            return rs;
        }

        public bool UpdatePasswordStatus(long id, LOGINUSER loginUser)
        {
            try
            {
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool GetAccountByName(string username)
        {
            var result = this.dbSet.FirstOrDefault(p => p.USERNAME.ToUpper().Contains(username.Trim().ToUpper()));
            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool getAccountByUserID(string id)
        {
            var result = this.dbSet.FirstOrDefault(p => p.USERID.ToUpper().Contains(id.Trim().ToUpper()));
            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<LOGINUSER> AddRange(List<LOGINUSER> loginUsers)
        {
            var dbContext = this.context as DataClassesDataContext;
            dbContext.BulkInsert(loginUsers);
            return loginUsers;
        }
    }
}
