using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("invoices-adjustment")]
    public class InvoiceAdjustmentController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoiceBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.InvoiceAdjustmentController()'
        public InvoiceAdjustmentController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.InvoiceAdjustmentController()'
        {
            business = new InvoiceBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{invoiceTypeId?}/{symbol?}/{invoiceStatus?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceType?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.Fillter(long?, string, int?, string, string, string, int?, string, string, string, string, string, string, int, int)'
        public IHttpActionResult Fillter(long? invoiceTypeId = null, string symbol = null, int? invoiceStatus = null, string invoiceNo = null, string customerName = null, string taxCode = null, int? invoiceType = null, string numberAccout = null, string dateFrom = null, string dateTo = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.Fillter(long?, string, int?, string, string, string, int?, string, string, string, string, string, string, int, int)'
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                var condition = new ConditionSearchInvoice(orderBy, orderType, invoiceStatus)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = invoiceTypeId,
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    InvoiceSample = invoiceType,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
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
        [Route("{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.GetInvoiceInfo(long)'
        public IHttpActionResult GetInvoiceInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.GetInvoiceInfo(long)'
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.RegistInvoiceNo(long, string)'
        public IHttpActionResult RegistInvoiceNo(long templateId, string symbol)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.RegistInvoiceNo(long, string)'
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
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.Create(InvoiceInfo)'
        public IHttpActionResult Create(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.Create(InvoiceInfo)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(invoiceInfo);
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
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.Update(long, InvoiceInfo)'
        public IHttpActionResult Update(long id, InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.Update(long, InvoiceInfo)'
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

        [HttpDelete]
        [Route("{id}/{companyId}")]
        public IHttpActionResult Delete(long id,long companyId = 0)
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.Delete(id, companyId);
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.UpdateStatus(long, InvoiceReleasesStatus)'
        public IHttpActionResult UpdateStatus(long id, InvoiceReleasesStatus status)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.UpdateStatus(long, InvoiceReleasesStatus)'
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
        [Route("update-note/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.UpdateStatus(long, InvoiceNote)'
        public IHttpActionResult UpdateStatus(long id, InvoiceNote noteInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceAdjustmentController.UpdateStatus(long, InvoiceNote)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.UpdateNote(id, noteInfo);
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