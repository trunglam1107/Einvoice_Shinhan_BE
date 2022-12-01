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
    [RoutePrefix("adjust-invoice")]
    public class AdjustInvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly AdjustInvoiceBusiness business;
        private readonly InvoiceBusiness invoiceBusiness;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.AdjustInvoiceController()'
        public AdjustInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.AdjustInvoiceController()'
        {
            business = new AdjustInvoiceBusiness(GetBOFactory());
            invoiceBusiness = new InvoiceBusiness(GetBOFactory());// thêm para true để by pass
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{dateFrom?}/{dateTo?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{branch?}")]
        [CustomAuthorize(Roles = UserPermission.AdjustedInvoice_Read)]
        public IHttpActionResult Fillter(long? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, string dateFrom = null, string dateTo = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue
            , int? branch = 0)
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
                    InvoiceSamples = new List<int>() { 
                        (int)InvoiceType.AdjustmentInfomation, 
                        (int)InvoiceType.AdjustmentUpDown, 
                        (int)InvoiceType.AdjustmentTax, 
                        (int)InvoiceType.AdjustmentTaxCode,
                        (int)InvoiceType.AdjustmentElse
                    },
                    InvoiceSymbol = symbol.DecodeUrl(),
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    NumberAccout = null,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch
                };
                response.Code = ResultCode.NoError;
                response.Data = business.Fillter(condition);
                long totalRecords = condition.TotalRecords;
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
        [Route("search/{code?}/{symbol?}/{invoiceNo?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceAdjustment_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.FillterAdjustInvoice(int?, string, string)'
        public IHttpActionResult FillterAdjustInvoice(int? code = null, string symbol = null, string invoiceNo = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.FillterAdjustInvoice(int?, string, string)'
        {
            var response = new ApiResult<InvoiceMaster>();
            try
            {
                var condition = new ConditionSearchInvoice(null, null, (int)InvoiceStatus.Released)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = 0,
                    Take = 1,
                    InvoiceSampleId = code,
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    IsInvoiceChange = true,
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterAdjustInvoice(condition);
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
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.GetInvoiceInfo(long)'
        public IHttpActionResult GetInvoiceInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.GetInvoiceInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetInvoicenfo(id);
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
        [Route("pre-add-info/{templateId}/{symbol}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.RegistInvoiceNo(long, string)'
        public IHttpActionResult RegistInvoiceNo(long templateId, string symbol)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.RegistInvoiceNo(long, string)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceNo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.RegistInvoiceNo(templateId, symbol);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceAdjustment_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.Create(InvoiceInfo)'
        public IHttpActionResult Create(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.Create(InvoiceInfo)'
        {
            var response = new ApiResult<Result>();

            try
            {
                Result result = this.business.Create(invoiceInfo);
                response.Code = result == null ? ResultCode.UnknownError : ResultCode.NoError;
                response.Data = result;
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
        [Route("uploadAdjustReceiptFile")]
        [CustomAuthorize(Roles = UserPermission.InvoiceAdjustment_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.UploadAdjustReceiptFile(InvoiceInfo)'
        public IHttpActionResult UploadAdjustReceiptFile(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.UploadAdjustReceiptFile(InvoiceInfo)'
        {
            var response = new ApiResult<Result>();

            try
            {
                response.Code = this.business.UpdateAdjustFileRecipt(invoiceInfo);
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
        [Route("cancel/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceSampleId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{fromInvoiceNo?}/{toInvoiceNo?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Read)]
        public IHttpActionResult FillterInvoiceCancel(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceSampleId = null, string numberAccount = null, string dateFrom = null, string dateTo = null, long? branch = 0, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue,
            decimal? fromInvoiceNo = 0, decimal? toInvoiceNo = 0)
        {
            logger.Error("FillterInvoiceCancel : current branch :  " + branch.ToString() + " dateFrom : " + dateFrom + " dateto : " + dateTo, new Exception());

            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                List<int> invoiceStatus = new List<int>() { (int)InvoiceStatus.Released };
                var condition = new ConditionSearchInvoice(orderBy, orderType, invoiceStatus)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    PersonContact = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    InvoiceSample = invoiceSampleId,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    CustomerCode = customerCode,
                    InvoiceNotiType = InvoiceNotiType.VanAdjustment,
                    FromInvoiceNo = fromInvoiceNo,
                    ToInvoiceNo = toInvoiceNo
                };
                response.Code = ResultCode.NoError;
                response.Data = invoiceBusiness.FillterV2(condition);
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
        [HttpPost]
        [Route("uploadCancelReceiptFile")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.UploadCancelReceiptFile(InvoiceInfo)'
        public IHttpActionResult UploadCancelReceiptFile(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.UploadCancelReceiptFile(InvoiceInfo)'
        {
            var response = new ApiResult<Result>();

            try
            {
                response.Code = this.business.UpdateCancelFileRecipt(invoiceInfo);
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
        [Route("export-excel/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{dateFrom?}/{dateTo?}/{branch?}/{invoiceNoReplaced?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.ExportExcel(int?, string, string, string, string, string, string, int?, string, string, string, string, int, int, string)'
        public IHttpActionResult ExportExcel(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, string dateFrom = null, string dateTo = null, int? branch = null, string invoiceNoReplaced = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceController.ExportExcel(int?, string, string, string, string, string, string, int?, string, string, string, string, int, int, string)'
        {
            try
            {
                var condition = new SearchReplacedInvoice(orderBy, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    InvoiceSamples = new List<int>() { 
                        (int)InvoiceType.AdjustmentInfomation, 
                        (int)InvoiceType.AdjustmentUpDown, 
                        (int)InvoiceType.AdjustmentTax, 
                        (int)InvoiceType.AdjustmentTaxCode,
                        (int)InvoiceType.AdjustmentElse
                    },
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