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
    [RoutePrefix("summary")]
    public class InvoiceSummaryController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoiceSummaryBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryController.InvoiceSummaryController()'
        public InvoiceSummaryController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryController.InvoiceSummaryController()'
        {
            business = new InvoiceSummaryBusiness(GetBOFactory());
        }

        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryController.GetSummaryInvoice()'
        public IHttpActionResult GetSummaryInvoice()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryController.GetSummaryInvoice()'
        {
            var response = new ApiResult<SummaryInvoice>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetSummaryInvoice();
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
        [Route("ExpireDate")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
        public IHttpActionResult GetExpireDateToken()
        {
            var response = new ApiResult<string>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetExpireDateToken();
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