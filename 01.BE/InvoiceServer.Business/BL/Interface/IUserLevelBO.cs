using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IUserLevelBO
    {

        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<USERLEVEL> GetList();

        USERLEVEL GetRoleById(long id);

        USERLEVEL GetRole(long id);

        void CreateOrEdit(RoleDetail roleDetail);

        USERLEVEL getByRoleSessionInfo(RoleLevel role);

    }
}
