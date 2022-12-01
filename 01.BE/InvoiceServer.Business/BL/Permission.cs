using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class Permission
    {
        private readonly IUserLevelRepository levelRepository;
        private readonly IRoleFunctionRepository roleFunctionRepository;
        private readonly IRoleFunctionRepository roleGroupFunctionRepository;
        private readonly IUVGetFunctionOfUserRepository uV_GetFunctionOfUserRepository;
        private readonly IRoleRepository roleRepository;

        public Permission(IRepositoryFactory repoFactory)
        {
            this.levelRepository = repoFactory.GetRepository<IUserLevelRepository>();
            this.roleFunctionRepository = repoFactory.GetRepository<IRoleFunctionRepository>();
            this.roleGroupFunctionRepository = repoFactory.GetRepository<IRoleFunctionRepository>();
            this.uV_GetFunctionOfUserRepository = repoFactory.GetRepository<IUVGetFunctionOfUserRepository>();
            this.roleRepository = repoFactory.GetRepository<IRoleRepository>();
        }

        public ResultCode CreatePermission(LOGINUSER loginUser, List<FunctionInfo> functions)
        {
            return ResultCode.NoError;
        }

        // Đổi business tính Role, dùng Method mới này
        // 1 User có nhiều Role, 1 Role có nhiều User
        public ResultCode CreatePermission(LOGINUSER loginUser, UserSessionInfo currentUser)
        {
            try
            {
                // Create Role
                var role = new ROLE()
                {
                    USERLEVELSID = loginUser.USERLEVELSID,
                    NAME = "Administrator",
                    CREATEDDATE = DateTime.Now,
                    CREATEDBY = currentUser.Id,
                };

                // Add User into Role
                role.LOGINUSERs.Add(loginUser);

                // Add functions into Role
                loginUser.USERLEVEL.FUNCTIONS.ForEach(function =>
                {
                    if (function != null)
                    {
                        var roleFunction = new ROLEFUNCTION();
                        roleFunction.FUNCTIONID = function.ID;
                        roleFunction.ACTION = function.ACTION;

                        role.ROLEFUNCTIONs.Add(roleFunction);
                    }

                });
                this.roleRepository.Insert(role);
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.UnknownError, ex.Message);
            }

            return ResultCode.NoError;
        }

        public ResultCode UpdatePermission(LOGINUSER loginUser, List<FunctionInfo> functions)
        {
            try
            {
                functions.ForEach(p =>
                {
                    var roleGroupFunction = this.roleGroupFunctionRepository.GetRoleOfUser(p.Id, loginUser.USERSID);
                    if (roleGroupFunction != null)
                    {
                        roleGroupFunction.ACTION = p.GetAction();
                        this.roleGroupFunctionRepository.Update(roleGroupFunction);
                    }
                    else
                    {
                        ROLEFUNCTION roleFunction = GetRoleFunctionById(p.Id);
                        if (roleFunction != null)
                        {
                            roleGroupFunction = new ROLEFUNCTION();
                            roleGroupFunction.FUNCTIONID = p.Id;
                            roleGroupFunction.ROLEID = p.RoleGroupId;
                            roleGroupFunction.ACTION = p.GetAction();
                            this.roleGroupFunctionRepository.Insert(roleGroupFunction);
                        }

                    }
                });
            }
            catch (Exception ex)
            {
                throw new BusinessLogicException(ResultCode.UnknownError, ex.Message);
            }
            return ResultCode.NoError;
        }

        public List<FunctionInfo> GetPermissionOfUser(LOGINUSER loginUser)
        {
            List<FunctionInfo> roleOfEmployee = new List<FunctionInfo>();

            //Set active control of permisson
            loginUser.USERLEVEL.FUNCTIONS.ToList().ForEach(p =>
            {
                roleOfEmployee.Add(new FunctionInfo(p.DETAILFUNCTION, p.ID, p.ACTION.ToCharArray()));
            });

            var roleOfUsers = this.uV_GetFunctionOfUserRepository.GetAll().ToList();
            roleOfUsers.ForEach(p =>
            {
                var functionInfo = roleOfEmployee.FirstOrDefault(i => i.Id == p.ROLEFUNCTIONID);
                if (functionInfo != null && p.ACTION != null)
                {
                    functionInfo.SetValue(p.ACTION.ToCharArray());
                }
            });
            return roleOfEmployee;
        }

        public List<FunctionInfo> FunctionByLevel(string level)
        {
            List<FunctionInfo> roleOfEmployee = new List<FunctionInfo>();
            var userLevel = this.levelRepository.FillterByLevel(level);
            if (userLevel == null)
            {
                return roleOfEmployee;
            }

            userLevel.FUNCTIONS.ToList().ForEach(p =>
            {
                roleOfEmployee.Add(new FunctionInfo(p.DETAILFUNCTION, p.ID, p.ACTION.ToCharArray()));
            });

            return roleOfEmployee;
        }

        public ROLEFUNCTION GetRoleFunctionById(long id)
        {
            ROLEFUNCTION roleFunction = this.roleFunctionRepository.GetById(id);
            return roleFunction;
        }
    }
}