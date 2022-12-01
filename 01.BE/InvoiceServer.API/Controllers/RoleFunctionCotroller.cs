using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("role-functions")]
    public class RoleFunctionController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly RoleFunctionBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionController.RoleFunctionController()'
        public RoleFunctionController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionController.RoleFunctionController()'
        {
            business = new RoleFunctionBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{id}/{lang}")]
        [CustomAuthorize(Roles = UserPermission.RoleManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionController.GetRoleFunctions(long, string)'
        public IHttpActionResult GetRoleFunctions(long id, string lang)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionController.GetRoleFunctions(long, string)'
        {
            var response = new ApiResultList<RoleFunctionInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetRoleFunctions(id, lang);
                response.Message = MsgApiResponse.ExecuteSeccessful;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("update")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionController.UpdateRoleFunctions(UpdateRoleFunctionInfo)'
        public IHttpActionResult UpdateRoleFunctions(UpdateRoleFunctionInfo updateRoleFunctionInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleFunctionController.UpdateRoleFunctions(UpdateRoleFunctionInfo)'
        {
            var response = new ApiResultList<RoleFunctionInfo>();
            try
            {
                response.Code = business.UpdateRoleFunction(updateRoleFunctionInfo);
                response.Message = MsgApiResponse.ExecuteSeccessful;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }

            return Ok(response);
        }

        #endregion API methods
    }
}