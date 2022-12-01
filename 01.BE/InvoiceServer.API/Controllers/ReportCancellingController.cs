using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.ReportCancelling;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("reportcancelling")]
    public class ReportCancellingController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ReportCancellingBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.ReportCancellingController()'
        public ReportCancellingController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.ReportCancellingController()'
        {
            business = new ReportCancellingBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.ReportCancelling_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.GetReportCancelling()'
        public IHttpActionResult GetReportCancelling()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.GetReportCancelling()'
        {
            var response = new ApiResultList<ReportCancellingMaster>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.FilterReportCancelling();
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
        [CustomAuthorize(Roles = UserPermission.ReportCancelling_Read)]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.GetContractInfo(long)'
        public IHttpActionResult GetContractInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.GetContractInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ReportCancellingInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetReportCancelling(id);
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
        [CustomAuthorize(Roles = UserPermission.ReportCancelling_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.Create(ReportCancellingInfo)'
        public IHttpActionResult Create(ReportCancellingInfo cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.Create(ReportCancellingInfo)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.Create(cancellingInfo);
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
        [Route("approved")]
        [CustomAuthorize(Roles = UserPermission.ReportCancelling_Approve)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.ApproveCancelling(ApprovedReportCancelling)'
        public IHttpActionResult ApproveCancelling(ApprovedReportCancelling cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.ApproveCancelling(ApprovedReportCancelling)'
        {
            var response = new ApiResult();
            try
            {

                response.Code = this.business.ApproveCancelling(cancellingInfo);
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
        [Route("revert-approved")]
        [CustomAuthorize(Roles = UserPermission.ReportCancelling_Approve)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.revertApproveCancelling(ApprovedReportCancelling)'
        public IHttpActionResult revertApproveCancelling(ApprovedReportCancelling cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.revertApproveCancelling(ApprovedReportCancelling)'
        {
            var response = new ApiResult();
            try
            {


                response.Code = this.business.revertApproveCancelling(cancellingInfo);
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.ReportCancelling_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.Update(long, ReportCancellingInfo)'
        public IHttpActionResult Update(long id, ReportCancellingInfo cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.Update(long, ReportCancellingInfo)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(id, cancellingInfo);
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
        [CustomAuthorize(Roles = UserPermission.ReportCancelling_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportCancellingController.Delete(long)'
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


        #endregion API methods
    }
}