using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IReportCancellingDetailRepository : IRepository<REPORTCANCELLINGDETAIL>
    {
        /// <summary>
        /// Get an IEnumerable City 
        /// </summary>
        /// <returns><c>IEnumerable City</c> if City not Empty, <c>null</c> otherwise</returns>
        REPORTCANCELLINGDETAIL GetById(long id);

        bool ContainRegisterTemplate(long registerTemplateId, string symbol, long reportCancellingId);

        IEnumerable<REPORTCANCELLINGDETAIL> Filter(long companyId, long registerTemplateId);

        IEnumerable<REPORTCANCELLINGDETAIL> FilterBy(long companyId, long reportCancellingId);

        REPORTCANCELLINGDETAIL Filter(long companyId, long registerTemplateId, string symbol, DateTime? dateFrom, DateTime? dateTo);

        long GetNumberInvoiceCancelling(ConditionInvoiceUse condition);
    }
}
