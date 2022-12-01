using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("emailActive")]
    public class EmailActiveController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly EmailActiveBusiness business;

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveController.EmailActiveController()'
        public EmailActiveController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveController.EmailActiveController()'
        {
            business = new EmailActiveBusiness(GetBOFactory());
        }

        #endregion

        #region API methods
        [HttpGet]
        [Route("{invoiceNo?}/{dateInvoice?}/{emailTo?}/{status?}/{dateFrom?}/{dateTo?}/{branch?}/{emailType?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.EmailActiveManagement_Read)]
        public IHttpActionResult FillterEmailActive(string invoiceNo = null, string customerCode = null, string dateInvoice = null, string emailTo = null, int? status = null, string dateFrom = null, string dateTo = null, string branch = null, int? emailType = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue, string teller = null)
        {
            var response = new ApiResultList<EmailActiveInfo>();
            try
            {
                var condition = new ConditionSearchEmailActive(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    DateInvoice = dateInvoice.DecodeUrl().ConvertDateTime(),
                    EmailTo = emailTo.DecodeUrl(),
                    Status = status,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch.ToInt(),
                    EmailType = emailType,
                    Teller = teller,
                    CustomerCode = customerCode
                };
                if (condition.DateTo.HasValue)
                    condition.DateTo = condition.DateTo.Value.AddDays(1).AddTicks(-1);
                response.Code = ResultCode.NoError;
                response.Data = business.Filter(condition);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
                     {CustomHttpRequestHeader.CollectionTotal, condition.TotalRecords.ToString()},
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.EmailActiveManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveController.GetEmailActiveInfo(long)'
        public IHttpActionResult GetEmailActiveInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveController.GetEmailActiveInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<EmailActiveInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetEmailActive(id);
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
        [Route("sendemail/{id}")]
        [CustomAuthorize(Roles = UserPermission.EmailActiveManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveController.SendEmailActive(long, EmailActiveInfo)'
        public IHttpActionResult SendEmailActive(long id, EmailActiveInfo emailActive)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveController.SendEmailActive(long, EmailActiveInfo)'
        {

            var response = new ApiResult();

            try
            {
                response.Code = this.business.SendEmailActive(id, emailActive);
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
        [Route("sendemails")]
        [CustomAuthorize(Roles = UserPermission.EmailActiveManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveController.SendEmailsActive(SendMultiMail)'
        public IHttpActionResult SendEmailsActive(SendMultiMail emailActives)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmailActiveController.SendEmailsActive(SendMultiMail)'
        {

            var response = new ApiResult();

            try
            {
                response.Code = this.business.SendEmailActives(emailActives);
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