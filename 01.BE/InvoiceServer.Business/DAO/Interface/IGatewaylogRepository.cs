using InvoiceServer.Business.Models.GatewayLog;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.DAO.Interface
{
    public interface IGatewaylogRepository
    { 
        /// <summary>
        /// Get an IEnumerable Client 
        /// </summary>
        /// <returns><c>IEnumerable Client</c> if Client not Empty, <c>null</c> otherwise</returns>
        IEnumerable<GATEWAY_LOG> GetList();

        /// <summary>
        /// Get list client by filter
        /// </summary>
        /// <param name="filterCondition"></param>
        /// <returns></returns>
        IEnumerable<GatewaylogDetail> Filter(ConditionSearchGatewaylog condition);
        long Count(ConditionSearchGatewaylog condition);
    }
}
