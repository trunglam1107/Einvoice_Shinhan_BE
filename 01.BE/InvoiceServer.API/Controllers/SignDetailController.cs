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
    [RoutePrefix("sign-detail")]
    public class SignDetailController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly SignDetailBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignDetailController.SignDetailController()'
        public SignDetailController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignDetailController.SignDetailController()'
        {
            business = new SignDetailBusiness(GetBOFactory());
        }
        #endregion Contructor

        #region API methods
        [HttpPost]
        [Route("")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignDetailController.Create(SignDetailInfo)'
        public IHttpActionResult Create(SignDetailInfo signDetailInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignDetailController.Create(SignDetailInfo)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(signDetailInfo);
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