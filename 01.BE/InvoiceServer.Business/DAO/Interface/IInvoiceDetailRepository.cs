using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.DAO
{
    public interface IInvoiceDetailRepository : IRepository<INVOICEDETAIL>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<INVOICEDETAIL> FilterInvoiceDetail(long invoiceId);

        IEnumerable<InvoiceItem> GetInvoiceItems(long invoiceId);

        INVOICEDETAIL GetById(long id);

        bool ContainProduct(long productId);
        IQueryable<INVOICEDETAIL> FilterInvoiceDetailJob(long invoiceId);
        List<InvoiceDetail> GetDetail(List<InvoicePrintModel> invoicePrints);
    }
}