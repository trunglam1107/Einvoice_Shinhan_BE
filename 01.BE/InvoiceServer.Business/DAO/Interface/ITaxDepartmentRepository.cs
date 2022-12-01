using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface ITaxDepartmentRepository : IRepository<TAXDEPARTMENT>
    {
        /// <summary>
        /// Get an IEnumerable TaxDepartment 
        /// </summary>
        /// <returns><c>IEnumerable TaxDepartment</c> if TaxDepartment not Empty, <c>null</c> otherwise</returns>
        IEnumerable<TAXDEPARTMENT> GetList();
        IEnumerable<TAXDEPARTMENT> FilterTaxDeparment(long cityId);
        IEnumerable<TAXDEPARTMENT> FilterTaxDeparment(long cityId, string taxDepartmentName);
        TAXDEPARTMENT GetById(long id);

    }
}
