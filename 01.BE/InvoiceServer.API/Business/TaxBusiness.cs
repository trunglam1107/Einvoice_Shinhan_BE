using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxBusiness'
    public class TaxBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxBusiness'
    {
        #region Fields, Properties

        private readonly ITaxBO taxBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxBusiness.TaxBusiness(IBOFactory)'
        public TaxBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxBusiness.TaxBusiness(IBOFactory)'
        {
            this.taxBO = boFactory.GetBO<ITaxBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxBusiness.GetList()'
        public IEnumerable<TaxInfo> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxBusiness.GetList()'
        {
            var taxs = this.taxBO.GetList();
            return taxs;
        }
        #endregion
    }
}