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
    [RoutePrefix("systemSettings")]
    public class SystemSettingController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly SystemSettingBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.SystemSettingController()'
        public SystemSettingController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.SystemSettingController()'
        {
            business = new SystemSettingBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.GetSystemSettingDetail()'
        public IHttpActionResult GetSystemSettingDetail()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.GetSystemSettingDetail()'
        {

            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<SystemSettingInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetSystemSettingDetail((long)this.CurrentUser.Company.Id);
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
        [Route("updateSystemSettings")]
        [CustomAuthorize(Roles = UserPermission.SystemSetting_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.CreateOrUpdateSystemSetting(SystemSettingInfo)'
        public IHttpActionResult CreateOrUpdateSystemSetting(SystemSettingInfo systemsSettingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.CreateOrUpdateSystemSetting(SystemSettingInfo)'
        {
            if (!ModelState.IsValid || systemsSettingInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.CreateOrUpdateSystemSetting((long)this.CurrentUser.Company.Id, systemsSettingInfo);
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
        [Route("getSlotSignServer")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.GetSlotSignServer()'
        public IHttpActionResult GetSlotSignServer()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.GetSlotSignServer()'
        {
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetSlots();
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
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }
        [HttpGet]
        [Route("checkSlotServerSign/{slot?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.CheckSlotServerSign(int?)'
        public bool CheckSlotServerSign(int? slot)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemSettingController.CheckSlotServerSign(int?)'
        {
            bool result = false;
            try
            {
                result = this.business.CheckSlots(slot, (long)this.CurrentUser.Company.Id);
            }
            catch (BusinessLogicException ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return result;
        }
        #endregion API methods
    }
}