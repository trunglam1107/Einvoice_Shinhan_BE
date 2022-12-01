using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class TaxDepartmentBO : ITaxDepartmenBO
    {
        #region Fields, Properties

        private readonly ITaxDepartmentRepository taxDepartmentRepository;
        #endregion

        #region Contructor

        public TaxDepartmentBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.taxDepartmentRepository = repoFactory.GetRepository<ITaxDepartmentRepository>();
        }

        #endregion
        #region Methods
        public IEnumerable<TaxDepartmentInfo> GetList()
        {
            var taxDepartments = this.taxDepartmentRepository.GetList();
            return taxDepartments.Select(p => new TaxDepartmentInfo(p));
        }

        public IEnumerable<TaxDepartmentInfo> FilterTaxDepartment(long cityId)
        {
            var taxDepartments = this.taxDepartmentRepository.FilterTaxDeparment(cityId);
            return taxDepartments.Select(p => new TaxDepartmentInfo(p));
        }

        #endregion
    }
}