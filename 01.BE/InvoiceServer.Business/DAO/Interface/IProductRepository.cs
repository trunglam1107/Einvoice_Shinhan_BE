using InvoiceServer.Business.Models;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;

namespace InvoiceServer.Business.DAO
{
    public interface IProductRepository : IRepository<PRODUCT>
    {
        /// <summary>
        /// Get an IEnumerable MyCompany 
        /// </summary>
        /// <returns><c>IEnumerable MyCompany</c> if MyCompany not Empty, <c>null</c> otherwise</returns>
        IEnumerable<PRODUCT> GetList();

        /// <summary>
        /// Get an MyCompany by CompanySID.
        /// </summary>
        /// <param name="id">The condition get MyCompany.</param>
        /// <returns><c>Operator</c> if CompanySID on database, <c>null</c> otherwise</returns>
        PRODUCT GetById(long id);

        PRODUCT GetById(long id, long companyId);

        /// <summary>
        /// Search Company by the filter conditions
        /// </summary>
        /// <param name="filterCondition"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<PRODUCT> FilterProducts(ConditionSearchProduct condition);

        PRODUCT FilterProduct(long companyId, string name, string unitName);

        PRODUCT FilterProductNameUnitCode(string name, string unitCode);

        PRODUCT FilterProductCode(string code);
        PRODUCT FilterProductNameUnitId(long companyId, string name, long unitId);
        bool ContainCode(long companyId, string code, bool isCreate, long id);
        bool ContainNameUnitId(string name, long unitId, bool isCreate, long id);
        bool ExistsDifferTax(long companyId, string name, long taxId, bool isCreate, long id);
        bool MyProductUsing(long id);

    }
}
