﻿using InvoiceServer.API.Business;
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
    [RoutePrefix("cancelling-invoice")]
    public class CancellingInvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly CancellingInvoiceBusiness business;
        private readonly InvoiceBusiness invoiceBusiness;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceController.CancellingInvoiceController()'
        public CancellingInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceController.CancellingInvoiceController()'
        {
            business = new CancellingInvoiceBusiness(GetBOFactory());
            invoiceBusiness = new InvoiceBusiness(GetBOFactory());// thêm para true để by pass
        }

        #endregion Contructor

        #region API methods
        [HttpPost]
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceController.Create(CancellingInvoiceMaster)'
        public IHttpActionResult Create(CancellingInvoiceMaster cancellingInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CancellingInvoiceController.Create(CancellingInvoiceMaster)'
        {
            var response = new ApiResult<Result>();

            try
            {
                Result result = this.business.Create(cancellingInfo);
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

        [HttpGet]
        [Route("cancel/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{branch?}/{fromInvoiceNo?}/{toInvoiceNo?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Read)]
        public IHttpActionResult FillterInvoiceCancel(long? code = null, string symbol = null, string invoiceNo = null, string customerName = null,
            string taxCode = null, int? invoiceTypeId = null, string numberAccout = null, string dateFrom = null, string dateTo = null, string orderBy = null, string orderType = null,
            int skip = 0, int take = int.MaxValue, int? branch = 0, decimal? fromInvoiceNo = 0, decimal? toInvoiceNo = 0)
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                int invoiceStatus = (int)InvoiceStatus.Released;
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
                    InvoiceSample = invoiceTypeId,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    InvoiceNotiType = InvoiceNotiType.VanCancel,
                    Branch = branch,
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

        #endregion API methods
    }
}