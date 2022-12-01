using InvoiceServer.API.Business;
using InvoiceServer.API.Constants;
using InvoiceServer.Business.Constants;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("payment-invoice")]
    public class PaymentInvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly PaymentInvoiceBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

        public PaymentInvoiceController()
        {
            business = new PaymentInvoiceBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{invoiceId}")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
        public IHttpActionResult GetList(int invoiceId)
        {
            var response = new ApiResult<PaymentInvoiceMaster>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetPayment(invoiceId);
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
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
        public IHttpActionResult Create(PaymentInvoiceInfo paymentInfo)
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(paymentInfo);
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