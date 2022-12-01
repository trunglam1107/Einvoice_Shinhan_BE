using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    [RoutePrefix("AutoNumber")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController'
    public class AutoNumberController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController'
    {
        private static readonly Logger logger = new Logger();
        private readonly AutoNumberBusiness business;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.AutoNumberController()'
        public AutoNumberController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.AutoNumberController()'
        {
            business = new AutoNumberBusiness(GetBOFactory());
        }


        [HttpGet]
        [Route("GetAll")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.GetAll(string, string, int, int)'
        public IHttpActionResult GetAll(string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.GetAll(string, string, int, int)'
        {
            var response = new ApiResultList<AutoNumberViewModel>();
            try
            {
                var condition = new ConditionSearchAutoNumber(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                };
                response.Code = ResultCode.NoError;
                response.Data = business.GetAll(condition);
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
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.GetById(string)'
        public IHttpActionResult GetById(string id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.GetById(string)'
        {
            var response = new ApiResult<AutoNumberViewModel>();
            try
            {

                response.Code = ResultCode.NoError;
                response.Data = business.GetById(id);
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
        [CustomAuthorize(Roles = UserPermission.AutoNumberManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.Update(string, AutoNumberViewModel)'
        public IHttpActionResult Update(string code, AutoNumberViewModel autoNumberViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.Update(string, AutoNumberViewModel)'
        {
            if (!ModelState.IsValid || autoNumberViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(code, autoNumberViewModel);
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.Create(AutoNumberViewModel)'
        public IHttpActionResult Create(AutoNumberViewModel autoNumberViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.Create(AutoNumberViewModel)'
        {
            if (!ModelState.IsValid || autoNumberViewModel == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(autoNumberViewModel);
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.Delete(string)'
        public IHttpActionResult Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.Delete(string)'
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

        [HttpGet]
        [Route("LoadAnnoucement/{invoiceId}/{typeId}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.LoadAnnoucement(long, string)'
        public IHttpActionResult LoadAnnoucement(long invoiceId, string typeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.LoadAnnoucement(long, string)'
        {
            var response = new ApiResult<string>();
            try
            {

                response.Code = ResultCode.NoError;
                response.Data = business.loadAnnoucement(invoiceId, typeId);
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
        [Route("checkMorethanOneMinus/{typeId}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.checkMorethanOneMinus(string)'
        public IHttpActionResult checkMorethanOneMinus(string typeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.checkMorethanOneMinus(string)'
        {
            var response = new ApiResult<string>();
            try
            {

                response.Code = ResultCode.NoError;
                response.Data = business.checkMorethanOneMinus(typeId).ToString();
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
        [Route("LoadAnnoucementLength/{typeId}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.LoadAnnoucementLength(string)'
        public IHttpActionResult LoadAnnoucementLength(string typeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AutoNumberController.LoadAnnoucementLength(string)'
        {
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.loadAnnoucementLenth(typeId).ToString();
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