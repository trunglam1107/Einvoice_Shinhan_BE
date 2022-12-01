using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
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
    [RoutePrefix("fileSystemLogs")]
    public class FileSystemLogController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly FileSystemLogBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogController.FileSystemLogController()'
        public FileSystemLogController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogController.FileSystemLogController()'
        {
            business = new FileSystemLogBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpGet]
        [Route("{dateFrom?}/{dateTo?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogController.GetSystemLogDetail(string, string, string, string, int, int)'
        public IHttpActionResult GetSystemLogDetail(string dateFrom = null, string dateTo = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogController.GetSystemLogDetail(string, string, string, string, int, int)'
        {

            var response = new ApiResultList<FileSystemLogInfo>();
            try
            {
                long totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.FillterSystemLog(out totalRecords, dateFrom, dateTo, orderby, orderType, skip, take);
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
        [Route("download/{fileName?}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.FileSystemLog_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogController.DownloadData(string)'
        public IHttpActionResult DownloadData(string fileName)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogController.DownloadData(string)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.DownloadFileLogs(fileName);
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
        #endregion API methods
    }
}