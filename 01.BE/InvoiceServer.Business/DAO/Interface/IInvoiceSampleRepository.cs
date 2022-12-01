using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceSampleRepository : IRepository<INVOICESAMPLE>
    {
        INVOICESAMPLE GetById(long id);

        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<INVOICESAMPLE> GetList(UserSessionInfo currentUser);
        INVOICESAMPLE GetInvoiceSampleById(long invoiceSampleId);

        INVOICESAMPLE CreateInvoiceSample(UserSessionInfo currentUser, INVOICESAMPLE invoiceSample);

        INVOICESAMPLE Update(UserSessionInfo currentUser, INVOICESAMPLE info);

        INVOICESAMPLE GetByCode(string code, UserSessionInfo currentUser);

        bool Update(UserSessionInfo currentUser, long id, INVOICESAMPLE invoiceSample);

        bool DeleteInvoiceSample(string code, UserSessionInfo currentUser);

        bool DeleteInvoiceSampleById(long id);

        IEnumerable<INVOICESAMPLE> Filter(UserSessionInfo currentUser, ConditionSearchInvoiceSample condition, int skip = 0, int take = Int32.MaxValue);

        IEnumerable<INVOICESAMPLE> GetByInvoiceTypeId(UserSessionInfo currentUser, long invoiceTypeId);

        bool ContainCode(string code, string name, bool create, UserSessionInfo currentUser);
        bool MyInvoiceSampleUsing(long invoiceID);
    }
}
