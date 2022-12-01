using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
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
    [RoutePrefix("InvoiceType")]
    public class InvoiceTypeController : BaseController
    {
        private static readonly Logger logger = new Logger();
        private readonly InvoiceTypeBusiness business;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.InvoiceTypeController()'
        public InvoiceTypeController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.InvoiceTypeController()'
        {
            business = new InvoiceTypeBusiness(GetBOFactory());
        }

        [HttpGet]
        [Route("{keyword?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceTypeManagement_Read + ", " + UserPermission.RegisterTemplate_Read
            + ", " + UserPermission.InvoiceSampleManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.Fillter(string, string, string, int, int)'
        public IHttpActionResult Fillter(string keyword = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.Fillter(string, string, string, int, int)'
        {
            var response = new ApiResultList<InvoiceTypeViewModel>();
            try
            {
                var condition = new ConditionSearchInvoiceType(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Key = keyword.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Data = business.Fillter(condition);
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
        [Route("{code}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceTypeManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.GetByCode(string)'
        public IHttpActionResult GetByCode(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.GetByCode(string)'
        {
            if (string.IsNullOrEmpty(code))
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceTypeViewModel>();

            try
            {
                response.Data = this.business.GetByCode(code);
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
        [Route("using/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.MyInvoiceTypeUsing(long)'
        public IHttpActionResult MyInvoiceTypeUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.MyInvoiceTypeUsing(long)'
        {
            var response = new ApiResult<MyUsingModel>();

            try
            {
                response.Data = this.business.MyInvoiceTypeUsing(id);
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
        [Route("{code}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceTypeManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.Update(string, InvoiceTypeViewModel)'
        public IHttpActionResult Update(string code, InvoiceTypeViewModel invoiceTypeViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.Update(string, InvoiceTypeViewModel)'
        {
            if (!ModelState.IsValid || invoiceTypeViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(code, invoiceTypeViewModel);
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
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.InvoiceTypeManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.Create(InvoiceTypeViewModel)'
        public IHttpActionResult Create(InvoiceTypeViewModel invoiceTypeViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.Create(InvoiceTypeViewModel)'
        {
            if (!ModelState.IsValid || invoiceTypeViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(invoiceTypeViewModel);
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
        [Route("{code}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceTypeManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.Delete(string)'
        public IHttpActionResult Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTypeController.Delete(string)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.Delete(code);
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
    }
}