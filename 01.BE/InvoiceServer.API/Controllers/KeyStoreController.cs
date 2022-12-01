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
    [RoutePrefix("keystores")]
    public class KeyStoreController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly KeyStoreBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreController.KeyStoreController()'
        public KeyStoreController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreController.KeyStoreController()'
        {
            business = new KeyStoreBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("company/{companyId}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceApproved_Sign + ", " + UserPermission.BundleRelease_Sign
            + ", " + UserPermission.InvoiceManagement_Sign)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreController.GetList(long)'
        public IHttpActionResult GetList(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreController.GetList(long)'
        {
            var response = new ApiResultList<KeyStoreOfCompany>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetList(companyId);
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
        [Route("company")]
        [CustomAuthorize(Roles = UserPermission.InvoiceApproved_Sign + ", " + UserPermission.BundleRelease_Sign
            + ", " + UserPermission.InvoiceManagement_Sign)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreController.Create(KeyStoreOfCompany)'
        public IHttpActionResult Create(KeyStoreOfCompany keyStore)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreController.Create(KeyStoreOfCompany)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(keyStore);
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
        [Route("add-signature")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreController.AddSignature(KeyStoreOfCompany)'
        public IHttpActionResult AddSignature(KeyStoreOfCompany keyStore)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'KeyStoreController.AddSignature(KeyStoreOfCompany)'
        {
            return Create(keyStore);
        }
        #endregion API methods
    }
}