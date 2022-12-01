using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IRoleBO
    {

        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<ROLE> GetList(ConditionSearchRole condition);

        long CountList(ConditionSearchRole condition);

        ROLE GetRoleByName(string name, long companySID);

        ROLE GetRoleByNameAndLevelName(string name, long companySID, string levelName);

        ROLE GetRoleById(long id);

        void CreateOrEdit(RoleGroupDetail roleGroupDetail, USERLEVEL userLevel, UserSessionInfo currentUser);

        void UpdateUsersInRole(UpdateUsersInRole usersInRole, UserSessionInfo currentUser);

        void CreateUsersInRole(UpdateUsersInRole usersInRole, USERLEVEL userLevel, UserSessionInfo currentUser);

        IEnumerable<ROLE> GetRolesByUserId(long userId);

        void CreateOrEditUserRoles(AccountRoleInfo accountRoleInfo, UserSessionInfo currentUser);

        ResultCode Delete(long id, UserSessionInfo currentUser);

    }
}
