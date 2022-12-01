using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IRoleFunctionBO
    {
        void InitRoleFunction(long roleId);

        List<ROLEFUNCTION> InitRoleFunctionNew(long roleId);
        ResultCode UpdateRoleFunction(UpdateRoleFunctionInfo updateRoleFunctionInfo);

    }
}
