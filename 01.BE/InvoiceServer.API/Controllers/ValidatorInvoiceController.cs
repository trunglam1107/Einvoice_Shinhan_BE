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
    [RoutePrefix("validator-invoice")]
    public class ValidatorInvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ValidatorInvoiceBusiness business;

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceController.ValidatorInvoiceController()'
        public ValidatorInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceController.ValidatorInvoiceController()'
        {
            business = new ValidatorInvoiceBusiness(GetBOFactory());
        }

        #endregion

        [HttpPost]
        [Route("upload")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceController.UploadInvoide(FileImport)'
        public IHttpActionResult UploadInvoide(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ValidatorInvoiceController.UploadInvoide(FileImport)'
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ValidatorResult>();

            try
            {
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Code = ResultCode.NoError;
                response.Data = this.business.ImportData(fileImport);
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
    }
}