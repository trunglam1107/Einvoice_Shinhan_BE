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
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("replaced-invoices")]
    public class ReplacedInvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ReplacedInvoiceBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceController.ReplacedInvoiceController()'
        public ReplacedInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceController.ReplacedInvoiceController()'
        {
            business = new ReplacedInvoiceBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [CustomAuthorize(Roles = UserPermission.InvoiceReplacement_Read)]
        [Route("{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{dateFrom?}/{dateTo?}/{branch?}/{invoiceNoReplaced?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceController.Fillter(int?, string, string, string, string, string, string, long?, string, string, string, string, int, int)'
        public IHttpActionResult Fillter(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, string dateFrom = null, string dateTo = null, long? branch = null, string invoiceNoReplaced = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceController.Fillter(int?, string, string, string, string, string, string, long?, string, string, string, string, int, int)'
        {
            var response = new ApiResultList<ReplacedInvoice>();
            try
            {
                var condition = new SearchReplacedInvoice(orderBy, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    InvoiceSamples = new List<int>() { (int)InvoiceType.Substitute },
                    InvoiceSymbol = symbol.DecodeUrl(),
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    InvoiceNoReplaced = invoiceNoReplaced,
                    CustomerCode = customerCode,
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
        [Route("export-excel/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{dateFrom?}/{dateTo?}/{branch?}/{invoiceNoReplaced?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceController.ExportExcel(int?, string, string, string, string, string, string, long?, string, string, string, string, int, int, string)'
        public IHttpActionResult ExportExcel(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, string dateFrom = null, string dateTo = null, long? branch = null, string invoiceNoReplaced = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceController.ExportExcel(int?, string, string, string, string, string, string, long?, string, string, string, string, int, int, string)'
        {
            try
            {
                var condition = new SearchReplacedInvoice(orderBy, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    InvoiceSamples = new List<int>() { (int)InvoiceType.Substitute },
                    InvoiceSymbol = symbol.DecodeUrl(),
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    InvoiceNoReplaced = invoiceNoReplaced,
                    CustomerCode = customerCode,
                    Language = language,
                };
                ExportFileInfo fileInfo = this.business.ExportExcel(condition);
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