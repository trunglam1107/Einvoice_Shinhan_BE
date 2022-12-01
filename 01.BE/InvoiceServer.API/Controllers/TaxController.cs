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
    [RoutePrefix("taxs")]
    public class TaxController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly TaxBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxController.TaxController()'
        public TaxController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxController.TaxController()'
        {
            business = new TaxBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TaxController.GetList()'
        public IHttpActionResult GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TaxController.GetList()'
        {
            var response = new ApiResultList<TaxInfo>();
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