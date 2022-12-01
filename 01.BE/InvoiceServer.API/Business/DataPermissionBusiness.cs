using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
    public class DataPermissionBusiness : BaseBusiness
    {
        #region Fields, Properties

        private IDataPermissionBO permissionBO;

        #endregion Fields, Properties

        #region Contructor

        public DataPermissionBusiness(IBOFactory boFactory)
        {
            this.permissionBO = boFactory.GetBO<IDataPermissionBO>();
        }

        #endregion Contructor

        #region Methods

        public IEnumerable<DataPermissionInfo> GetList()
        {
            var roles =  this.permissionBO.GetList();
            return roles.Select(p => new DataPermissionInfo(p));
        }
        #endregion
    }
}