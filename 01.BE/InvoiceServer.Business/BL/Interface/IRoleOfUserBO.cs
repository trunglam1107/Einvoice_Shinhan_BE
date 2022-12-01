using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IRoleOfUserBO
    {

        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        ResultCode CreatePermission(LOGINUSER loginUser, List<FunctionInfo> functions);

        ResultCode UpdatePermission(LOGINUSER loginUser, List<FunctionInfo> functions);

        List<FunctionInfo> GetPermissionOfUser(LOGINUSER loginUser);
    }
}
