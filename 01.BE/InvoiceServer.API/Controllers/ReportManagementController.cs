using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.HistoryReportGeneral;
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
    [RoutePrefix("report-management")]
    public class ReportManagementController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ReportManagementBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementController.ReportManagementController()'
        public ReportManagementController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementController.ReportManagementController()'
        {
            business = new ReportManagementBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("customers/{dateFrom?}/{dateTo?}/{contractType?}/{id?}/{name?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementController.ViewReportCustomers(string, string, int?, long?, string, int, int)'
        public IHttpActionResult ViewReportCustomers(string dateFrom = null, string dateTo = null, int? contractType = null, long? id = null, string name = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementController.ViewReportCustomers(string, string, int?, long?, string, int, int)'
        {
            var response = new ApiResultList<ReportCustomer>();
            try
            {
                long totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.ReportCustomers(out totalRecords, dateFrom, dateTo, 1, id, name);
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
        [Route("customers-download/{dateFrom?}/{dateTo?}/{contractType?}/{id?}/{name?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementController.ReportCustomers(string, string, int?, long?, string)'
        public IHttpActionResult ReportCustomers(string dateFrom = null, string dateTo = null, int? contractType = null, long? id = null, string name = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementController.ReportCustomers(string, string, int?, long?, string)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ReportCustomersDownload(dateFrom, dateTo, 1, id, name);
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
        [Route("history-report-general/get-history-report-general/{skip?}/{take?}/{orderBy?}/{orderType?}/{companyId?}/{BranchID?}/{dateFrom?}/{dateTo?}/{hrgStatus?}/{ReportType?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementController.FilterHRGALL(string, string, int, int, long, string, string, string, int, int)'
        public IHttpActionResult FilterHRGALL(string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementController.FilterHRGALL(string, string, int, int, long, string, string, string, int, int)'
            long companyId = 0, string BranchID = null
            , string dateFrom = null, string dateTo = null, int hrgStatus = 0, int ReportType = 0)
        {
            var response = new ApiResultList<InvoiceServer.Business.Models.HistoryReportGeneralInfo>();
            try
            {
                var condition = new FilterHistoryReportGeneralCondition()
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                };

                if (companyId > 0) {
                    condition.CompanyId = long.Parse(companyId.ToString().DecodeUrl());
                }
                
                condition.BranchID = BranchID.DecodeUrl();
                condition.DateFrom = dateFrom.DecodeUrl().ConvertDateTime();
                condition.DateTo = dateTo.DecodeUrl().ConvertDateTime();

                if (hrgStatus > 0)
                {
                    condition.Status = hrgStatus;
                }
                if (ReportType > 0)
                {
                    condition.ReportType = ReportType ;
                }

                response.Code = ResultCode.NoError;
                response.Data = business.GetAll(condition);
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

        [HttpGet]//thiếu này nè hàm méo chạy
        [Route("DownloadExcelHistoryGeneralReportUrl/{companyId?}/{BranchID?}/{dateFrom?}/{dateTo?}/{hrgStatus?}/{ReportType?}")]
        public IHttpActionResult DownloadExcelHGRALL(long companyId = 0, string BranchID = null
                    , string dateFrom = null, string dateTo = null, int hrgStatus = 0, int ReportType = 0)
        {
            var response = new ApiResult();
            try
            {
                var condition = new FilterHistoryReportGeneralCondition()
                {
                    CurrentUser = this.CurrentUser,
                };

                condition.CompanyId = companyId;
                condition.BranchID = BranchID.DecodeUrl();
                condition.DateFrom = dateFrom.DecodeUrl().ConvertDateTime();
                condition.DateTo = dateTo.DecodeUrl().ConvertDateTime();

                if (hrgStatus > 0)
                {
                    condition.Status = hrgStatus;
                }
                if (ReportType > 0)
                {
                    condition.ReportType = ReportType;
                }

                ExportFileInfo fileInfo = business.DownloadExcelHRG(condition);


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
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }
        #endregion API methods
    }
}