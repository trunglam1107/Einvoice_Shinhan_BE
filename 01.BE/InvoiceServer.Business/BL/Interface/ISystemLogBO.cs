using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface ISystemLogBO
    {
        /// <summary>
        /// Search systemlog with condition
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<SystemLogInfo> Filter(ConditionSearchSystemLog condition);
        int Count(ConditionSearchSystemLog condition);
        ExportFileInfo DownloadDataSystem(ConditionSearchSystemLog condition);
        ResultCode Create(SystemLogInfo model);

    }
}
