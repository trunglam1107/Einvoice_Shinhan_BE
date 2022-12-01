using InvoiceServer.API.Business.GateWay;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.GateWay.Models.MVan;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers.GateWay
{
    [RoutePrefix("mVan")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController'
    public class MVanController :BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController'
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly MVanBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.MVanController()'
        public MVanController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.MVanController()'
        {
            business = new MVanBusiness(GetServiceFactory());
        }

        #endregion Contructor

        #region VAN
        [HttpPost]
        [Route("Receivedata")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.MVanReceivedata(MVanResponse)'
        public IHttpActionResult MVanReceivedata(MVanResponse input)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.MVanReceivedata(MVanResponse)'
        {
            var response = new MVanResult();
            try
            {
                response.code = "00";
                response.message = "";
                response.data = this.business.MVanReceivedata(input);
            }
            catch (Exception ex)
            {
                response.code = "01";
                response.message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }

        [HttpGet]
        [Route("MVanReceived/{maThongDiep}/{type}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.MVanReceived(string, int)'
        public IHttpActionResult MVanReceived(string maThongDiep, int type)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.MVanReceived(string, int)'
        {
            var response = new ApiResult<VanResponseResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = "";
                response.Data = this.business.MVanReceived(maThongDiep, false, type);
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
        [Route("VAN/SignUp")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanSignUp(VanSignUpRequest)'
        public IHttpActionResult VanSignUp(VanSignUpRequest vanSignUpRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanSignUp(VanSignUpRequest)'
        {
            var response = new ApiResult<VanSignUpResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.VanSignUp(vanSignUpRequest);
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
        [Route("VAN/Login")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanLogin(VanLoginRequest)'
        public IHttpActionResult VanLogin(VanLoginRequest vanLoginRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanLogin(VanLoginRequest)'
        {
            var response = new ApiResult<VanLoginResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.VanLogin(vanLoginRequest);
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
        [Route("VAN/Register")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanRegister(VanDefaulRequest)'
        public IHttpActionResult VanRegister(VanDefaulRequest vanDefaulRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanRegister(VanDefaulRequest)'
        {
            var response = new ApiResult<VanDefaulResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.VanRegister(vanDefaulRequest);
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
        [Route("VAN/SendInvoiceWithCode")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanSendInvoiceWithCode(VanDefaulRequest)'
        public IHttpActionResult VanSendInvoiceWithCode(VanDefaulRequest vanDefaulRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanSendInvoiceWithCode(VanDefaulRequest)'
        {
            var response = new ApiResult<VanDefaulResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.SendInvoiceWithCode(vanDefaulRequest);
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
        [Route("VAN/SendInvoiceNotCode")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanSendInvoiceNotCode(VanDefaulRequest)'
        public IHttpActionResult VanSendInvoiceNotCode(VanDefaulRequest vanDefaulRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanSendInvoiceNotCode(VanDefaulRequest)'
        {
            var response = new ApiResult<VanDefaulResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.SendInvoiceNotCode(vanDefaulRequest);
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
        [Route("VAN/Cancel")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanCancel(VanDefaulRequest)'
        public IHttpActionResult VanCancel(VanDefaulRequest vanDefaulRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanCancel(VanDefaulRequest)'
        {
            var response = new ApiResult<VanDefaulResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.VanCancel(vanDefaulRequest);
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
        [Route("VAN/Synthesis")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanSynthesis(VanDefaulRequest)'
        public IHttpActionResult VanSynthesis(VanDefaulRequest vanDefaulRequest)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.VanSynthesis(VanDefaulRequest)'
        {
            var response = new ApiResult<VanDefaulResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.VanSynthesis(vanDefaulRequest);
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
        [Route("VAN/getInvoiceSystemSettings")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.GetInvoiceSystemSettings()'
        public IHttpActionResult GetInvoiceSystemSettings()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.GetInvoiceSystemSettings()'
        {

            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceSystemSettingInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetInvoiceSystemSettingDetail();
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
        [Route("VAN/updateInvoiceSystemSettings")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'MVanController.UpdateInvoiceSystemSettings(InvoiceSystemSettingInfo)'
        public IHttpActionResult UpdateInvoiceSystemSettings(InvoiceSystemSettingInfo info)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'MVanController.UpdateInvoiceSystemSettings(InvoiceSystemSettingInfo)'
        {

            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceSystemSettingInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.UpdateInvoiceSystemSettingDetail(info);
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
        #endregion VAN
    }
}
