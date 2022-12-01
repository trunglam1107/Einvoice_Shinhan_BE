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
    [RoutePrefix("roles")]
    public class UserLevelController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly LevelBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UserLevelController.UserLevelController()'
        public UserLevelController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UserLevelController.UserLevelController()'
        {
            business = new LevelBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UserLevelController.GetRoles()'
        public IHttpActionResult GetRoles()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UserLevelController.GetRoles()'
        {
            var response = new ApiResultList<RoleDetail>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetList();
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UserLevelController.GetRole(long)'
        public IHttpActionResult GetRole(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UserLevelController.GetRole(long)'
        {
            var response = new ApiResult<RoleDetail>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetRole(id);
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
        [Route("create-or-edit")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UserLevelController.CreateOrEdit(RoleDetail)'
        public IHttpActionResult CreateOrEdit(RoleDetail roleDetail)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UserLevelController.CreateOrEdit(RoleDetail)'
        {
            var response = new ApiResult();
            try
            {
                business.CreateOrEdit(roleDetail);
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

        #endregion API methods
    }
}