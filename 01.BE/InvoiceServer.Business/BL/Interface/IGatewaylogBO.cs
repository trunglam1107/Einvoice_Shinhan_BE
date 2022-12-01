using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.GatewayLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.BL.Interface
{
    public interface IGatewaylogBO
    {
        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<GatewaylogDetail> Filter(ConditionSearchGatewaylog condition);

        long Count(ConditionSearchGatewaylog condition);

        ExportFileInfo ReportHistoryTVan(ConditionSearchGatewaylog condition);
    }
}
