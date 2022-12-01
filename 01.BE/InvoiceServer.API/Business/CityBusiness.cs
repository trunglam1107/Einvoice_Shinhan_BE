using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CityBusiness'
    public class CityBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CityBusiness'
    {
        #region Fields, Properties

        private readonly ICityBO cityBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CityBusiness.CityBusiness(IBOFactory)'
        public CityBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CityBusiness.CityBusiness(IBOFactory)'
        {
            this.cityBO = boFactory.GetBO<ICityBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CityBusiness.GetList()'
        public IEnumerable<CityInfo> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CityBusiness.GetList()'
        {
            var roles = this.cityBO.GetList();
            return roles.Select(p => new CityInfo(p));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CityBusiness.GetById(long)'
        public CityInfo GetById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CityBusiness.GetById(long)'
        {
            return this.cityBO.GetById(id);
        }
        #endregion
    }
}