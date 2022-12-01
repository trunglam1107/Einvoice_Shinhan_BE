using InvoiceServer.API.Business;
using InvoiceServer.Common.Constants;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("employee")]
    public class EmployeeController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly EmployeeBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

        public EmployeeController()
        {
            business = new EmployeeBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods


        [HttpGet]
        [Route("{name?}/{loginId?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.EmployeesManagement_Read)]
        //[AllowAnonymous]
        public IHttpActionResult FillterEmployee(string name = null, string loginId = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
        {
            var response = new ApiResultList<EmployeeInfo>();
            try
            {
                int totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.FillterUser(out totalRecords, name, loginId, orderby, orderType, skip, take);
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


        [HttpGet]
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.EmployeesManagement_Read)]
        public IHttpActionResult GetEmployeeInfo(int id)
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<EmployeeInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetEmployeeInfo(id);
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
        [CustomAuthorize(Roles = UserPermission.EmployeesManagement_Create)]
        public IHttpActionResult CreateEmployee(EmployeeInfo employeeInfo)
        {
            if (!ModelState.IsValid || employeeInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.CreateEmployee(employeeInfo);
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
        [Route("{id:int}")]
        [CustomAuthorize(Roles = UserPermission.EmployeesManagement_Update)]
        public IHttpActionResult UpdateEmployee(int id, EmployeeInfo employeeInfo)
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.UpdateUser(id, employeeInfo);
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
        [Route("{id:int}")]
        [CustomAuthorize(Roles = UserPermission.EmployeesManagement_Delete)]
        public IHttpActionResult DeleteEmployee(int id)
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.DeleteUser(id);
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