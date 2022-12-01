using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    [RoutePrefix("emailserver")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController'
    public class EmailServerController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController'
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly EmailServerBusiness business;

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController.EmailServerController()'
        public EmailServerController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController.EmailServerController()'
        {
            business = new EmailServerBusiness(GetBOFactory());
        }

        #endregion

        #region API methods
        [HttpGet]
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.EmailServerManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController.GetEmailServerInfo()'
        public IHttpActionResult GetEmailServerInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController.GetEmailServerInfo()'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<EmailServerInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetEmailServer();
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
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.EmailServerManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController.Create(EmailServerInfo)'
        public IHttpActionResult Create(EmailServerInfo emailServerInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController.Create(EmailServerInfo)'
        {
            if (!ModelState.IsValid || emailServerInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(emailServerInfo);
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
        [Route("send-email-test")]
        [CustomAuthorize(Roles = UserPermission.EmailServerManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController.SendEmailTest(SendMailTest)'
        public IHttpActionResult SendEmailTest(SendMailTest sendMailTest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailServerController.SendEmailTest(SendMailTest)'
        {
            if (!ModelState.IsValid || sendMailTest == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.SendEmailTest(sendMailTest);
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
        #endregion
    }
}