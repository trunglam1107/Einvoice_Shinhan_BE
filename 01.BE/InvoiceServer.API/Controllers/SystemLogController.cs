using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
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
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("systemLogs")]
    public class SystemLogController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly SystemLogBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogController.SystemLogController()'
        public SystemLogController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogController.SystemLogController()'
        {
            business = new SystemLogBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpGet]
        [Route("{userName?}/{functionName?}/{dateFrom?}/{dateTo?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogController.GetSystemLogDetail(string, string, string, string, string, string, int, int)'
        public IHttpActionResult GetSystemLogDetail(string userName = null, string functionName = null, string dateFrom = null, string dateTo = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogController.GetSystemLogDetail(string, string, string, string, string, string, int, int)'
        {

            var response = new ApiResultList<SystemLogInfo>();
            try
            {
                var condition = new ConditionSearchSystemLog(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    UserName = userName.DecodeUrl(),
                    FucntionName = functionName.DecodeUrl(),
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterSystemLog(condition);
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
        [Route("download/{userName?}/{functionName?}/{dateFrom?}/{dateTo?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.SystemLog_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogController.DownloadData(string, string, string, string, string)'
        public IHttpActionResult DownloadData(string userName = null, string functionName = null, string dateFrom = null, string dateTo = null, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogController.DownloadData(string, string, string, string, string)'
        {
            try
            {
                var condition = new ConditionSearchSystemLog("LOGDATE", "ASC")
                {
                    CurrentUser = this.CurrentUser,
                    UserName = userName.DecodeUrl(),
                    FucntionName = functionName.DecodeUrl(),
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Language = language,
                };
                ExportFileInfo fileInfo = this.business.DownloadDataSystemLogs(condition);
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

        [HttpPost]
        [Route("save-audit")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogController.SaveAudit(SaveSystemLog)'
        public IHttpActionResult SaveAudit(SaveSystemLog saveSystemLog)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogController.SaveAudit(SaveSystemLog)'
        {
            var response = new ApiResult();
            try
            {
                this.business.SaveAudit(saveSystemLog);
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
        #endregion API methods
    }
}