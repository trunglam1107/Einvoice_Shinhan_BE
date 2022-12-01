using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxDepartmentBusiness'
    public class TaxDepartmentBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxDepartmentBusiness'
    {
        #region Fields, Properties

        private readonly ITaxDepartmenBO taxDepartmentBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxDepartmentBusiness.TaxDepartmentBusiness(IBOFactory)'
        public TaxDepartmentBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxDepartmentBusiness.TaxDepartmentBusiness(IBOFactory)'
        {
            this.taxDepartmentBO = boFactory.GetBO<ITaxDepartmenBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxDepartmentBusiness.GetList()'
        public IEnumerable<TaxDepartmentInfo> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxDepartmentBusiness.GetList()'
        {
            var roles = this.taxDepartmentBO.GetList();
            return roles.Select(p => new TaxDepartmentInfo(p));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxDepartmentBusiness.FilterTaxDepartment(long)'
        public IEnumerable<TaxDepartmentInfo> FilterTaxDepartment(long cityId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxDepartmentBusiness.FilterTaxDepartment(long)'
        {
            var taxDepartments = this.taxDepartmentBO.FilterTaxDepartment(cityId);
            return taxDepartments.Select(p => new TaxDepartmentInfo(p));
        }

        #endregion
    }
}