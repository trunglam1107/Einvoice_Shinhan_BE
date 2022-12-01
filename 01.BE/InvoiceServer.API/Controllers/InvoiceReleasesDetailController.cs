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
    [RoutePrefix("invoice-releases-detail")]
    public class InvoiceReleasesDetailController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoiceReleasesDetailBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailController.InvoiceReleasesDetailController()'
        public InvoiceReleasesDetailController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailController.InvoiceReleasesDetailController()'
        {
            business = new InvoiceReleasesDetailBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpGet]
        [Route("{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailController.GetInvoiceReleasesDetailInfo(long)'
        public IHttpActionResult GetInvoiceReleasesDetailInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesDetailController.GetInvoiceReleasesDetailInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceReleasesDetailInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetInvoiceReleasesDetailInfo(id);
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