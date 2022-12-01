using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionBusiness'
    public class RoleFunctionBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionBusiness'
    {
        #region Fields, Properties

        private readonly IRoleFunctionBO roleFunctionBO;
        private readonly IRoleBO roleBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionBusiness.RoleFunctionBusiness(IBOFactory)'
        public RoleFunctionBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionBusiness.RoleFunctionBusiness(IBOFactory)'
        {
            this.roleFunctionBO = boFactory.GetBO<IRoleFunctionBO>();
            this.roleBO = boFactory.GetBO<IRoleBO>();
        }

        #endregion Contructor

        #region Methods
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionBusiness.GetRoleFunctions(long, string)'
        public List<RoleFunctionInfo> GetRoleFunctions(long roleId, string lang)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionBusiness.GetRoleFunctions(long, string)'
        {
            List<RoleFunctionInfo> functionsOfRole = new List<RoleFunctionInfo>();

            this.roleFunctionBO.InitRoleFunction(roleId);
            var listRoleFunctions = this.roleFunctionBO.InitRoleFunctionNew(roleId);
            var roleFunctions = this.roleBO.GetRoleById(roleId);
            if (roleFunctions == null)
            {
                return functionsOfRole;
            }

            if (lang == "vi")
            {
                //roleFunctions.ROLEFUNCTION.OrderBy(f => f.FUNCTIONS.ORDERS).ToList().ForEach(p =>
                //{
                //    functionsOfRole.Add(new RoleFunctionInfo(p.FUNCTIONS.DETAILFUNCTION, p.ID, p.ACTION?.ToCharArray(), p.FUNCTIONS.ACTION?.ToCharArray()));
                //});
                roleFunctions.ROLEFUNCTIONs.OrderBy(f => f.FUNCTION.ORDERS).ToList().ForEach(p =>
                {
                    listRoleFunctions.ForEach(x =>
                    {
                        if (p.FUNCTIONID == x.FUNCTIONID)
                        {
                            functionsOfRole.Add(new RoleFunctionInfo(p.FUNCTION.DETAILFUNCTION, p.ID, p.ACTION?.ToCharArray(), p.FUNCTION.ACTION?.ToCharArray()));
                        }
                    });
                });
            }
            else if (lang == "en")
            {
                //roleFunctions.ROLEFUNCTION.OrderBy(f => f.FUNCTIONS.ORDERS).ToList().ForEach(p =>
                //{
                //    functionsOfRole.Add(new RoleFunctionInfo(p.FUNCTIONS.DETAILFUNCTIONEN, p.ID, p.ACTION?.ToCharArray(), p.FUNCTIONS.ACTION?.ToCharArray()));
                //});
                roleFunctions.ROLEFUNCTIONs.OrderBy(f => f.FUNCTION.ORDERS).ToList().ForEach(p =>
                {
                    listRoleFunctions.ForEach(x =>
                    {
                        if (p.FUNCTIONID == x.FUNCTIONID)
                        {
                            functionsOfRole.Add(new RoleFunctionInfo(p.FUNCTION.DETAILFUNCTIONEN, p.ID, p.ACTION?.ToCharArray(), p.FUNCTION.ACTION?.ToCharArray()));
                        }
                    });
                });
            }


            functionsOfRole = functionsOfRole.Select(f => { f.RoleId = roleId; return f; }).ToList();

            return functionsOfRole;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionBusiness.UpdateRoleFunction(UpdateRoleFunctionInfo)'
        public ResultCode UpdateRoleFunction(UpdateRoleFunctionInfo updateRoleFunctionInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionBusiness.UpdateRoleFunction(UpdateRoleFunctionInfo)'
        {
            if (updateRoleFunctionInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.roleFunctionBO.UpdateRoleFunction(updateRoleFunctionInfo);
        }

        #endregion
    }
}