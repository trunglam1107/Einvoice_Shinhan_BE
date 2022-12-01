using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class RoleFunctionBO : IRoleFunctionBO
    {
        #region Fields, Properties

        private readonly IRoleFunctionRepository roleFunctionRepository;
        private readonly IFunctionRepository functionRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IDbTransactionManager transaction;
        #endregion

        #region Contructor

        public RoleFunctionBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.roleFunctionRepository = repoFactory.GetRepository<IRoleFunctionRepository>();
            this.functionRepository = repoFactory.GetRepository<IFunctionRepository>();
            this.roleRepository = repoFactory.GetRepository<IRoleRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add RoleFunction with not any permission, if RoleFunction not exists
        /// </summary>
        /// <param name="roleId"></param>
        public void InitRoleFunction(long roleId)
        {
            var roleById = this.roleRepository.GetById(roleId);
            var functions = this.functionRepository.GetAll()
                        .Where(f => f.USERLEVELSID == roleById.USERLEVELSID)
                        .ToList();

            var roleFunctionIdsQuery = from roleFunction in this.roleFunctionRepository.GetAll()
                                       join role in this.roleRepository.GetAll() on roleFunction.ROLEID equals role.ID
                                       where role.USERLEVELSID == roleById.USERLEVELSID
                                       && roleFunction.ROLEID == roleId
                                       select roleFunction.FUNCTIONID;
            var roleFunctionIds = roleFunctionIdsQuery.ToList();

            //var roleFunctionIds = this.roleFunctionRepository.GetAll()
            //            .Where(rf => rf.ROLE.USERLEVELSID == roleById.USERLEVELSID)
            //            .Where(rf => rf.ROLEID == roleId)
            //            .Select(rf => rf.FUNCTIONID)
            //            .ToList();

            foreach (var function in functions)
            {
                if (!roleFunctionIds.Contains(function.ID))
                {
                    var roleFunction = new ROLEFUNCTION()
                    {
                        ROLEID = roleId,
                        FUNCTIONID = function.ID,
                        ACTION = "",
                    };
                    this.roleFunctionRepository.Insert(roleFunction);
                }
            }
        }

        public List<ROLEFUNCTION> InitRoleFunctionNew(long roleId)
        {
            var roleById = this.roleRepository.GetById(roleId);
            var functions = this.functionRepository.GetAll()
                        .Where(f => f.USERLEVELSID == roleById.USERLEVELSID && !(f.DELETED ?? false))
                        .ToList();

            var roleFunctionIdsQuery = from roleFunction in this.roleFunctionRepository.GetAll()
                                       join role in this.roleRepository.GetAll() on roleFunction.ROLEID equals role.ID
                                       where role.USERLEVELSID == roleById.USERLEVELSID
                                       && roleFunction.ROLEID == roleId && !(roleFunction.FUNCTION.DELETED ?? false)
                                       select roleFunction.FUNCTIONID;
            var roleFunctionIds = roleFunctionIdsQuery.ToList();

            //var roleFunctionIds = this.roleFunctionRepository.GetAll()
            //            .Where(rf => rf.ROLE.USERLEVELSID == roleById.USERLEVELSID)
            //            .Where(rf => rf.ROLEID == roleId)
            //            .Select(rf => rf.FUNCTIONID)
            //            .ToList();

            List<ROLEFUNCTION> newListRoleFunction = new List<ROLEFUNCTION>();
            var roleFunctionModel = new ROLEFUNCTION();
            foreach (var function in functions)
            {
                if (!roleFunctionIds.Contains(function.ID))
                {
                    roleFunctionModel = new ROLEFUNCTION()
                    {
                        ROLEID = roleId,
                        FUNCTIONID = function.ID,
                        ACTION = "",
                    };
                    this.roleFunctionRepository.Insert(roleFunctionModel);

                }
                else
                {
                    roleFunctionModel = new ROLEFUNCTION()
                    {
                        ROLEID = roleId,
                        FUNCTIONID = function.ID,
                        ACTION = "",
                    };
                }

                newListRoleFunction.Add(roleFunctionModel);
            }

            return newListRoleFunction;
        }

        public ResultCode UpdateRoleFunction(UpdateRoleFunctionInfo updateRoleFunctionInfo)
        {
            if (updateRoleFunctionInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            try
            {
                transaction.BeginTransaction();
                UpdatePermission(updateRoleFunctionInfo);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return ResultCode.NoError;
        }

        #endregion


        #region Private Methods
        private void UpdatePermission(UpdateRoleFunctionInfo updateRoleFunctionInfo)
        {
            try
            {
                updateRoleFunctionInfo.Roles.ForEach(p =>
                {
                    ROLEFUNCTION roleFunction = this.roleFunctionRepository.GetById(p.Id);
                    if (roleFunction != null)
                    {
                        roleFunction.ACTION = p.GetAction();
                        this.roleFunctionRepository.Update(roleFunction);
                    }
                });
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.UnknownError, ex.Message);
            }
        }
        #endregion
    }
}