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
    [RoutePrefix("hold-invoice")]
    public class HoldInvoiceController : BaseController
    {
        private static readonly Logger logger = new Logger();
        private readonly HoldInvoiceBusiness business;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.HoldInvoiceController()'
        public HoldInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.HoldInvoiceController()'
        {
            business = new HoldInvoiceBusiness(GetBOFactory());
        }

        [HttpGet]
        [Route("{registerTemplateId?}/{symbol?}/{dateInvoice?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.HoldInvoiceManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.Fillter(string, string, string, string, string, int, int)'
        public IHttpActionResult Fillter(string registerTemplateId = null, string symbol = null, string dateInvoice = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.Fillter(string, string, string, string, string, int, int)'
        {
            var response = new ApiResultList<HoldInvoiceViewModel>();
            try
            {
                long totalRecords = 0;
                var condition = new ConditionSearchHoldInvoice(this.CurrentUser, registerTemplateId, symbol, dateInvoice, orderType, orderby);
                response.Code = ResultCode.NoError;
                response.Data = business.Fillter(out totalRecords, condition, skip, take);
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

        [HttpPost]
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.HoldInvoiceManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.Create(HoldInvoiceViewModel)'
        public IHttpActionResult Create(HoldInvoiceViewModel holdInvoiceViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.Create(HoldInvoiceViewModel)'
        {
            if (!ModelState.IsValid || holdInvoiceViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(holdInvoiceViewModel);
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
        [CustomAuthorize(Roles = UserPermission.HoldInvoiceManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.GetByCode(long)'
        public IHttpActionResult GetByCode(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.GetByCode(long)'
        {
            var response = new ApiResult<HoldInvoiceViewModel>();

            try
            {
                response.Data = this.business.GetById(id);
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
        [CustomAuthorize(Roles = UserPermission.HoldInvoiceManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.Update(HoldInvoiceViewModel)'
        public IHttpActionResult Update(HoldInvoiceViewModel holdInvoiceViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.Update(HoldInvoiceViewModel)'
        {
            if (!ModelState.IsValid || holdInvoiceViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(holdInvoiceViewModel);
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
        [CustomAuthorize(Roles = UserPermission.HoldInvoiceManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'HoldInvoiceController.Delete(long)'
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
    }
}