using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IInvoiceReleasesBO
    {


        /// <summary>
        /// Search Clients by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<InvoiceReleasesMaster> Filter(ConditionSearchInvoiceReleases condition);

        /// <summary>
        /// Count number clients by condition
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        long Count(ConditionSearchInvoiceReleases condition);

        /// <summary>
        /// Add new client
        /// </summary>
        /// <param name="client">client info</param>
        /// <returns></returns>
        ResultCode Create(InvoiceReleasesInfo info);

        /// <summary>
        /// Update client info
        /// </summary>
        /// <param name="client">client info to update</param>
        /// <returns></returns>
        ResultCode Update(long id, long? companyId, InvoiceReleasesInfo info);

        ResultCode UpdateStatus(long id, long? companyId, InvoiceReleasesStatus status, bool isUnApproved = false);

        /// <summary>
        /// Delete Client By Update Deleted Flag = true
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        ResultCode Delete(long id, long? companyId);

        /// <summary>
        /// Get the client info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        InvoiceReleasesInfo GetInvoiceReleasesInfo(long id, long? companyId, bool isView = false);

        InvoiceReleasesTemplateInfo GetTemplateReleaseTemplate();


    }
}
