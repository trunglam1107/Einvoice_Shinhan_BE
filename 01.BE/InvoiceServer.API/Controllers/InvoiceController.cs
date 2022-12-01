using InvoiceServer.API.Business;
using InvoiceServer.API.Utils;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("invoices")]
    public class InvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoiceBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.InvoiceController()'
        public InvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.InvoiceController()'
        {
            business = new InvoiceBusiness(GetBOFactory());// thêm para true để by pass
                                                           // createFilesTask = new CreateFilesTask(GetBOFactory());


        }
        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{code?}/{symbol?}/{invoiceStatus?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{paymentStatus?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{refNumber?}/{teller?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{announcementSearch?}/{importType?}/{taxVat?}/{BranchID?}/{Count?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCheckout_Read)]
        public IHttpActionResult Fillter(int? code = null, string symbol = null, int? invoiceStatus = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceTypeId = null, bool? paymentStatus = null, string numberAccout = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string refNumber = null, string teller = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, bool announcementSearch = false,
            long? importType = 0, string taxVat = null, string BranchID = null, long? Count = 0)
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                var condition = new ConditionSearchInvoice(orderBy, orderType, invoiceStatus)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    InvoiceSample = invoiceTypeId,
                    PaymentStatus = paymentStatus,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    CustomerCode = customerCode,
                    RefNumber = refNumber,
                    Teller = teller,
                    AnnouncementSearch = announcementSearch,
                    ImportType = importType,
                    TaxVAT = taxVat,
                    BranchID = BranchID,
                    TotalRecords = Count ?? 0
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterSP(condition); //business.Fillter(condition);
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
        [Route("get-new-invoice/{code?}/{symbol?}/{invoiceStatus?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{paymentStatus?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{refNumber?}/{teller?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{announcementSearch?}/{importType?}/{taxVat?}/{BranchID?}/{Count?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCheckout_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterNewInvoice(int?, string, int?, string, string, string, long?, bool?, string, string, string, long?, string, string, string, string, string, int, int, bool, long?, string, string, long?)'
        public IHttpActionResult FillterNewInvoice(int? code = null, string symbol = null, int? invoiceStatus = null, string invoiceNo = null, string customerName = null, string taxCode = null,
            long? invoiceTypeId = null, bool? paymentStatus = null, string numberAccout = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null,
            string refNumber = null, string teller = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, bool announcementSearch = false,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterNewInvoice(int?, string, int?, string, string, string, long?, bool?, string, string, string, long?, string, string, string, string, string, int, int, bool, long?, string, string, long?)'
            long? importType = 0, string taxVat = null, string BranchID = null, long? Count = 0)
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                var condition = new ConditionSearchInvoice(orderBy, orderType, invoiceStatus)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    InvoiceSample = invoiceTypeId,
                    PaymentStatus = paymentStatus,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    CustomerCode = customerCode,
                    RefNumber = refNumber,
                    Teller = teller,
                    AnnouncementSearch = announcementSearch,
                    ImportType = importType,
                    TaxVAT = taxVat,
                    BranchID = BranchID,
                    TotalRecords = Count ?? 0
                };
                response.Code = ResultCode.NoError;
                response.Data = business.Fillter(condition);
                //response.Data = business.FilterSP_Replace(condition);
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


        // Get list invoiceNo để ký khi chọn option ký ngay sau khi duyệt
        [HttpPost]
        [Route("get-list-invoice-no")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCheckout_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetListInvoiceNo(List<ReleaseInvoiceInfo>)'
        public IHttpActionResult GetListInvoiceNo(List<ReleaseInvoiceInfo> invoices)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetListInvoiceNo(List<ReleaseInvoiceInfo>)'
        {
            var response = new ApiResultList<ReleaseInvoiceInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetListInvoiceNo(invoices);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
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
        [Route("of-client/{dateFrom?}/{dateTo?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceOfClient(string, string, int, int)'
        public IHttpActionResult FillterInvoiceOfClient(string dateFrom = null, string dateTo = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceOfClient(string, string, int, int)'
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {

                long totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.FillterInvoiceOfClient(out totalRecords, dateFrom, dateTo, skip, take);
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
        [Route("cancel/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceSampleId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceCancel_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceCancel(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int)'
        public IHttpActionResult FillterInvoiceCancel(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceSampleId = null, string numberAccount = null, string dateFrom = null, string dateTo = null, long? branch = 0, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceCancel(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int)'
        {
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
        [Route("created/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{teller?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCreated_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceCreated(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int, string)'
        public IHttpActionResult FillterInvoiceCreated(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceTypeId = null,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceCreated(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int, string)'
            string numberAccout = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, string teller = null)
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                List<int> invoiceStatus = new List<int>() { (int)InvoiceStatus.New };
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
                    Branch = branch,
                    CustomerCode = customerCode,
                    Teller = teller
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterCreated(condition, 1);
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
        [Route("approved/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{teller?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceApproved_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceApproved(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int, string)'
        public IHttpActionResult FillterInvoiceApproved(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceTypeId = null, string numberAccout = null,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceApproved(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int, string)'
            string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, string teller = null)
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                List<int> invoiceStatus = new List<int>() { (int)InvoiceStatus.Approved, (int)InvoiceStatus.Releaseding };
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
                    Branch = branch,
                    CustomerCode = customerCode,
                    Teller = teller
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterCreated(condition, 0);
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
        [Route("cancelled/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.CanceledInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceCancelled(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int)'
        public IHttpActionResult FillterInvoiceCancelled(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceTypeId = null,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceCancelled(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int)'
            string numberAccount = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                List<int> invoiceStatus = new List<int>() { (int)InvoiceStatus.Cancel };
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
                    Branch = branch,
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
        [Route("deleted/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.CanceledInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceDeleted(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int)'
        public IHttpActionResult FillterInvoiceDeleted(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceTypeId = null,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceDeleted(int?, string, string, string, string, long?, string, string, string, long?, string, string, string, int, int)'
            string numberAccount = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
        {
            var response = new ApiResultList<InvoiceMaster>();
            try
            {
                List<int> invoiceStatus = new List<int>() { (int)InvoiceStatus.Delete };
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
                    Branch = branch,
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
        [Route("converted/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{converted?}/{numberAccount?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.ReportInvoiceConverted_Read + ", " + UserPermission.ConvertedInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceConverted(int?, string, string, string, string, bool?, string, string, string, long?, string, string, string, int, int)'
        public IHttpActionResult FillterInvoiceConverted(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, bool? converted = null, string numberAccount = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FillterInvoiceConverted(int?, string, string, string, string, bool?, string, string, string, long?, string, string, string, int, int)'
        {
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
                    Converted = converted,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    CustomerCode = customerCode,
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterInvoiceConverted(condition);
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
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("filter-invoice-symbol/{templateId}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.CreateInvoice_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FilterInvoiceSymbol(long)'
        public IHttpActionResult FilterInvoiceSymbol(long templateId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.FilterInvoiceSymbol(long)'
        {
            var response = new ApiResultList<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.FilterInvoiceSymbol(templateId);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.SubstituteInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetInvoiceInfo(long)'
        public IHttpActionResult GetInvoiceInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetInvoiceInfo(long)'
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
        [Route("preview/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.AdjustedInvoice_Read
            + ", " + UserPermission.InvoiceReplacement_Read + ", " + UserPermission.CanceledInvoice_Read
            + ", " + UserPermission.ConvertedInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.PreviewInvoiceData(long)'
        public IHttpActionResult PreviewInvoiceData(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.PreviewInvoiceData(long)'
        {

            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult<InvoicePrintInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetInvoiceData(id);
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
        [Route("invoice-Of-client/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetInvoiceOfClient(long)'
        public IHttpActionResult GetInvoiceOfClient(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetInvoiceOfClient(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceClientSign>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetInvoiceOfClient(id);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.RegistInvoiceNo(long, string)'
        public IHttpActionResult RegistInvoiceNo(long templateId, string symbol)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.RegistInvoiceNo(long, string)'
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
        [CustomAuthorize(Roles = UserPermission.CreateInvoice_Create + ", " + UserPermission.InvoiceManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.Create(InvoiceInfo)'
        public IHttpActionResult Create(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.Create(InvoiceInfo)'
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
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.Update(long, InvoiceInfo)'
        public IHttpActionResult Update(long id, InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.Update(long, InvoiceInfo)'
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
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Delete)]
        public IHttpActionResult Delete(long id, long companyId)
        {
            logger.Error("Delete invoice " + id.ToString() + "CompanyId now : " + companyId.ToString(), new Exception("Function Delete invoice"));
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
        [CustomAuthorize(Roles = UserPermission.InvoiceCreated_Approve)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UpdateStatus(long, InvoiceReleasesStatus)'
        public IHttpActionResult UpdateStatus(long id, InvoiceReleasesStatus status)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UpdateStatus(long, InvoiceReleasesStatus)'
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
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Update + ", " + UserPermission.SubstituteInvoice_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UpdateNote(long, InvoiceNote)'
        public IHttpActionResult UpdateNote(long id, InvoiceNote noteInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UpdateNote(long, InvoiceNote)'
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

        [HttpPost]
        [Route("approved")]
        [CustomAuthorize(Roles = UserPermission.BundleRelease_Sign + ", " + UserPermission.InvoiceManagement_Approve
             + ", " + UserPermission.InvoiceCreated_Approve)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ApproveInvoice(ApprovedInvoice)'
        public IHttpActionResult ApproveInvoice(ApprovedInvoice invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ApproveInvoice(ApprovedInvoice)'
        {
            var response = new ApiResult<ResultApproved>();

            try
            {
                var resultImport = this.business.ApproveInvoice(invoiceInfo);
                response.Message = resultImport.Message;
                response.Code = resultImport.ErrorCode;
                response.Data = resultImport;
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
        [Route("checkPermission")]
        [CustomAuthorize(Roles = UserPermission.BundleRelease_Sign + ", " + UserPermission.InvoiceManagement_Approve
         + ", " + UserPermission.InvoiceCreated_Approve)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.CheckPermission(ApprovedInvoice)'
        public Boolean CheckPermission(ApprovedInvoice invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.CheckPermission(ApprovedInvoice)'
        {
            if (invoiceInfo.ReleaseInvoiceInfos.Count == 0)
            {
                return false;
            }
            var result = this.business.CheckPermission(invoiceInfo);
            return result;
        }

        [HttpPost]
        [Route("revert-approved")]
        [CustomAuthorize(Roles = UserPermission.BundleRelease_Sign + ", " + UserPermission.InvoiceManagement_Approve
             + ", " + UserPermission.InvoiceCreated_Approve)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.RevertApprovedInvoice(ApprovedInvoice)'
        public IHttpActionResult RevertApprovedInvoice(ApprovedInvoice approvedInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.RevertApprovedInvoice(ApprovedInvoice)'
        {
            var response = new ApiResult<ResultApproved>();

            try
            {
                var resultImport = this.business.RevertApprovedInvoice(approvedInfo);
                response.Message = resultImport.Message;
                response.Code = resultImport.ErrorCode;
                response.Data = resultImport;
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
        [Route("print-view/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.PreviewInvoice(long)'
        public IHttpActionResult PreviewInvoice(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.PreviewInvoice(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.PrintView(id);
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
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message); // + 
            }
        }


        [HttpGet]
        [Route("download-xmlfile/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetXmlFile(long)'
        public IHttpActionResult GetXmlFile(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetXmlFile(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.GetXmlFile(id);
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
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message); // + 
            }
        }
        [HttpGet]
        [Route("downloadStatistical/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DownloadStatistical(long)'
        public IHttpActionResult DownloadStatistical(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DownloadStatistical(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.DownloadStatistical(id);
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
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message); // + 
            }
        }
        [HttpGet]
        [Route("downloadInvoiceGift/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DownloadInvoiceGift(long)'
        public IHttpActionResult DownloadInvoiceGift(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DownloadInvoiceGift(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.DownloadGift(id);
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
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message); // + 
            }
        }
        [HttpPost]
        [Route("upload")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UploadData(UploadInvoice)'
        public IHttpActionResult UploadData(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UploadData(UploadInvoice)'
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultImportSheet>();

            try
            {

                var resultImport = this.business.ImportData(fileImport);
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
        [Route("uploadInvoiceReplace")]
        [AllowAnonymous]
        public IHttpActionResult UploadDataInvoiceReplace(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UploadData(UploadInvoice)'
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultImportSheet>();

            try
            {

                var resultImport = this.business.ImportDataInvoiceReplace(fileImport);
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
        [Route("uploadInvoiceAdjustForInvAdjusted")]
        [AllowAnonymous]
        public IHttpActionResult UploadDataInvoiceAdjustForInvAdjusted(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UploadData(UploadInvoice)'
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultImportSheet>();

            try
            {

                var resultImport = this.business.ImportDataInvoiceAdjustForInvAdjusted(fileImport);
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

        [HttpGet]
        [Route("download-xml/{id?}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceApproved_Sign
            + ", " + UserPermission.BundleRelease_Sign)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DownloadInvoiceXML(long)'
        public IHttpActionResult DownloadInvoiceXML(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DownloadInvoiceXML(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.DownloadInvoiceXml(id);
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
        [Route("release-info/{id?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.AdjustedInvoice_Read
            + ", " + UserPermission.InvoiceReplacement_Read + ", " + UserPermission.CanceledInvoice_Read
            + ", " + UserPermission.ConvertedInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetInvoiceReleaseInfo(long)'
        public IHttpActionResult GetInvoiceReleaseInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetInvoiceReleaseInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceReleaseInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetInvoiceReleaseInfo(id);
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
        [Route("send-verificationCode")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.ConvertedInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.SendVerificationCode(InvoiceVerificationCode)'
        public IHttpActionResult SendVerificationCode(InvoiceVerificationCode invoiceVerificationCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.SendVerificationCode(InvoiceVerificationCode)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.SendVerificationCode(invoiceVerificationCode);
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
        [Route("switch/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.SwitchInvoice(long)'
        public IHttpActionResult SwitchInvoice(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.SwitchInvoice(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.SwitchInvoice(id);
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
                return Error(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        [Route("date-can-use/{templateId?}/{symbol?}/{dateOfInvoice?}/{branch?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.CreateInvoice_Create
            + ", " + UserPermission.InvoiceAdjustment_Create + ", " + UserPermission.SubstituteInvoice_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DateCanUse(long, long, string, string)'
        public IHttpActionResult DateCanUse(long templateId, long branch, string symbol = null, string dateOfInvoice = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DateCanUse(long, long, string, string)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultValidDate>();

            try
            {
                var invoiceDateOfInfo = new InvoiceDateInfo(dateOfInvoice, templateId, symbol, branch);
                response.Code = ResultCode.NoError;
                response.Data = this.business.DataInvoiceCanUse(invoiceDateOfInfo);
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
        [Route("date-can-use-update/{templateId?}/{symbol?}/{dateOfInvoice?}/{no?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DateCanUseEdit(long, string, string, string)'
        public IHttpActionResult DateCanUseEdit(long templateId, string symbol = null, string dateOfInvoice = null, string no = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DateCanUseEdit(long, string, string, string)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultValidDate>();

            try
            {
                var invoiceDateOfInfo = new InvoiceDateInfo(dateOfInvoice, templateId, symbol, no);
                response.Code = ResultCode.NoError;
                response.Data = this.business.DataInvoiceCanUse(invoiceDateOfInfo, false);
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
        [Route("date-can-use-branch/{templateId?}/{symbol?}/{dateOfInvoice?}/{branch?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.CreateInvoice_Create
            + ", " + UserPermission.InvoiceAdjustment_Create + ", " + UserPermission.SubstituteInvoice_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DateCanUseBranch(long, string, string, long?)'
        public IHttpActionResult DateCanUseBranch(long templateId, string symbol = null, string dateOfInvoice = null, long? branch = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DateCanUseBranch(long, string, string, long?)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultValidDate>();

            try
            {
                var invoiceDateOfInfo = new InvoiceDateInfo(dateOfInvoice, templateId, symbol?.Trim(), (long)branch);
                response.Code = ResultCode.NoError;
                response.Data = this.business.DataInvoiceCanUse(invoiceDateOfInfo);
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
        [Route("research/{verificationCode?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ResearchInvoice(string)'
        public IHttpActionResult ResearchInvoice(string verificationCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ResearchInvoice(string)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ResearchInvoice(verificationCode);
                if (fileInfo == null || !File.Exists(fileInfo.FullPathFileName))
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
        [Route("download-receipt/{id?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DownloadFile(long)'
        public IHttpActionResult DownloadFile(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.DownloadFile(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.GetFileRecipt(id);
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
        [Route("converted/download/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{converted?}/{numberAccount?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{language?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.ConvertedInvoice_Read
            + ", " + UserPermission.ReportInvoiceConverted_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ExportInvoiceConverted(int?, string, string, string, string, bool?, string, string, string, long?, string, string, string, int, int, string)'
        public IHttpActionResult ExportInvoiceConverted(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, bool? converted = null, string numberAccount = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ExportInvoiceConverted(int?, string, string, string, string, bool?, string, string, string, long?, string, string, string, int, int, string)'
        {
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
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    CustomerCode = customerCode,
                    Converted = true,
                    Language = language,
                };
                var fileInfo = business.ExportInvoiceConverted(condition);

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

        // trả về ngày hóa đơn lớn nhất đã tạo ra
        [HttpPost]
        [Route("max-release-date")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.MaxReleaseDate(InvoiceInfo)'
        public IHttpActionResult MaxReleaseDate(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.MaxReleaseDate(InvoiceInfo)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceReleaseDate>();

            try
            {
                response.Data = this.business.MaxReleaseDate(invoiceInfo);
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

        // trả về số hóa đơn lớn nhất đã tạo ra
        [HttpPost]
        [Route("min-invoice-no")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.MinInvoiceNo(InvoiceInfo)'
        public IHttpActionResult MinInvoiceNo(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.MinInvoiceNo(InvoiceInfo)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceNo>();

            try
            {
                response.Data = this.business.MinInvoiceNo(invoiceInfo);
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

        // trả về số hóa đơn lớn nhất đã đăng ký (số lớn nhất được phép tạo)
        [HttpPost]
        [Route("max-invoice-no")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.MaxInvoiceNo(InvoiceInfo)'
        public IHttpActionResult MaxInvoiceNo(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.MaxInvoiceNo(InvoiceInfo)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceNo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Data = this.business.MaxInvoiceNo(invoiceInfo);
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
        [Route("readGiftFile")]
        //[CustomAuthorize(Roles = UserPermission.UploadInvoice_Create)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ReadGiftFile(UploadInvoice)'
        public IHttpActionResult ReadGiftFile(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ReadGiftFile(UploadInvoice)'
        {
            var response = new ApiResultList<InvoiceGift>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.ReadFileGift(fileImport);
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
        [Route("readDetailFile")]
        //[CustomAuthorize(Roles = UserPermission.UploadInvoice_Create)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ReadDetailFile(UploadInvoice)'
        public IHttpActionResult ReadDetailFile(UploadInvoice fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.ReadDetailFile(UploadInvoice)'
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.FileUploadNotTheSameType);
            }

            var response = new ApiResultList<InvoiceDetailInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.ReadDetailFile(fileImport);
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
        [Route("getListStatiscal/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetListStatiscal(long)'
        public IHttpActionResult GetListStatiscal(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetListStatiscal(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResultList<InvoiceStatistical>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetListStatisticals(id);
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
        [Route("getListGift/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetListGift(long)'
        public IHttpActionResult GetListGift(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetListGift(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResultList<InvoiceGift>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetListGift(id);
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
        [Route("importInvoice")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.autoImport(bool)'
        public IHttpActionResult autoImport(bool isJob = false)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.autoImport(bool)'
        {
            var response = new ApiResult<ResultImportSheet>();

            try
            {
                // if run by QuartJob isJob(true), run by hand isJob(false)
                response.Code = this.business.importInvoice(isJob);
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
        [Route("import-external-api")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Create + ", " + UserPermission.CreateInvoice_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.BIDCImportExternalApi(BIDCImportFromAPI)'
        public IHttpActionResult BIDCImportExternalApi(BIDCImportFromAPI bIDCImportFromAPI)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.BIDCImportExternalApi(BIDCImportFromAPI)'
        {
            var response = new ApiResult<BIDCImportFromAPIOutput>();
            if (!CustomerIndex.IsBIDC)
            {
                return Ok(response);
            }

            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.BIDCImportInvoiceExternalApi(bIDCImportFromAPI);
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
        [Route("export-excel/{code?}/{symbol?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceSampleId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{invoiceStatus?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
        public IHttpActionResult ExportExcel(int? code = null, string symbol = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceTypeId = null, bool? paymentStatus = null, string numberAccout = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string refNumber = null, string teller = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, int? invoiceStatus = null, string language = null)
        {
            try
            {
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
                    Branch = branch,
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

        [HttpGet]
        [Route("export-excel-Invoices/{code?}/{symbol?}/{invoiceStatus?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{paymentStatus?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{customerCode?}/{refNumber?}/{teller?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{BranchID?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
        public IHttpActionResult ExportExcelInvoices(int? code = null, string symbol = null, int? invoiceStatus = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceTypeId = null, bool? paymentStatus = null, string dateFrom = null, string dateTo = null, long? branch = null, string customerCode = null, string refNumber = null, string teller = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, string BranchID = null)
        {
            try
            {

                var condition = new ConditionSearchInvoice(orderBy, orderType, invoiceStatus)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    InvoiceSample = invoiceTypeId,
                    PaymentStatus = paymentStatus,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch,
                    CustomerCode = customerCode,
                    RefNumber = refNumber,
                    Teller = teller,
                    BranchID = BranchID
                };
                ExportFileInfo fileInfo = this.business.ExportExcelInvoices(condition);
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

        [HttpGet]
        [Route("get-check-invoice-number-lst/{branch?}/{BranchId?}/{dateFrom?}/{dateTo?}/{resultType?}/{resultValue?}/{orderBy?}/{orderType?}/{invSymbol?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCheckout_Read)]
        public IHttpActionResult GetCheckInvoiceNumberList(long? branch = null, string BranchID = null, string dateFrom = null, string dateTo = null, 
           string resultType = null, string resultValue = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, string invSymbol= null)
        {
            var response = new ApiResultList<InvoiceCheckDaily>();
            try
            {
                var condition = new ConditionSearchCheckNumberInvoice(orderBy, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Branch = branch,
                    BranchID = BranchID,
                    DateFrom = dateFrom.ConvertDateTime(),
                    DateTo = dateTo.ConvertDateTime(),
                    ResultType = resultType,
                    ResultValue = resultValue,
                    Symbols = invSymbol
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FilterInvoiceCheckDaily(condition); //business.Fillter(condition);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
                     {CustomHttpRequestHeader.CollectionTotal, condition.TotalRecords.ToString() }, //condition.TotalRecords.ToString()},
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
        [Route("export-excel-check-invoice-number/{branch?}/{BranchId?}/{dateFrom?}/{dateTo?}/{resultType?}/{resultValue?}/{orderBy?}/{orderType?}/{invSymbol?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
        public IHttpActionResult ExportExcelInvoicesCheckNumber(long? branch = null, string BranchID = null, 
            string dateFrom = null, string dateTo = null, string resultType = null, string resultValue = null, 
            string orderBy = null, string orderType = null, string invSymbol = null)
        {
            try
            {
                var condition = new ConditionSearchCheckNumberInvoice(orderBy, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Branch = branch,
                    BranchID = BranchID,
                    DateFrom = dateFrom.ConvertDateTime(),
                    DateTo = dateTo.ConvertDateTime(),
                    ResultType = resultType,
                    ResultValue = resultValue,
                    Symbols = invSymbol
                };
                ExportFileInfo fileInfo = this.business.ExportExcelCkInvoiceNumber(condition);
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
            catch(Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message);
            }
        }

        #endregion API methods

        #region Job
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UpdateProcessJob(string, bool)'
        public void UpdateProcessJob(string name, bool processing)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.UpdateProcessJob(string, bool)'
        {
            this.business.UpdateProcessJob(name, processing);
        }
        public QUARZTJOB FindByJobName(string name)
        {
            return this.business.findByName(name);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.JobApprove()'
        public void JobApprove()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.JobApprove()'
        {
            try
            {
                this.business.JobApproveShinhan();
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
        }
        public void JobServerSign()
        {
            try
            {
                this.business.JobCreateFile();
                //this.business.JobSignShinhan();
            }
            catch (Exception ex)
            {
                logger.Error("jobAutoSign", ex);
            }
        }
        public void JobSignPersonal()
        {
            try
            {
                this.business.SignFilePersonal();
            }
            catch (Exception ex)
            {
                logger.Error("SignPersonal", ex);
            }
        }
        public void JobSendMails()
        {
            try
            {
                this.business.JobSendMail();
            }
            catch (Exception ex)
            {
                logger.Error("jobAutoSendMails", ex);
            }
        }
        [Route("test")]
        [HttpGet]
        public IHttpActionResult Test()
        {
            business.JobCreateFile();

            return Ok(string.Empty);
        }
        public void JobCreateFile()
        {
            try
            {
                this.business.JobCreateFile();
            }
            catch (Exception ex)
            {
                logger.Error("jobAutoCreateFile", ex);
            }
        }

        [HttpGet]
        [Route("get-by-invoice-no/{registerTemplateId?}/{symbol?}/{invoiceNo?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCheckout_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetByInvoiceNo(int?, string, string)'
        public IHttpActionResult GetByInvoiceNo(int? registerTemplateId = null, string symbol = null, string invoiceNo = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceController.GetByInvoiceNo(int?, string, string)'
        {
            var response = new ApiResult<InvoiceInfo>();
            try
            {
                var condition = new ConditionSearchInvoice()
                {
                    CurrentUser = this.CurrentUser,
                    InvoiceSampleId = registerTemplateId,
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Data = business.GetByInvoiceNo(condition);
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

        #endregion
    }
}