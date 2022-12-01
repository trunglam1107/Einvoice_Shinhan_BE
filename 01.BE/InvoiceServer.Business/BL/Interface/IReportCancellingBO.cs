using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.ReportCancelling;
using InvoiceServer.Common;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IReportCancellingBO
    {
        IEnumerable<ReportCancellingMaster> FilterReportCancelling(long companyId);
        /// <summary>
        /// Add new client
        /// </summary>
        /// <param name="client">client info</param>
        /// <returns></returns>
        ResultCode Create(ReportCancellingInfo cancellingInfo);
        ResultCode ApproveCancelling(ApprovedReportCancelling cancellingInfo, UserSessionInfo currentUser);
        ResultCode revertApproveCancelling(ApprovedReportCancelling cancellingInfo, UserSessionInfo currentUser);
        ReportCancellingInfo GetById(long id, long companyId);

        long GetNumberInvoiceCancelling(long companyId);

        long GetNumberInvoiceCancelling(ConditionInvoiceUse condition);

        long GetAllNumberInvoiceCancelling();
        ResultCode Update(long id, long? companyId, ReportCancellingInfo cancellingInfo);
        ResultCode Delete(long id, long? companyId);

    }
}
