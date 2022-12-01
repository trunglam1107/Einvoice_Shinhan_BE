using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    [RoutePrefix("working-calendar")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController'
    public class WorkingCalendarController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController'
    {
        #region Fields, Properties
        private static readonly Logger logger = new Logger();
        private readonly WorkingCalendarBussiness business;
        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.WorkingCalendarController()'
        public WorkingCalendarController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.WorkingCalendarController()'
        {
            business = new WorkingCalendarBussiness(GetBOFactory());
        }

        #endregion Contructor

        #region API method
        [HttpGet]
        [Route("{year}")]
        [CustomAuthorize(Roles = UserPermission.WorkingCalendar_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.GetWorkingCalendar(int)'
        public IHttpActionResult GetWorkingCalendar(int year)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.GetWorkingCalendar(int)'
        {
            var response = new ApiResultList<WorkingCalendarDTO>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetWorkingCalendar(year);
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
        [Route("get-years")]
        [CustomAuthorize(Roles = UserPermission.WorkingCalendar_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.GetYears()'
        public IHttpActionResult GetYears()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.GetYears()'
        {
            var response = new ApiResult<FullYearWorkingCalendarDAO>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetYears();
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
        [CustomAuthorize(Roles = UserPermission.WorkingCalendar_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.Update(WorkingCalendarDTO)'
        public IHttpActionResult Update(WorkingCalendarDTO calendarDTO)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.Update(WorkingCalendarDTO)'
        {
            if (!ModelState.IsValid || calendarDTO == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult();
            try
            {
                response.Code = business.UpdateWorkingCalendar(calendarDTO);
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
        [Route("add-year")]
        [CustomAuthorize(Roles = UserPermission.WorkingCalendar_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.AddYear()'
        public IHttpActionResult AddYear()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.AddYear()'
        {
            var response = new ApiResult<FullYearWorkingCalendarDAO>();
            try
            {
                response.Data = business.AddYearWorkingCalendar();
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
        #endregion

        #region Job
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.IsRunJob(long)'
        public bool IsRunJob(long quarztId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WorkingCalendarController.IsRunJob(long)'
        {
            return business.IsRunJob(quarztId);
        }
        #endregion Job
    }
}