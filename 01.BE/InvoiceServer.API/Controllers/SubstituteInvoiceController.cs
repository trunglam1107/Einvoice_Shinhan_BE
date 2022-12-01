using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("substitute-invoices")]
    public class SubstituteInvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly SubstituteInvoiceBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.SubstituteInvoiceController()'
        public SubstituteInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.SubstituteInvoiceController()'
        {
            business = new SubstituteInvoiceBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.SubstituteInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.Fillter(int?, string, string, string, string, string, string, long?, string, string, string, int, int)'
        public IHttpActionResult Fillter(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.Fillter(int?, string, string, string, string, string, string, long?, string, string, string, int, int)'
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                var condition = new ConditionSearchInvoice(orderBy, orderType, (int)InvoiceStatus.Cancel)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    CustomerCode = customerCode,
                    TaxCode = taxCode.DecodeUrl(),
                    Branch = branch,
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.SubstituteInvoice_Create + ", " + UserPermission.InvoiceAdjustment_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.GetInvoiceInfo(long)'
        public IHttpActionResult GetInvoiceInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.GetInvoiceInfo(long)'
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.RegistInvoiceNo(long, string)'
        public IHttpActionResult RegistInvoiceNo(long templateId, string symbol)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.RegistInvoiceNo(long, string)'
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
        [CustomAuthorize(Roles = UserPermission.SubstituteInvoice_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.Create(InvoiceInfo)'
        public IHttpActionResult Create(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.Create(InvoiceInfo)'
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.SubstituteInvoice_Create + ", " + UserPermission.InvoiceManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.Update(long, InvoiceInfo)'
        public IHttpActionResult Update(long id, InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceController.Update(long, InvoiceInfo)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(id, invoiceInfo);
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