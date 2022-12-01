using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness'
    public class LevelBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness'
    {
        #region Fields, Properties

        private readonly IUserLevelBO levelBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness.LevelBusiness(IBOFactory)'
        public LevelBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness.LevelBusiness(IBOFactory)'
        {
            this.levelBO = boFactory.GetBO<IUserLevelBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness.GetList()'
        public IEnumerable<RoleDetail> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness.GetList()'
        {
            var roles = this.levelBO.GetList();
            return roles.Select(p => new RoleDetail(p));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness.GetRole(long)'
        public RoleDetail GetRole(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness.GetRole(long)'
        {
            var role = this.levelBO.GetRole(id);
            return new RoleDetail(role);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness.CreateOrEdit(RoleDetail)'
        public void CreateOrEdit(RoleDetail roleDetail)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'LevelBusiness.CreateOrEdit(RoleDetail)'
        {
            this.levelBO.CreateOrEdit(roleDetail);
        }

        #endregion
    }
}