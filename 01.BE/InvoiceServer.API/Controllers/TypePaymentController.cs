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
    [RoutePrefix("type-payments")]
    public class TypePaymentController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly TypePaymentBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentController.TypePaymentController()'
        public TypePaymentController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentController.TypePaymentController()'
        {
            business = new TypePaymentBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentController.GetRole()'
        public IHttpActionResult GetRole()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TypePaymentController.GetRole()'
        {
            var response = new ApiResultList<TypePaymentInfo>();
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
        #endregion API methods
    }
}