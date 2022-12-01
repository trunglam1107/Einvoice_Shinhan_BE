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
    [RoutePrefix("UnitList")]
    public class UnitListController : BaseController
    {
        private static readonly Logger logger = new Logger();
        private readonly UnitListBusiness business;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.UnitListController()'
        public UnitListController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.UnitListController()'
        {
            business = new UnitListBusiness(GetBOFactory());
        }

        [HttpGet]
        [Route("{keyword?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        //[CustomAuthorize(Roles = UserPermission.UnitManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.Fillter(string, string, string, int, int)'
        public IHttpActionResult Fillter(string keyword = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.Fillter(string, string, string, int, int)'
        {
            var response = new ApiResultList<UnitListViewModel>();
            try
            {
                var condition = new ConditionSearchUnitList(orderby, orderType)
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
        [CustomAuthorize(Roles = UserPermission.UnitManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.GetByCode(string)'
        public IHttpActionResult GetByCode(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.GetByCode(string)'
        {
            if (string.IsNullOrEmpty(code))
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<UnitListViewModel>();

            try
            {
                var condition = new ConditionSearchUnitList(null, null)
                {
                    CurrentUser = this.CurrentUser,
                    Key = code.DecodeUrl(),
                };
                response.Data = this.business.GetByCode(condition);
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.MyUnitUsing(long)'
        public IHttpActionResult MyUnitUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.MyUnitUsing(long)'
        {
            var response = new ApiResult<MyUsingModel>();

            try
            {
                response.Data = this.business.MyUnitUsing(id);
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
        [CustomAuthorize(Roles = UserPermission.UnitManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.Update(string, UnitListViewModel)'
        public IHttpActionResult Update(string code, UnitListViewModel unitListViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.Update(string, UnitListViewModel)'
        {
            if (!ModelState.IsValid || unitListViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(code, unitListViewModel);
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
        [CustomAuthorize(Roles = UserPermission.UnitManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.Create(UnitListViewModel)'
        public IHttpActionResult Create(UnitListViewModel unitListViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.Create(UnitListViewModel)'
        {
            if (!ModelState.IsValid || unitListViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(unitListViewModel);
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
        [CustomAuthorize(Roles = UserPermission.UnitManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.Delete(string)'
        public IHttpActionResult Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'UnitListController.Delete(string)'
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