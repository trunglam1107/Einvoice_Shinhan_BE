using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Account;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ILoginUserRepository : IRepository<LOGINUSER>
    {
        /// <summary>
        /// Get an IEnumerable LoginUser 
        /// </summary>
        /// <returns><c>IEnumerable LoginUser</c> if LoginUser not Empty, <c>null</c> otherwise</returns>
        IEnumerable<LOGINUSER> GetList();

        IEnumerable<AccountDetail> FillterUser(ConditionSearchUser condition);

        IEnumerable<LOGINUSER> FillterUserOutRole(ConditionSearchUser condition);
        LOGINUSER getByUserID(string id);
        /// <summary>
        /// Get an LoginUser by UserId, Password.
        /// </summary>
        /// <param name="userId">The condition get LoginUser.</param>
        /// <param name="password">The condition get LoginUser.</param>
        /// <returns><c>LoginUser</c> if id userId,password exist on database, <c>null</c> otherwise</returns>
        LOGINUSER LoginLDAP(string userId);

        LOGINUSER ClientLogin(string userId, string password);

        /// <summary>
        /// Get an LoginUser by UserSID.
        /// </summary>
        /// <param name="id">The condition get LoginUser.</param>
        /// <returns><c>LoginUser</c> if UserSID on database, <c>null</c> otherwise</returns>
        LOGINUSER GetById(long id);

        LOGINUSER GetByUserId(long id);
        bool GetAccountByName(string username);
        LOGINUSER Login(string userId, string password);
        LOGINUSER LoginAdmin(string userId, string password);
        LOGINUSER JobLogin();
        /// <summary>
        /// Get an LoginUser by Email.
        /// </summary>
        /// <param name="email">The condition get LoginUser.</param>
        /// <returns><c>LoginUser</c> if email on database, <c>null</c> otherwise</returns>
        LOGINUSER GetByEmail(string email);

        /// <summary>
        /// Get an LoginUser by Email(not contain client).
        /// </summary>
        /// <param name="email">The condition get LoginUser.</param>
        /// <returns><c>LoginUser</c> if email on database, <c>null</c> otherwise</returns>
        LOGINUSER GetUserByEmail(string email);

        /// <summary>
        /// Get an IEnumerable LoginUser 
        /// </summary>
        /// <param name="idCompany">The condition get list LoginUser.</param>
        /// <returns><c>IEnumerable LoginUser</c> if idCompany exist on database, <c>null</c> otherwise</returns>
        IEnumerable<LOGINUSER> GetByIdCompany(long idCompany);

        /// <summary>
        /// Check an userId already exists in database
        /// </summary>
        /// <param name="userId">The condition check.</param>
        /// <returns><c>True</c> if userId exist on database, <c>false</c> otherwise</returns>
        bool ContainUserId(string userId);

        bool ContainUserIdSameCompany(string userId, long companyId);

        /// <summary>
        /// Check an Email already exists in database
        /// </summary>
        /// <param name="email">The condition check.</param>
        /// <returns><c>True</c> if email exist on database, <c>false</c> otherwise</returns>
        bool ContainEmail(string email);

        bool ContainEmailSameCompany(string email, long companyId);

        bool CheckMailEditUser(long userSID, string email);

        /// <summary>
        /// Get an IEnumerable LoginUser by Role.
        /// </summary>
        /// <param name="roleName">The condition get LoginUser.</param>
        /// <returns><c>LoginUser</c> if UserSID on database, <c>null</c> otherwise</returns>
        IEnumerable<LOGINUSER> GetUsersByRoleName(string roleName);

        IEnumerable<AccountViewModel> GetListAccount(ConditionSearchUser condition);
        long CountListAccount(ConditionSearchUser condition);
        LOGINUSER GetByUserId(string userId, long companyId);

        LOGINUSER GetByUserIdAndEmail(string userId, string emails);

        bool ContainAccount(long companyId);

        LOGINUSER GetByClientId(long clientId);

        decimal LoginUserUsing(long id);

        LOGINUSER GetAccountById(long id);

        bool UpdatePasswordStatus(long id, LOGINUSER loginUser);

        bool getAccountByUserID(string id);
        LOGINUSER GetByUserId(string userId);
        List<LOGINUSER> AddRange(List<LOGINUSER> loginUsers);
    }
}
