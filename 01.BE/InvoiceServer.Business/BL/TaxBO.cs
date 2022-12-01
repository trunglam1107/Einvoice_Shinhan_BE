using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class TaxBO : ITaxBO
    {
        #region Fields, Properties

        private readonly ITaxRepository taxRepository;
        #endregion

        #region Contructor

        public TaxBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.taxRepository = repoFactory.GetRepository<ITaxRepository>();
        }

        #endregion
        public IEnumerable<TaxInfo> GetList()
        {
            var tax = this.taxRepository.GetAll().OrderBy(p => p.ORDERS);
            return tax.Select(p => new TaxInfo(p));
        }
        public TaxInfo GetById(long id)
        {
            var taxById = this.taxRepository.GetById(id);
            TaxInfo tax = new TaxInfo();
            tax.Code = taxById.TAX1;
            tax.Name = taxById.TAX1.ToString();
            return tax;
        }
        #region Methods

        #endregion
    }
}