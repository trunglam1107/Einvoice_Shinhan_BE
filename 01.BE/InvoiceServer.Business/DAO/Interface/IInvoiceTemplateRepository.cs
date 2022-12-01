using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceTemplateRepository : IRepository<INVOICETEMPLATE>
    {
        /// <summary>
        /// Get an IEnumerable InvoiceTemplate 
        /// </summary>
        /// <returns><c>IEnumerable InvoiceTemplate</c> if InvoiceTemplate not Empty, <c>null</c> otherwise</returns>
        IEnumerable<INVOICETEMPLATE> GetList();
        IEnumerable<INVOICETEMPLATE> GetAll(long companySID);
        IEnumerable<INVOICETEMPLATE> Filter(long invoiceSampleId, long companySID);
        INVOICETEMPLATE GetById(long id);
        INVOICETEMPLATE GetByIdNoTracking(long id);
        long CreateInvoiceTemplate(INVOICETEMPLATE invoiceTemplate);

        INVOICETEMPLATE GetByInvoiceSampleCode(string invoiceSampleCode, long companyId);
        INVOICETEMPLATE GetByInvoiceSampleId(long invoiceSampleId, long companyId);
    }
}
