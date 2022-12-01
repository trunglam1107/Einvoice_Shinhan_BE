using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Business.Models.InvoiceSample_Type;
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
    [RoutePrefix("CurrencyList")]
    public class CurrencyListController : BaseController
    {
        private static readonly Logger logger = new Logger();
        private readonly CurrencyListBusiness business;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.CurrencyListController()'
        public CurrencyListController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.CurrencyListController()'
        {
            business = new CurrencyListBusiness(GetBOFactory());
        }

        [HttpGet]
        [Route("{keyword?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.CurrencyManagement_Read)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.Fillter(string, string, string, int, int)'
        public IHttpActionResult Fillter(string keyword = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.Fillter(string, string, string, int, int)'
        {
            var response = new ApiResultList<CurrencyListViewModel>();
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
        [CustomAuthorize(Roles = UserPermission.CurrencyManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.GetByCode(string)'
        public IHttpActionResult GetByCode(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.GetByCode(string)'
        {
            if (string.IsNullOrEmpty(code))
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<CurrencyListViewModel>();

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
        //[CustomAuthorize(Roles = UserPermission.CurrencyManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.MyCurrencyUsing(long)'
        public IHttpActionResult MyCurrencyUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.MyCurrencyUsing(long)'
        {
            var response = new ApiResult<MyUsingModel>();

            try
            {
                response.Data = this.business.MyCurrencyUsing(id);
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
        [CustomAuthorize(Roles = UserPermission.CurrencyManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.Create(CurrencyListViewModel)'
        public IHttpActionResult Create(CurrencyListViewModel currencyListViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.Create(CurrencyListViewModel)'
        {
            if (!ModelState.IsValid || currencyListViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(currencyListViewModel);
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
        [CustomAuthorize(Roles = UserPermission.CurrencyManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.Update(string, CurrencyListViewModel)'
        public IHttpActionResult Update(string code, CurrencyListViewModel currencyListViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.Update(string, CurrencyListViewModel)'
        {
            if (!ModelState.IsValid || currencyListViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(code, currencyListViewModel);
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
        [CustomAuthorize(Roles = UserPermission.CurrencyManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.Delete(string)'
        public IHttpActionResult Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CurrencyListController.Delete(string)'
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