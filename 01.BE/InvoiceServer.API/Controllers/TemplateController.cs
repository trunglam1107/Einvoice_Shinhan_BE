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
    [RoutePrefix("templates")]
    public class TemplateController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly TemplateBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateController.TemplateController()'
        public TemplateController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateController.TemplateController()'
        {
            business = new TemplateBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateController.GetList()'
        public IHttpActionResult GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateController.GetList()'
        {
            var response = new ApiResultList<InvoiceTemplateInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetList();
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
        [Route("typeInvoice/{typeInvoiceId}")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateController.GetByInvoiceSample(long)'
        public IHttpActionResult GetByInvoiceSample(long typeInvoiceId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateController.GetByInvoiceSample(long)'
        {
            var response = new ApiResultList<InvoiceTemplateInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.FilterByInvoiceSample(typeInvoiceId);
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

        //Task #20870: bỏ field InvoiceSampleCode trong table InvoiceTemplate
        [HttpGet]
        [Route("getTemplateId/{invoiceTypeCode?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateController.GetInvoiceTemplateId(string)'
        public IHttpActionResult GetInvoiceTemplateId(string invoiceTypeCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateController.GetInvoiceTemplateId(string)'
        {
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetByInvoiceSampleCode(invoiceTypeCode).ToString();
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