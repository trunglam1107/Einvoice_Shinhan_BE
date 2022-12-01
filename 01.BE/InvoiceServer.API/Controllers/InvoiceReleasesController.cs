using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("invoice-releases")]
    public class InvoiceReleasesController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoiceReleasesBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.InvoiceReleasesController()'
        public InvoiceReleasesController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.InvoiceReleasesController()'
        {
            business = new InvoiceReleasesBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{status?}/{branch?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRelease_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.Fillter(string, string, string, string, int, int)'
        public IHttpActionResult Fillter(string status = null, string branch = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.Fillter(string, string, string, string, int, int)'
        {
            var response = new ApiResultList<InvoiceReleasesMaster>();
            try
            {
                var condition = new ConditionSearchInvoiceReleases(orderby, orderType, status, branch)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRelease_Read + ", " + UserPermission.InvoiceRelease_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.GetInvoiceReleasesInfo(long)'
        public IHttpActionResult GetInvoiceReleasesInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.GetInvoiceReleasesInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceReleasesInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetInvoiceReleasesInfo(id);
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
        [Route("view/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRelease_Read + ", " + UserPermission.InvoiceRelease_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.ViewGetInvoiceReleasesInfo(long)'
        public IHttpActionResult ViewGetInvoiceReleasesInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.ViewGetInvoiceReleasesInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceReleasesInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.ViewGetInvoiceReleasesInfo(id);
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
        // add get webconfig info
        [HttpGet]
        [Route("get-config")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRelease_Read + ", " + UserPermission.InvoiceRelease_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.GetConfigInfo()'
        public IHttpActionResult GetConfigInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.GetConfigInfo()'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<WebConfigInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetConfigInfo();
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
        [CustomAuthorize(Roles = UserPermission.InvoiceRelease_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.Create(InvoiceReleasesInfo)'
        public IHttpActionResult Create(InvoiceReleasesInfo invoiceReleasesInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.Create(InvoiceReleasesInfo)'
        {
            if (!ModelState.IsValid || invoiceReleasesInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(invoiceReleasesInfo);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceRelease_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.Update(long, InvoiceReleasesInfo)'
        public IHttpActionResult Update(long id, InvoiceReleasesInfo invoiceReleasesInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.Update(long, InvoiceReleasesInfo)'
        {
            if (!ModelState.IsValid || invoiceReleasesInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(id, invoiceReleasesInfo);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceRelease_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.Delete(long)'
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

        [HttpPost]
        [Route("change-status/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.UpdateStatus(long, InvoiceReleasesStatus)'
        public IHttpActionResult UpdateStatus(long id, InvoiceReleasesStatus status)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.UpdateStatus(long, InvoiceReleasesStatus)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.UpdateStatus(id, status);
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

        [Route("approved/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.ApprovedReleaseInvoice(long)'
        public IHttpActionResult ApprovedReleaseInvoice(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.ApprovedReleaseInvoice(long)'
        {
            var response = new ApiResult();

            try
            {
                InvoiceReleasesStatus status = new InvoiceReleasesStatus(id, RecordStatus.Approved);
                response.Code = this.business.UpdateStatus(id, status);
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
        [Route("unApproved/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.UnApprovedReleaseInvoice(long)'
        public IHttpActionResult UnApprovedReleaseInvoice(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.UnApprovedReleaseInvoice(long)'
        {
            var response = new ApiResult();

            try
            {
                InvoiceReleasesStatus status = new InvoiceReleasesStatus(id, RecordStatus.Created);
                response.Code = this.business.UpdateStatus(id, status, true);
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
        [Route("template")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.GetReleaseTemplate()'
        public IHttpActionResult GetReleaseTemplate()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesController.GetReleaseTemplate()'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceReleasesTemplateInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetTemplateRelease();
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