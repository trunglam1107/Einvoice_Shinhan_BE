using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ITypePaymentRepository : IRepository<TYPEPAYMENT>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<TYPEPAYMENT> GetList();

        TYPEPAYMENT GetPaymentByCode(string code);

        TYPEPAYMENT GetById(long id);

    }
}
