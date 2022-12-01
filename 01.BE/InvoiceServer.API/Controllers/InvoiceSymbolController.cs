using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("invoice-symbols")]
    public class InvoiceSymbolController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly NoticeUseInvoiceDetailBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSymbolController.InvoiceSymbolController()'
        public InvoiceSymbolController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSymbolController.InvoiceSymbolController()'
        {
            business = new NoticeUseInvoiceDetailBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("invoice-template/{templateId}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCreated_Read
            + ", " + UserPermission.InvoiceApproved_Read + ", " + UserPermission.BundleRelease_Read
            + ", " + UserPermission.InvoiceCheckout_Read + ", " + UserPermission.InvoiceAdjustment_Read
            + ", " + UserPermission.SubstituteInvoice_Read + ", " + UserPermission.InvoiceCancel_Read
            + ", " + UserPermission.UploadInvoice_Create + ", " + UserPermission.AdjustedInvoice_Read
            + ", " + UserPermission.InvoiceReplacement_Read + ", " + UserPermission.CanceledInvoice_Read
            + ", " + UserPermission.ConvertedInvoice_Read + ", " + UserPermission.ReportInvoiceConverted_Read
            + ", " + UserPermission.ReportInvoiceDetail_Read + ", " + UserPermission.ReportInvoiceCombine_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSymbolController.FilterInvoiceSymbol(long)'
        public IHttpActionResult FilterInvoiceSymbol(long templateId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSymbolController.FilterInvoiceSymbol(long)'
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
        [Route("invoice-template-use/{invoiceTypeId}/{reportCancellingId}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.ReportCancelling_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSymbolController.FilterInvoiceSymbolUse(long, long)'
        public IHttpActionResult FilterInvoiceSymbolUse(long invoiceTypeId, long reportCancellingId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSymbolController.FilterInvoiceSymbolUse(long, long)'
        {
            var response = new ApiResultList<RegisterTemplateCancelling>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.FilterInvoiceSymbolUse(invoiceTypeId, reportCancellingId);
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