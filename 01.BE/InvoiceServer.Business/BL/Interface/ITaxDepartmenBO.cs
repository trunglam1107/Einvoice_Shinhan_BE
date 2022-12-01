using InvoiceServer.Business.Models;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface ITaxDepartmenBO
    {

        /// <summary>
        /// Search DataPermission by condition
        /// </summary>
        /// <param name="condition">condition</param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<TaxDepartmentInfo> GetList();

        IEnumerable<TaxDepartmentInfo> FilterTaxDepartment(long cityId);
    }
}
