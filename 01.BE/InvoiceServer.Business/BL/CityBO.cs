using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class CityBO : ICityBO
    {
        #region Fields, Properties

        private readonly ICityRepository cityRepository;
        #endregion

        #region Contructor

        public CityBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.cityRepository = repoFactory.GetRepository<ICityRepository>();
        }

        #endregion

        #region Methods
        public IEnumerable<CityInfo> GetList()
        {
            var cities = this.cityRepository.GetAll();
            return cities.Select(p => new CityInfo(p));
        }


        public CityInfo GetById(long id)
        {
            var city = this.cityRepository.GetById(id);
            return new CityInfo(city);
        }

        #endregion

    }
}