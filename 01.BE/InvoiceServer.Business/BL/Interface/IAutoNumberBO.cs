using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IAutoNumberBO
    {
        IEnumerable<AutoNumberViewModel> GetAll(ConditionSearchAutoNumber condition);
        AutoNumberViewModel GetByID(string id);
        ResultCode Update(string code, AutoNumberViewModel model);
        ResultCode Create(AutoNumberViewModel model);
        ResultCode Delete(string code);
        String CreateAutoNumber(long companyId, string typeID, string branchId, bool saved = false);
        bool checkEnable(string id);
        int Length(string id);
    }
}
