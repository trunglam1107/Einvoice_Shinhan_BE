using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// Thông báo sử dụng hóa đơn
    /// </summary>
    [RoutePrefix("notice-use-invoice")]
    public class NoticeUseInvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly NoticeUseInvoiceBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.NoticeUseInvoiceController()'
        public NoticeUseInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.NoticeUseInvoiceController()'
        {
            business = new NoticeUseInvoiceBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{status?}/{branch?}/{dateFrom?}/{dateTo?}/{orderby?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceNotification_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.Fillter(string, string, string, string, string, string, int, int, long)'
        public IHttpActionResult Fillter(string status = null, string branch = null, string dateFrom = null, string dateTo = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue, long companyId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.Fillter(string, string, string, string, string, string, int, int, long)'
        {
            var response = new ApiResultList<NotificeUseInvoiceMaster>();
            try
            {
                var condition = new ConditionSearchUseInvoice(orderby, orderType, status, branch)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
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
        [Route("type-invoice")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.GetInvoiceSample()'
        public IHttpActionResult GetInvoiceSample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.GetInvoiceSample()'
        {
            var response = new ApiResultList<RegisterTemplateMaster>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetInvoiceSample();
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
        [Route("startNumberInvoiceUse/{invoiceTemplateId?}/{prefix?}/{suffix?}/{companyId?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.GetStartNumberInvoiceUse(long, string, string, long)'
        public IHttpActionResult GetStartNumberInvoiceUse(long invoiceTemplateId, string prefix = null, string suffix = null, long companyId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.GetStartNumberInvoiceUse(long, string, string, long)'
        {
            var response = new ApiResult<InvoiceNumber>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetNumumberInvoiceStart(invoiceTemplateId, prefix, suffix, companyId);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceNotification_Read + ", " + UserPermission.InvoiceNotification_Update)]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.GetInvoiceReleasesInfo(long)'
        public IHttpActionResult GetInvoiceReleasesInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.GetInvoiceReleasesInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<UseInvoiceInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetNoticeUseInvoiceInfo(id);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceNotification_Read + ", " + UserPermission.InvoiceNotification_Update)]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.ViewGetInvoiceReleasesInfo(long)'
        public IHttpActionResult ViewGetInvoiceReleasesInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.ViewGetInvoiceReleasesInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<UseInvoiceInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.ViewGetNoticeUseInvoiceInfo(id);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceNotification_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.Create(UseInvoiceInfo)'
        public IHttpActionResult Create(UseInvoiceInfo useInvoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.Create(UseInvoiceInfo)'
        {
            if (useInvoiceInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(useInvoiceInfo);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceNotification_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.Update(long, UseInvoiceInfo)'
        public IHttpActionResult Update(long id, UseInvoiceInfo useInvoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.Update(long, UseInvoiceInfo)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(id, useInvoiceInfo);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceNotification_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.Delete(long)'
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.UpdateStatus(long, InvoiceReleasesStatus)'
        public IHttpActionResult UpdateStatus(long id, InvoiceReleasesStatus status)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.UpdateStatus(long, InvoiceReleasesStatus)'
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

        [HttpPost]
        [Route("approved/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.UpdateStatus(long)'
        public IHttpActionResult UpdateStatus(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.UpdateStatus(long)'
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
        [HttpPost]
        [Route("unApproved/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.UnUpdateStatus(long)'
        public IHttpActionResult UnUpdateStatus(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.UnUpdateStatus(long)'
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
        [Route("download/{id?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.ReportNotificationInvoiceXML(long)'
        public IHttpActionResult ReportNotificationInvoiceXML(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.ReportNotificationInvoiceXML(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.DownloadNotificationInvoiceXml(id);
                if (!File.Exists(fileInfo.FullPathFileName))
                {
                    return NotFound();
                }

                InvoiceFile file = new InvoiceFile()
                {
                    FilePath = fileInfo.FullPathFileName,
                    FileName = fileInfo.FileName,
                };

                SetResponseHeaders("Content-Disposition", "inline; filename=" + fileInfo.FileName);
                return Ok(file);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file");
            }
        }

        [HttpGet]
        [Route("download-pdffile/{id?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.ReportNotificationInvoicePDF(long)'
        public IHttpActionResult ReportNotificationInvoicePDF(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'NoticeUseInvoiceController.ReportNotificationInvoicePDF(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.DownloadNotificationInvoicePdf(id);
                if (!File.Exists(fileInfo.FullPathFileName))
                {
                    return NotFound();
                }

                InvoiceFile file = new InvoiceFile()
                {
                    FilePath = fileInfo.FullPathFileName,
                    FileName = null,
                };

                SetResponseHeaders("Content-Disposition", "inline; filename=" + fileInfo.FileName);
                return Ok(file);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message);
            }
        }

        #endregion API methods
    }
}