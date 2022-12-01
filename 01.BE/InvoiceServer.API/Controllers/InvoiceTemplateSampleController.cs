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
    [RoutePrefix("invoice-template-sample")]
    public class InvoiceTemplateSampleController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoiceTemplateSampleBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.InvoiceTemplateSampleController()'
        public InvoiceTemplateSampleController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.InvoiceTemplateSampleController()'
        {
            business = new InvoiceTemplateSampleBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.GetList()'
        public IHttpActionResult GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.GetList()'
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
        [Route("get-by-invoice-type-id/{invoiceTypeId}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.GetListByInvoiceTypeId(long)'
        public IHttpActionResult GetListByInvoiceTypeId(long invoiceTypeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.GetListByInvoiceTypeId(long)'
        {
            var response = new ApiResultList<InvoiceTemplateInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetListByInvoiceTypeId(invoiceTypeId);
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
        [Route("get-by-invoice-type-id")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.GetListByInvoiceTypeId2()'
        public IHttpActionResult GetListByInvoiceTypeId2()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.GetListByInvoiceTypeId2()'
        {
            var response = new ApiResultList<InvoiceTemplateInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetListByInvoiceTypeId(0);
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
        [Route("template-sample-name/{templateSampleId}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.GetTemplateSampleNameById(long)'
        public IHttpActionResult GetTemplateSampleNameById(long templateSampleId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleController.GetTemplateSampleNameById(long)'
        {
            var response = new ApiResult<InvoiceTemplateSampleName>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetTemplateSampleNameById(templateSampleId);
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