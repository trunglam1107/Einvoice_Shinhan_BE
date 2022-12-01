using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("rolegroup")]
    public class RoleController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly RoleBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.RoleController()'
        public RoleController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.RoleController()'
        {
            business = new RoleBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{keyword?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.RoleManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.GetRoles(string, string, string, int, int)'
        public IHttpActionResult GetRoles(string keyword = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.GetRoles(string, string, string, int, int)'
        {
            var response = new ApiResultList<RoleGroupDetail>();
            try
            {
                long totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.GetList(out totalRecords, keyword, orderType, orderby, skip, take);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
                     {CustomHttpRequestHeader.CollectionTotal, totalRecords.ToString()},
                     {CustomHttpRequestHeader.CollectionSkip, skip.ToString()},
                     {CustomHttpRequestHeader.CollectionTake, take.ToString()},
                };

                SetResponseHeaders(responHeaders);
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
        [HttpGet]
        [Route("get-all-role")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.GetAllRoles()'
        public IHttpActionResult GetAllRoles()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.GetAllRoles()'
        {
            var response = new ApiResultList<RoleGroupDetail>();
            try
            {
#pragma warning disable CS0219 // The variable 'totalRecords' is assigned but its value is never used
                long totalRecords = 0;
#pragma warning restore CS0219 // The variable 'totalRecords' is assigned but its value is never used
                response.Code = ResultCode.NoError;
                response.Data = business.GetAllRole();
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
        [HttpGet]
        [Route("{id}")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.GetRole(long)'
        public IHttpActionResult GetRole(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.GetRole(long)'
        {
            var response = new ApiResult<RoleGroupDetail>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetRoleGroup(id);
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
        [Route("update-users-in-role")]
        [CustomAuthorize(Roles = UserPermission.RoleManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.UpdateUsersInRole(UpdateUsersInRole)'
        public IHttpActionResult UpdateUsersInRole(UpdateUsersInRole usersInRole)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.UpdateUsersInRole(UpdateUsersInRole)'
        {
            var response = new ApiResult();
            try
            {
                business.UpdateUsersInRole(usersInRole);
                response.Code = ResultCode.NoError;
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
        [Route("create-users-in-role")]
        [CustomAuthorize(Roles = UserPermission.RoleManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.CreateUsersInRole(UpdateUsersInRole)'
        public IHttpActionResult CreateUsersInRole(UpdateUsersInRole usersInRole)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.CreateUsersInRole(UpdateUsersInRole)'
        {
            var response = new ApiResult();
            try
            {
                business.CreateUsersInRole(usersInRole);
                response.Code = ResultCode.NoError;
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

        [HttpGet]
        [Route("get-roles-by-user/{userId}")]
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Create + ", " + UserPermission.AccountManagement_Update
            + ", " + UserPermission.EmployeesManagement_Create + ", " + UserPermission.EmployeesManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.GetRolesByUserId(long)'
        public IHttpActionResult GetRolesByUserId(long userId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.GetRolesByUserId(long)'
        {
            var response = new ApiResult<AccountRoleInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = this.business.GetRolesByUserId(userId);
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
        [Route("user-roles/create-or-edit")]
        [CustomAuthorize(Roles = UserPermission.AccountManagement_Create + ", " + UserPermission.AccountManagement_Update
             + ", " + UserPermission.EmployeesManagement_Create + ", " + UserPermission.EmployeesManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.CreateOrEditUserRoles(AccountRoleInfo)'
        public IHttpActionResult CreateOrEditUserRoles(AccountRoleInfo accountRoleInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.CreateOrEditUserRoles(AccountRoleInfo)'
        {
            var response = new ApiResult();
            try
            {
                business.CreateOrEditUserRoles(accountRoleInfo);
                response.Code = ResultCode.NoError;
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

        [HttpDelete]
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.RoleManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RoleController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RoleController.Delete(long)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.Delete(id);
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