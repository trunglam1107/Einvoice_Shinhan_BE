using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Enums;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IQueueCreateFileInvoiceBO
    {
        /// <summary>
        /// Update client info
        /// </summary>
        /// <param name="client">client info to update</param>
        /// <returns></returns>
        ResultCode Update(List<long> invoiceIds, QueueCreateFileStatus status);

        IEnumerable<QueueCreateFile> FilterByStatus(List<long> status);

        ResultCode Create(List<long> invoiceIds);

    }
}
