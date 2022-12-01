using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceTemplateSampleRepository : IRepository<INVOICETEMPLATESAMPLE>
    {
        /// <summary>
        /// Get an IEnumerable InvoiceTemplateSample 
        /// </summary>
        /// <returns><c>IEnumerable InvoiceTemplateSample</c> if InvoiceTemplateSample not Empty, <c>null</c> otherwise</returns>
        IEnumerable<INVOICETEMPLATESAMPLE> GetList();
        INVOICETEMPLATESAMPLE GetById(long id);

        IEnumerable<INVOICETEMPLATESAMPLE> GetListByInvoiceTypeId(long invoiceTypeId);
    }
}
