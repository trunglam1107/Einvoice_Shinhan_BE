using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class UserLevelBO : IUserLevelBO
    {
        #region Fields, Properties

        private readonly IUserLevelRepository levelRepository;
        #endregion

        #region Contructor

        public UserLevelBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.levelRepository = repoFactory.GetRepository<IUserLevelRepository>();
        }

        #endregion

        #region Methods
        public IEnumerable<USERLEVEL> GetList()
        {
            return this.levelRepository.GetAll();
        }

        public USERLEVEL GetRoleById(long id)
        {
            return this.levelRepository.GetById(id);
        }

        public USERLEVEL GetRole(long id)
        {
            return this.levelRepository.GetAll().FirstOrDefault(p => p.USERLEVELSID == id);
        }

        public void CreateOrEdit(RoleDetail roleDetail)
        {
            if (roleDetail.Id == 0)
            {
                var userLevel = new USERLEVEL()
                {
                    ROLENAME = roleDetail.Name,
                    DEFAULTPAGE = "/",
                    CREATEDDATE = DateTime.Now,
                };
                this.levelRepository.Insert(userLevel);
            }
            else
            {
                var existedUserLevel = levelRepository.GetById(roleDetail.Id);
                existedUserLevel.ROLENAME = roleDetail.Name;
                existedUserLevel.UPDATEDDATE = DateTime.Now;
                this.levelRepository.Update(existedUserLevel);
            }
        }

        public USERLEVEL getByRoleSessionInfo(RoleLevel role)
        {
            return this.levelRepository.GetAll()
                    .FirstOrDefault(r => role.DefaultPage == r.DEFAULTPAGE
                    && role.Level == r.LEVELS
                    && role.RoleName == r.ROLENAME);
        }

        #endregion
    }
}