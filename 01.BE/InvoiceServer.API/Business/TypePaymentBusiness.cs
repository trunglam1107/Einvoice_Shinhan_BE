using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentBusiness'
    public class TypePaymentBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentBusiness'
    {
        #region Fields, Properties

        private readonly ITypePaymentBO typePaymentBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentBusiness.TypePaymentBusiness(IBOFactory)'
        public TypePaymentBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentBusiness.TypePaymentBusiness(IBOFactory)'
        {
            this.typePaymentBO = boFactory.GetBO<ITypePaymentBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentBusiness.GetList()'
        public IEnumerable<TypePaymentInfo> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentBusiness.GetList()'
        {
            var roles = this.typePaymentBO.GetList();
            return roles.Select(p => new TypePaymentInfo(p));
        }
        #endregion
    }
}