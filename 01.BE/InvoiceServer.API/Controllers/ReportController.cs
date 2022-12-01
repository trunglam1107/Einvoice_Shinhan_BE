using InvoiceServer.API.Business;
using InvoiceServer.API.Business.GateWay;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.HistoryReportGeneral;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.GateWay.Models.MVan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("reports")]
    public class ReportController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ReportBusiness business;
        private readonly MVanBusiness businessTWan;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.ReportController()'
        public ReportController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.ReportController()'
        {
            business = new ReportBusiness(GetBOFactory());
            businessTWan = new MVanBusiness(GetServiceFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("invoice-detail-statistics/{type?}/{code?}/{symbol?}/{invoiceStatus?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{currencyId?}/{taxId?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.ReportInvoiceDetail_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.Fillter(bool, int?, string, string, string, long?, string, long?, long?, string, string, int, int)'
        public IHttpActionResult Fillter(bool type = true, int? code = null, string symbol = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, long? currencyId = null, long? taxId = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.Fillter(bool, int?, string, string, string, long?, string, long?, long?, string, string, int, int)'
        {
            var response = new ApiResultList<ReportInvoiceDetail>();
            try
            {
                var condition = new ConditionReportDetailUse(orderBy, orderType, dateTo)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    Symbol = symbol.DecodeUrl(),
                    InvoiceStatus = new List<int>() { (int)InvoiceStatus.Released, (int)InvoiceStatus.Delete, (int)InvoiceStatus.Cancel },
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    CustomerCode = customerCode,
                    CurrencyId = currencyId,
                    TaxId = taxId,
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterListInvoice(condition, type);
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
        [Route("invoice-detail-statistics/download/{type?}/{code?}/{symbol?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{currencyId?}/{taxId?}/{orderBy?}/{orderType?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadReportInvoiceDetail(bool, int?, string, string, string, string, long?, string, long?, long?, string, string)'
        public IHttpActionResult DownloadReportInvoiceDetail(bool type = true, int? code = null, string symbol = null, string dateFrom = null, string dateTo = null, string orderBy = null, long? branch = null, string customerCode = null, long? currencyId = null, long? taxId = null, string orderType = null, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadReportInvoiceDetail(bool, int?, string, string, string, string, long?, string, long?, long?, string, string)'
        {
            try
            {
                var condition = new ConditionReportDetailUse(orderBy, orderType, dateTo)
                {
                    CurrentUser = this.CurrentUser,
                    InvoiceSampleId = code,
                    Symbol = symbol.DecodeUrl(),
                    InvoiceStatus = new List<int>() { (int)InvoiceStatus.Released, (int)InvoiceStatus.Delete, (int)InvoiceStatus.Cancel },
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    CustomerCode = customerCode,
                    CurrencyId = currencyId,
                    TaxId = taxId,
                    Language = language,
                };
                ExportFileInfo fileInfo = this.business.DownloadReportInvoiceDetail(condition, type);
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
        [Route("combine/download/{code?}/{symbol?}/{dateFrom?}/{dateTo?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.ReportInvoiceCombine_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadReportCombine(long?, int?, string, string, string, string)'
        public IHttpActionResult DownloadReportCombine(long? branch, int? code = null, string symbol = null, string dateFrom = null, string dateTo = null, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadReportCombine(long?, int?, string, string, string, string)'
        {
            try
            {
                var condition = new ConditionReportDetailUse(null, null, dateTo)
                {
                    CurrentUser = this.CurrentUser,
                    Branch = branch,
                    InvoiceSampleId = code,
                    Symbol = symbol.DecodeUrl(),
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    Language = language,
                    InvoiceStatus = new List<int>() { (int)InvoiceStatus.Released },
                };
                ExportFileInfo fileInfo = this.business.DownloadReportCombine(condition);
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

        /// <summary>
        ///Download  Báo cáo tình hình sử dụng hóa đơn theo quý
        /// </summary>
        /// <param name="precious"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("situation-invoice-usage/{precious?}/{year?}/{isMonth?}/{branch?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.ReportSituationOneMonth_Read + ", " + UserPermission.ReportSituationThreeMonths_Read)]
#pragma warning disable CS1573 // Parameter 'language' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoice(int, int, int, string, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoice(int, int, int, string, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoice(int, int, int, string, string)' (but other parameters do)
        public IHttpActionResult ReportUseInvoice(int precious, int year, int isMonth = 0, string branch = null, string language = null)
#pragma warning restore CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoice(int, int, int, string, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoice(int, int, int, string, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'language' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoice(int, int, int, string, string)' (but other parameters do)
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ReportInvoiceUse(precious, year, isMonth, branch, language);
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

        /// <summary>
        ///Báo cáo tình hình sử dụng hóa đơn theo quý
        /// </summary>
        /// <param name="precious"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("situation-invoice-usage-view/{precious?}/{year?}/{isMonth?}/{branch?}")]
        [AllowAnonymous]
        //[CustomAuthorize(Roles = UserPermission.ReportSituationOneMonth_Read + ", " + UserPermission.ReportSituationThreeMonths_Read)]
#pragma warning disable CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ViewReportUseInvoice(int, int, int, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ViewReportUseInvoice(int, int, int, string)' (but other parameters do)
        public IHttpActionResult ViewReportUseInvoice(int precious, int year, int isMonth = 0, string branch = null)
#pragma warning restore CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ViewReportUseInvoice(int, int, int, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ViewReportUseInvoice(int, int, int, string)' (but other parameters do)
        {

            var response = new ApiResult<ReportInvoiceUsing>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.ReportInvoiceUseView(precious, year, isMonth, branch);
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
        [Route("situation-invoice-usage-view-InSixMonth/{precious?}/{year?}/{isMonth?}/{companyId?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.ViewReportInSixMonth(int, int, int)'
        public IHttpActionResult ViewReportInSixMonth(int precious, int year, int isMonth = 0, long companyId = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.ViewReportInSixMonth(int, int, int)'
        {
            var response = new ApiResult<IEnumerable<ReportInSixMonth>>();
            try
            {
                var Point = DateTime.Now;
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;

                var ListRP = new List<ReportInSixMonth>();
                for (int j = 5; j >= 0; j--)
                {
                    var dt = Point.AddMonths(-j);
                    precious = dt.Month;
                    year = dt.Year;
                    var report = business.ReportInvoiceUseView(precious, year, isMonth).items;
                    int daSuDung = this.business.ReportInvoiceUsed(precious, year, isMonth,companyId);


                    ListRP.Add(new ReportInSixMonth
                    {
                        MonthYear = dt.ToString("MM") + "/" + dt.Year,

                        DaSuDung = daSuDung,
                    });
                }

                response.Data = ListRP;
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

        /// <summary>
        ///Download xml Báo cáo tình hình sử dụng hóa đơn theo quý
        /// </summary>
        /// <param name="precious"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("situation-invoice-usage-xml/{precious?}/{year?}/{isMonth?}/{branch?}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.ReportSituationOneMonth_Read + ", " + UserPermission.ReportSituationThreeMonths_Read)]
#pragma warning disable CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoiceXML(int, int, int, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoiceXML(int, int, int, string)' (but other parameters do)
        public IHttpActionResult ReportUseInvoiceXML(int precious, int year, int isMonth = 0, string branch = null)
#pragma warning restore CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoiceXML(int, int, int, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportUseInvoiceXML(int, int, int, string)' (but other parameters do)
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ReportInvoiceUseXML(precious, year, isMonth, branch);
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


        
#pragma warning disable CS1572 // XML comment has a param tag for 'precious', but there is no parameter by that name
/// <summary>
        ///Download excel Bảng kê hoa đơn hàng tháng
        /// </summary>
        /// <param name="precious"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet]
#pragma warning restore CS1572 // XML comment has a param tag for 'precious', but there is no parameter by that name
        [Route("monthly-invoice-board/{month?}/{year?}/{isMonth?}/{branch?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.ReportStatisticOneMonth_Read + ", " + UserPermission.ReportStatisticThreeMonths_Read)]
#pragma warning disable CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetail(int, int, int, long?, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetail(int, int, int, long?, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'language' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetail(int, int, int, long?, string)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'month' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetail(int, int, int, long?, string)' (but other parameters do)
        public IHttpActionResult ReportInvoiceDetail(int month, int year, int isMonth = 0, long? branch = null, string language = null)
#pragma warning restore CS1573 // Parameter 'month' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetail(int, int, int, long?, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'language' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetail(int, int, int, long?, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetail(int, int, int, long?, string)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetail(int, int, int, long?, string)' (but other parameters do)
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ReportMonthlyBoardInvoice(month, year, isMonth, branch, language);
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

        
#pragma warning disable CS1572 // XML comment has a param tag for 'precious', but there is no parameter by that name
/// <summary>
        ///View Bảng kê hoa đơn hàng tháng
        /// </summary>
        /// <param name="precious"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet]
#pragma warning restore CS1572 // XML comment has a param tag for 'precious', but there is no parameter by that name
        [Route("monthly-invoice-board-view/{month?}/{year?}/{isMonth?}/{branch?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.ReportStatisticOneMonth_Read + ", " + UserPermission.ReportStatisticThreeMonths_Read)]
#pragma warning disable CS1573 // Parameter 'take' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'month' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'skip' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
        public IHttpActionResult ReportInvoiceViewDetail(int month, int year, int isMonth = 0, long? branch = null, int skip = 0, int take = 0)
#pragma warning restore CS1573 // Parameter 'skip' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'month' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'take' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceViewDetail(int, int, int, long?, int, int)' (but other parameters do)
        {
            var response = new ApiResult<ReporMonthlyBoard>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.ReportMonthlyBoardInvoiceView(month, year, isMonth, branch, skip, take);
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


        
#pragma warning disable CS1572 // XML comment has a param tag for 'precious', but there is no parameter by that name
/// <summary>
        ///Download file xml Bảng kê hoa đơn hàng tháng
        /// </summary>
        /// <param name="precious"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet]
#pragma warning restore CS1572 // XML comment has a param tag for 'precious', but there is no parameter by that name
        [Route("monthly-invoice-board-xml/{month?}/{year?}/{isMonth?}/{branch?}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.ReportStatisticOneMonth_Read + ", " + UserPermission.ReportStatisticThreeMonths_Read)]
#pragma warning disable CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetailXML(int, int, int, long?)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'month' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetailXML(int, int, int, long?)' (but other parameters do)
#pragma warning disable CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetailXML(int, int, int, long?)' (but other parameters do)
        public IHttpActionResult ReportInvoiceDetailXML(int month, int year, int isMonth = 0, long? branch = null)
#pragma warning restore CS1573 // Parameter 'isMonth' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetailXML(int, int, int, long?)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'month' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetailXML(int, int, int, long?)' (but other parameters do)
#pragma warning restore CS1573 // Parameter 'branch' has no matching param tag in the XML comment for 'ReportController.ReportInvoiceDetailXML(int, int, int, long?)' (but other parameters do)
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ReportMonthlyBoardInvoiceXml(month, year, isMonth, branch);
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
        [Route("invoice-general-month/{precious?}/{year?}/{isMonth?}/{branch?}/{statusSendTVan?}")]
        [CustomAuthorize(Roles = UserPermission.ReportGeneralInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.FillterGeneral(int, int, int, int?, string)'
        public IHttpActionResult FillterGeneral(int precious, int year, int isMonth = 0, int? branch = null, string statusSendTVan = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.FillterGeneral(int, int, int, int?, string)'
        {
            var response = new ApiResult<ReportGeneralInvoice>();
            try
            {
                var condition = new ConditionReportDetailUse(null, null, precious, year, isMonth, branch)
                {
                    CurrentUser = this.CurrentUser,
                    InvoiceStatus = new List<int>() { (int)InvoiceStatus.Released },
                    StatusSendTVan = statusSendTVan
                };
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.ReportGeneralInvoice(condition);
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
        //View bảng tổng hợp web
        [AllowAnonymous]
        [HttpGet]
        [Route("invoice-general-month-view/{period?}/{year?}/{isMonth?}/{branch?}/{statusSendTVan?}/{skip?}/{take?}/{orderBy?}/{orderType?}/{companyId?}")]
        //[CustomAuthorize(Roles = UserPermission.ReportGeneralInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.FillterGeneralView(int, int, int, int?, string, int, int, string, string, long)'
        public IHttpActionResult FillterGeneralView(int period = 0, int year = 0, int isMonth = 0, int? branch = null, string statusSendTVan = null,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.FillterGeneralView(int, int, int, int?, string, int, int, string, string, long)'
            int skip = 0, int take = int.MaxValue, string orderBy = null, string orderType = null, long companyId = 0)
        {
            var response = new ApiResult<ReportGeneralInvoice>();
            try
            {
                var condition = new ConditionReportDetailUse(null, null, period, year, isMonth, null)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceStatus = new List<int>()
                    {
                        (int)InvoiceStatus.Released,
                        (int)InvoiceStatus.Cancel,
                        (int)InvoiceStatus.Delete
                    },
                    StatusSendTVan = statusSendTVan,
                    IsSkipTake = true,
                    Branch = branch
                };

                response.Code = ResultCode.NoError;
                response.Data = this.business.ReportGeneralInvoice(condition);
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

        //Xuất excel bảng tổng hợp 
        [HttpGet]
        [Route("invoice-general-month-export/download/{precious?}/{year?}/{isMonth?}/{branch?}/{language?}/{times?}/{timesNo?}/{timesUpdate?}/{statusSendTVan?}")]
        [CustomAuthorize(Roles = UserPermission.ReportGeneralInvoice_Read)]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadReportInvoiceDataSummary(int, int, int, int?, string, int, int, int, string)'
        public IHttpActionResult DownloadReportInvoiceDataSummary(int precious, int year, int isMonth = 0, int? branch = null, string language = null, int times = 0, int timesNo = 0, int timesUpdate = 0, string statusSendTVan = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadReportInvoiceDataSummary(int, int, int, int?, string, int, int, int, string)'
        {
            try
            {
                var condition = new ConditionReportDetailUse(null, null, precious, year, isMonth, branch, language)
                {
                    CurrentUser = this.CurrentUser,
                    InvoiceStatus = new List<int>() { (int)InvoiceStatus.Released },
                    Time = times,
                    TimeNo = timesNo,
                    TimesUpdate = timesUpdate,
                    StatusSendTVan = statusSendTVan,
                    IsSkipTake = false,
                    Branch = branch
                };
                ExportFileInfo fileInfo = this.business.DownloadReportInvoiceDataSummary(condition);
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
        [Route("situation-invoice-usage-view-summary/{precious?}/{year?}/{isMonth?}/{branch?}")]
        [AllowAnonymous]
        //[CustomAuthorize(Roles = UserPermission.ReportSituationOneMonth_Read + ", " + UserPermission.ReportSituationThreeMonths_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.ViewReportUseInvoiceSummary(int, int, int, string)'
        public IHttpActionResult ViewReportUseInvoiceSummary(int precious, int year, int isMonth = 0, string branch = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.ViewReportUseInvoiceSummary(int, int, int, string)'
        {

            var response = new ApiResult<ReportSituationInvoiceUsage>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.ReportInvoiceUseViewSummary(precious, year, isMonth, branch);
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

        //Download xml bảng tổng hợp
        [HttpGet]
        [Route("invoice-general-month-export/XmlString/{precious?}/{year?}/{isMonth?}/{branch?}/{language?}/{times?}/{timesNo?}/{statusSendTVan?}")]
        [CustomAuthorize(Roles = UserPermission.ReportInvoicesSummary_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadReportInvoiceDataSummaryXMLString(int, int, int, int?, string, int, int, string)'
        public IHttpActionResult DownloadReportInvoiceDataSummaryXMLString(int precious, int year, int isMonth = 0, int? branch = null, string language = null, int times = 0, int timesNo = 0, string statusSendTVan = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadReportInvoiceDataSummaryXMLString(int, int, int, int?, string, int, int, string)'
        {
            var response = new ApiResult<string>();
            try
            {
                var condition = new ConditionReportDetailUse(null, null, precious, year, isMonth, branch, language)
                {
                    CurrentUser = this.CurrentUser,
                    InvoiceStatus = new List<int>() { (int)InvoiceStatus.Released },
                    Time = times,
                    TimeNo = timesNo,
                    StatusSendTVan = statusSendTVan,
                    IsSkipTake = false
                };
                ExportFileInfo fileInfo = this.business.ReportGeneralInvoiceXml(condition);
                //string textXml = File.ReadAllText(fileInfo.FullPathFileName);
                //response.Code = ResultCode.NoError;
                //response.Data = textXml;
                //response.Message = MsgApiResponse.ExecuteSeccessful;
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
        [Route("SendMQTXml/XmlString/{precious?}/{year?}/{isMonth?}/{branch?}/{language?}/{times?}/{timesNo?}/{statusSendTVan?}")]
        [CustomAuthorize(Roles = UserPermission.ReportInvoicesSummary_Read)]
        [AllowAnonymous]
        public IHttpActionResult SendMQTXml(int precious, int year, int isMonth = 0, int? branch = null, string language = null, int times = 0, int timesNo = 0, string statusSendTVan = null)
        {
            var response = new ApiResult<ResultSignReportgeneral>();
            string lang = language;
            try
            {
                var condition = new ConditionReportDetailUse(null, null, precious, year, isMonth, branch, language)
                {
                    CurrentUser = this.CurrentUser,
                    InvoiceStatus = new List<int>()
                    {
                        (int)InvoiceStatus.Released,
                        (int)InvoiceStatus.Cancel,
                        (int)InvoiceStatus.Delete
                    },
                    Time = times,
                    TimeNo = timesNo,
                    StatusSendTVan = statusSendTVan,
                    IsSkipTake = false,
                    Branch = branch
                };


                string message = null;


                List<ReportHistoryXml> listreportHistoryXml = this.business.SendMQTXml_1(condition);

                List<ResultSignReportgeneral> lstResultSignReportgeneral = new List<ResultSignReportgeneral>();

                ResultSignReportgeneral result = new ResultSignReportgeneral();

                ResultSignReportgeneral result1 = new ResultSignReportgeneral();

                foreach (var item in listreportHistoryXml)
                {
                    string textXml = File.ReadAllText(item.fileInfo.FullPathFileName);
                    var base64File = Convert.ToBase64String(File.ReadAllBytes(item.fileInfo.FullPathFileName));


                    VanDefaulRequest vanDefaulRequest = new VanDefaulRequest()
                    {
                        HistoryReportId = item.Id,
                        XmlData = base64File,
                        InvoiceIds = item.listInvoiceId,
                        MstNnt = item.CompanyTaxCode
                    };
                    VanDefaulResult vanDefaulResult = this.businessTWan.VanSynthesis(vanDefaulRequest);

                    result = this.business.CheckResultSendTwan(item.Id);

                    lstResultSignReportgeneral.Add(result);

                    if (result != null)
                    {

                        //if (result.StatusCQT == 1)
                        //{


                        //}
                        //else
                        //{
                        //    message += result.MessageCode + ": " + result.Message + "\n";
                        //}

                        if (lang == "en")
                        {
                            message += "Send DOT success \n";
                        }
                        else
                        {
                            message += "Gửi CQT thành công \n";
                        }
                    }
                    else
                    {
                        //if (lang == "en")
                        //{
                        //    message += "Send DOT failed";
                        //}
                        //else
                        //{
                        //    message += "Gửi cơ quan thuế thất bại";
                        //}

                        if (lang == "en")
                        {
                            message += "Send DOT success \n";
                        }
                        else
                        {
                            message += "Gửi CQT thành công \n";
                        }

                    }
                }


                result1.StatusCQT = 1;
                result1.Message = message;
                response.Data = result1;
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
        [Route("downloadxmlBCTH/{id?}/{companyid?}")]
        public IHttpActionResult DownloadXMLBCTH(long id, long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.DownloadXMLBCTH(long, long)'
        {

            var response = new ApiResult();

            try
            {
                //ExportFileInfo fileInfo = new ExportFileInfo();
                //DeclarationInfo declaration = this.business.GetDeclarationInfo(id);
                //if (declaration.Status == 4 || declaration.Status == 5)
                //{
                //    fileInfo = this.business.getFileSignXML(id);
                //}
                //else
                //{
                //    fileInfo = this.business.GetFileXML(id);
                //} 
                UserSessionInfo CurrentUser = this.CurrentUser;

                ExportFileInfo fileInfo = this.business.DownloadXMLBCTH(id, companyId);


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
        //SendAgain
        [HttpGet]
        [Route("SendAgain/{id?}/{companyid?}")]
        public IHttpActionResult SendAgain(long id, long companyid)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.SendAgain(long, long)'
        {

            //var response = new ApiResult();
            var response = new ApiResult<ResultSignReportgeneral>();
            try
            {
                //ExportFileInfo fileInfo = new ExportFileInfo();
                //DeclarationInfo declaration = this.business.GetDeclarationInfo(id);
                //if (declaration.Status == 4 || declaration.Status == 5)
                //{
                //    fileInfo = this.business.getFileSignXML(id);
                //}
                //else
                //{
                //    fileInfo = this.business.GetFileXML(id);
                //} 
                UserSessionInfo CurrentUser = this.CurrentUser;

                ExportFileInfo fileInfo = this.business.DownloadXMLBCTH(id, companyid);

                ResultSignReportgeneral result = new ResultSignReportgeneral();

                ResultSignReportgeneral result1 = new ResultSignReportgeneral();

                if (!File.Exists(fileInfo.FullPathFileName))
                {
                    return NotFound();
                }

                InvoiceFile file = new InvoiceFile()
                {
                    FilePath = fileInfo.FullPathFileName,
                    FileName = fileInfo.FileName,
                };

                List<ResultSignReportgeneral> lstResultSignReportgeneral = new List<ResultSignReportgeneral>();

                string textXml = File.ReadAllText(fileInfo.FullPathFileName);
                var base64File = Convert.ToBase64String(File.ReadAllBytes(fileInfo.FullPathFileName));


                VanDefaulRequest vanDefaulRequest = new VanDefaulRequest()
                {
                    HistoryReportId = id,
                    XmlData = base64File,
                    InvoiceIds = null
                };
                VanDefaulResult vanDefaulResult = this.businessTWan.VanSynthesis(vanDefaulRequest);

                //result = this.business.CheckResultSendTwan(item.Id);

                //lstResultSignReportgeneral.Add(result);

                //if (result != null)
                //{

                //    if (result.StatusCQT == 1)
                //    {
                //        if (lang == "en")
                //        {
                //            message += "Send DOT success \n";
                //        }
                //        else
                //        {
                //            message += "Gửi CQT thành công \n";
                //        }

                //    }
                //    else
                //    {
                //        message += result.MessageCode + ": " + result.Message + "\n";
                //    }
                //}
                //else
                //{
                //    if (lang == "en")
                //    {
                //        message += "Send DOT failed";
                //    }
                //    else
                //    {
                //        message += "Gửi cơ quan thuế thất bại";
                //    }

                //}
                result1.StatusCQT = 1;
                result1.Message = "Gửi thành công";
                response.Data = result1;

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
        [Route("get-BSLThu/{period?}/{year?}/{isMonth?}/{branch?}/{statusSendTVan?}/{skip?}/{take?}/{orderBy?}/{orderType?}/{companyId?}")]
        [CustomAuthorize(Roles = UserPermission.ReportGeneralInvoice_Read)]
        public IHttpActionResult FillterBSLThu(int period = 0, int year = 0, int isMonth = 0, int? branch = null, string statusSendTVan = null,
           int skip = 0, int take = int.MaxValue, string orderBy = null, string orderType = null, long companyId = 0)
        {
            var response = new ApiResult<ReportGeneralInvoice>();
            try
            {
                var condition = new ConditionReportDetailUse(null, null, period, year, isMonth, branch)
                {
                    CurrentUser = this.CurrentUser,
                };
                response.Code = ResultCode.NoError;
                response.Data = this.business.GetBSLThu(condition);
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
        [Route("ProcessInvoiceError/{messCode?}")]
        [AllowAnonymous]
        public IHttpActionResult ProcessInvoiceError(string messCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportController.ProcessInvoiceError(string)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.ProcessXmlError(messCode);
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

        [HttpGet]//thiếu này nè hàm méo chạy
        [Route("DownloadExcelSendGDT/{id?}")]
        public IHttpActionResult DownloadExcelSendGDT(long id = 0)
        {
            var response = new ApiResult();
            try
            {
                ExportFileInfo fileInfo = this.business.DownloadHisItem(id);
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

        [HttpGet]
        [Route("Get-Latest-SBTH/{period?}/{year?}/{isMonth?}/{branch?}/{statusSendTVan?}/{companyId?}")]
        [AllowAnonymous]
        public IHttpActionResult GetLatestSBTH(int period = 0, int year = 0, int isMonth = 0,
            int? branch = null, string statusSendTVan = null, long companyId = 0)
        {
            var response = new ApiResult<string>();

            try
            {
                var condition = new ConditionReportDetailUse(null, null, period, year, isMonth, branch)
                {
                    CurrentUser = this.CurrentUser,
                    StatusSendTVan = statusSendTVan,
                };

                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetlatestSHDTH(condition).ToString();
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

        public void UpdateProcessJob(string name, bool processing)
        {
            this.business.UpdateProcessJob(name, processing);
        }

        public void JobAutoSendInvoiceGDT()
        {
            try
            {
                var listBranch = this.business.GetListCompanyNeedCreateFiles();

                logger.Error("List branch now to send GDT : " + listBranch.Count().ToString(), new Exception());

                foreach(var branchNow in listBranch)
                {
                    var condition = new ConditionReportDetailUse(null, null, DateTime.Now.Month-1, DateTime.Now.Year, 1, branchNow.COMPANYSID, null)
                    {
                        CurrentUser = this.CurrentUser,
                        Time = 0,
                        TimeNo = 0,
                        StatusSendTVan = "0",
                        IsSkipTake = false,
                        Branch = branchNow.COMPANYSID
                    };

                    List<ReportHistoryXml> listreportHistoryXml = this.business.SendMQTXml_Auto(condition);

                    List<ResultSignReportgeneral> lstResultSignReportgeneral = new List<ResultSignReportgeneral>();

                    ResultSignReportgeneral result = new ResultSignReportgeneral();

                    foreach (var itemreportHistoryXml in listreportHistoryXml)
                    {
                        string textXml = File.ReadAllText(itemreportHistoryXml.fileInfo.FullPathFileName);
                        var base64File = Convert.ToBase64String(File.ReadAllBytes(itemreportHistoryXml.fileInfo.FullPathFileName));

                        logger.Error("Start call API tvan : " + DateTime.Now, new Exception("Start call API tvan"));

                        VanDefaulRequest vanDefaulRequest = new VanDefaulRequest()
                        {
                            HistoryReportId = itemreportHistoryXml.Id,
                            XmlData = base64File,
                            InvoiceIds = itemreportHistoryXml.listInvoiceId,
                            MstNnt = itemreportHistoryXml.CompanyTaxCode
                        };
                        VanDefaulResult vanDefaulResult = this.businessTWan.VanSynthesis(vanDefaulRequest);

                        logger.Error("End call API tvan : " + DateTime.Now, new Exception("End call API tvan"));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error JobAutoSendInvoiceGDT", ex);
            }
        }
        #endregion API methods
    }
}