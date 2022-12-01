using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("session")]
    public class SessionController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly SessionBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionController.SessionController()'
        public SessionController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionController.SessionController()'
        {
            business = new SessionBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        /// <summary>
        /// Request login to new session
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public IHttpActionResult Login(LoginInfo loginInfo)
        {

            if (!ModelState.IsValid || loginInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<UserSessionInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = Authentication.LoginSuccessful;
                response.Data = this.business.Login(loginInfo);
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                this.business.SaveAuditLogInOut("Login", (int)SystemLog_LogType.Error, loginInfo.UserId);
                logger.Error(loginInfo.UserId, ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                this.business.SaveAuditLogInOut("Login", (int)SystemLog_LogType.Error, loginInfo.UserId);
                logger.Error(loginInfo.UserId, ex);
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("client-login")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionController.ClientLogin(LoginInfo)'
        public IHttpActionResult ClientLogin(LoginInfo loginInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionController.ClientLogin(LoginInfo)'
        {

            if (!ModelState.IsValid || loginInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<UserSessionInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = Authentication.LoginSuccessful;
                response.Data = this.business.ClientLogin(loginInfo);
                this.business.SaveAuditLogInOut("PortalLogin", (int)SystemLog_LogType.Success, loginInfo.UserId);
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                this.business.SaveAuditLogInOut("PortalLogin", (int)SystemLog_LogType.Error, loginInfo.UserId);
                logger.Error(loginInfo.UserId, ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                this.business.SaveAuditLogInOut("PortalLogin", (int)SystemLog_LogType.Error, loginInfo.UserId);
                logger.Error(loginInfo.UserId, ex);
            }

            return Ok(response);
        }

        /// <summary>
        /// Request logout current session
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            string authorizeToken = GetAuthenticatedToken();
            if (string.IsNullOrWhiteSpace(authorizeToken))
            {
                return Error(ResultCode.TokenInvalid, Authentication.TokenInvalid);
            }

            var response = new ApiResult();

            try
            {
                ResultCode resultCode = this.business.Logout(authorizeToken);
                response.Code = resultCode;
                response.Message = Authentication.LogoutSuccessful;
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

        /// <summary>
        /// Check a session alive or not
        /// </summary>
        /// <param name="token">Session token</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{token}")]
        [AllowAnonymous]
        public IHttpActionResult CheckSessionStatus(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            var response = new ApiResult();

            var resultCode = ResultCode.NoError;
            try
            {
                resultCode = this.business.CheckUserSession(token);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = ex.Message;
                logger.Error(string.Empty, ex);
            }

            switch (resultCode)
            {
                case ResultCode.SessionAlive:
                    {
                        response.Code = ResultCode.NoError;
                        response.Message = MsgApiResponse.ExecuteSeccessful;
                        break;
                    }
                case ResultCode.SessionEnded:
                    {
                        response.Code = ResultCode.TokenInvalid;
                        response.Message = "Session is timeout";
                        break;
                    }
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("reset-password")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionController.ResetPassword(ResetPassword)'
        public async Task<IHttpActionResult> ResetPassword(ResetPassword resetPasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionController.ResetPassword(ResetPassword)'
        {
            if (!ModelState.IsValid || resetPasswordInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                response.Code = await this.business.ResetPassword(resetPasswordInfo) ? ResultCode.NoError : ResultCode.UnknownError;
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
        [Route("new-password")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionController.UpdatePassword(ChangePassword)'
        public IHttpActionResult UpdatePassword(ChangePassword updatePasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionController.UpdatePassword(ChangePassword)'
        {
            if (!ModelState.IsValid || updatePasswordInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                this.business.UpdatePassword(updatePasswordInfo);
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
        [Route("account-info/{token}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionController.GetAccountInfo(string)'
        public IHttpActionResult GetAccountInfo(string token)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionController.GetAccountInfo(string)'
        {

            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest();
            }

            var response = new ApiResult<UserSessionInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = Authentication.LoginSuccessful;
                response.Data = this.business.GetAccountInfo(token);
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(token, ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(token, ex);
            }

            return Ok(response);
        }


        [HttpGet]
        [Route("get-type-auth")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionController.GetTypeAuthentication()'
        public IHttpActionResult GetTypeAuthentication()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionController.GetTypeAuthentication()'
        {
            var response = new ApiResult<string>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetTypeAuthentication();
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(string.Empty, ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(string.Empty, ex);
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("save-audit-logout/{log?}/{flgSuccess?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SessionController.SaveAuditLogout(string, int)'
        public IHttpActionResult SaveAuditLogout(string log, int flgSuccess = (int)SystemLog_LogType.Success)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SessionController.SaveAuditLogout(string, int)'
        {
            var response = new ApiResult();
            try
            {
                var token = GetAuthenticatedToken();
                this.business.SaveAuditLogInOut(log, flgSuccess, token);
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