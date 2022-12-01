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
    [RoutePrefix("invoice-delete")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController'
    public class InvoiceDeleteController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController'
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoiceDeleteBusiness business;
        private readonly InvoiceBusiness invoiceBusiness;
        private readonly JobReplaceInvoiceBusiness jobReplaceInvoiceBusiness;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController.InvoiceDeleteController()'
        public InvoiceDeleteController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController.InvoiceDeleteController()'
        {
            business = new InvoiceDeleteBusiness(GetBOFactory());
            invoiceBusiness = new InvoiceBusiness(GetBOFactory());// thêm para true để by pass
            jobReplaceInvoiceBusiness = new JobReplaceInvoiceBusiness(GetBOFactory());
        }

        #endregion Contructor
        [HttpPost]
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController.InvoiceDelete(CancellingInvoiceMaster)'
        public IHttpActionResult InvoiceDelete(CancellingInvoiceMaster deleteInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController.InvoiceDelete(CancellingInvoiceMaster)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.InvoiceDelete(deleteInfo);
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
        [Route("cancel/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{branch?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Read)]
        public IHttpActionResult FillterInvoiceCancel(long? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, int? invoiceTypeId = null, string numberAccout = null, string dateFrom = null, string dateTo = null, 
            string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, int? branch = 0)
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
                    InvoiceNotiType = InvoiceNotiType.VanDelete,
                    Branch = branch,
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
        [Route("deleteMultipleInvoice")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController.MultipleInvoiceDelete(CancellingInvoiceMaster)'
        public IHttpActionResult MultipleInvoiceDelete(CancellingInvoiceMaster deleteInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController.MultipleInvoiceDelete(CancellingInvoiceMaster)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.MultipleInvoiceDelete(deleteInfo);
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
        [Route("cancelMultipleInvoice")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController.MultipleInvoiceDelete(CancellingInvoiceMaster)'
        public IHttpActionResult MultipleInvoiceCancel(CancellingInvoiceMaster deleteInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceDeleteController.MultipleInvoiceDelete(CancellingInvoiceMaster)'
        {
            var response = new ApiResult();
            try
            {
                //response.Code = this.business.MultipleInvoiceCancel(deleteInfo);
                response.Code = this.jobReplaceInvoiceBusiness.MultipleInvoiceCancel(deleteInfo);
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
        [Route("adjustedMultipleInvoice")]
        [AllowAnonymous]
        public IHttpActionResult MultipleInvoiceAdjusted(CancellingInvoiceMaster deleteInfo)
        {
            var response = new ApiResult();
            try
            {
                //response.Code = this.business.MultipleInvoiceCancel(deleteInfo);
                response.Code = this.jobReplaceInvoiceBusiness.MultipleInvoiceAdjusted(deleteInfo);
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
        [Route("uploadInvoiceCancel")]
        public IHttpActionResult UploadData(UploadInvoice fileImport)
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultImportSheet>();
            try
            {

                var resultImport = this.jobReplaceInvoiceBusiness.ImportData(fileImport);
                response.Message = resultImport.Message;
                response.Code = resultImport.ErrorCode;
                //response.Data = resultImport;
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
        [Route("uploadInvoiceAdjustImport")]
        public IHttpActionResult UploadDataInvoiceAdjusted(UploadInvoice fileImport)
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultImportSheet>();
            try
            {

                var resultImport = this.jobReplaceInvoiceBusiness.ImportDataInvoiceAdjust(fileImport);
                response.Message = resultImport.Message;
                response.Code = resultImport.ErrorCode;
                //response.Data = resultImport;
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



        #region API methods
        #endregion
    }
}
