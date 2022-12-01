using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("signature")]
    public class SignatureController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly SignatureBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.SignatureController()'
        public SignatureController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.SignatureController()'
        {
            business = new SignatureBusiness(GetBOFactory());
        }
        #endregion Constructor

        #region API Method
        [HttpGet]
        [Route("{taxCode?}/{branch?}")]
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Read + ", " + UserPermission.CertificateAuthorityManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.FillterCA(string, long?, int, int)'
        public IHttpActionResult FillterCA(string taxCode = null, long? branch = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.FillterCA(string, long?, int, int)'
        {
            var response = new ApiResultList<SignatureInfo>();
            try
            {
                long totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.FillterCA(out totalRecords, taxCode, branch, skip, take);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
                     {CustomHttpRequestHeader.CollectionTotal, totalRecords.ToString()},
                     {CustomHttpRequestHeader.CollectionSkip, skip.ToString()},
                     {CustomHttpRequestHeader.CollectionTake, take.ToString()},
                };

                SetResponseHeaders(responHeaders);
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
        [Route("add-or-edit")]
        [CustomAuthorize(Roles = UserPermission.DivisionInformation_Update + ", " + UserPermission.CertificateAuthorityManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.CreateOrUpdate(KeyStoreOfCompany)'
        public IHttpActionResult CreateOrUpdate(KeyStoreOfCompany keyStore)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.CreateOrUpdate(KeyStoreOfCompany)'
        {
            var response = new ApiResult();
            var signatureInfo = new SignatureInfo()
            {
                CompanyId = keyStore.CompanyId,
                SerialNumber = keyStore.SerialNumber,
                CompanyName = keyStore.CompanyName,
                FromDate = keyStore.FromDate,
                ToDate = keyStore.ToDate,
            };

            try
            {
                response.Code = this.business.CreateOrUpdate(signatureInfo);
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
        [CustomAuthorize(Roles = UserPermission.CertificateAuthorityManagement_Create)]
        //[AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.Upload(SignatureInfo)'
        public IHttpActionResult Upload(SignatureInfo infoCA)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.Upload(SignatureInfo)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.CreateOrUpdate(infoCA);
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.CertificateAuthorityManagement_Read)]
        //[AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetSignatureInfo(long)'
        public IHttpActionResult GetSignatureInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetSignatureInfo(long)'
        {
            var response = new ApiResult<SignatureInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = this.business.GetSignatureInfo(id);
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
        [Route("getSerial")]
        [CustomAuthorize(Roles = UserPermission.CertificateAuthorityManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetSerial(SignatureInfo)'
        public IHttpActionResult GetSerial(SignatureInfo info)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetSerial(SignatureInfo)'
        {
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = this.business.GetSerial(info);
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
        [Route("getTypeSign")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetTypeSign()'
        public bool GetTypeSign()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetTypeSign()'
        {
            bool result = false;
            try
            {
                result = this.business.GetTypeSign();
            }
            catch (BusinessLogicException ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return result;
        }
        [HttpDelete]
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.CertificateAuthorityManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.Delete(long)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.Delete(id);
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
        [Route("get-all")]
        [CustomAuthorize(Roles = UserPermission.InvoiceApproved_Sign + ", " + UserPermission.BundleRelease_Sign
            + ", " + UserPermission.InvoiceManagement_Sign + ", " + UserPermission.CertificateAuthorityManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetList()'
        public IHttpActionResult GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetList()'
        {
            var response = new ApiResultList<SignatureInfo>();
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
        [HttpPost]
        [Route("getSerialBySlot")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetSerialBySlot(SignatureInfo)'
        public IHttpActionResult GetSerialBySlot(SignatureInfo signatureInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetSerialBySlot(SignatureInfo)'
        {
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetSerialBySlot(signatureInfo.Slot ?? 0, signatureInfo.Password);
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
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }
        [HttpPost]
        [Route("getSlots")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetSlots(SignatureInfo)'
        public IHttpActionResult GetSlots(SignatureInfo signatureInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.GetSlots(SignatureInfo)'
        {
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetSlots(signatureInfo.Password);
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
                response.Message = ex.Message;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }
        [HttpGet]
        [Route("checkSlotCert/{slot?}/{cert?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.CheckSlotCert(int, string)'
        public bool CheckSlotCert(int slot, string cert)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.CheckSlotCert(int, string)'
        {
            bool result = false;
            try
            {
                result = this.business.GetBySlot(slot, cert);
            }
            catch (BusinessLogicException ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return result;
        }

        [HttpGet]
        [Route("check-branch/{branch?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.CheckCompany(long)'
        public bool CheckCompany(long branch)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SignatureController.CheckCompany(long)'
        {
            bool result = false;
            try
            {
                result = this.business.GetByCompanyId(branch);
            }
            catch (BusinessLogicException ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return result;
        }

        [HttpGet]
        [Route("download/{taxCode?}/{branch?}")]
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Read + ", " + UserPermission.CertificateAuthorityManagement_Read)]
        [AllowAnonymous]
        public IHttpActionResult DownloadData(string taxCode = null, long? branch = null)
        {
            try
            {
                var condition = new ConditionSearchCA(taxCode, branch);
                ExportFileInfo fileInfo = this.business.DownloadDataSignature(condition);
                if (!File.Exists(fileInfo.FullPathFileName))
                {
                    return NotFound();
                }

                InvoiceFile file = new InvoiceFile()
                {
                    FilePath = fileInfo.FullPathFileName,
                    FileName = fileInfo.FileName,
                };

                SetResponseHeaders("Content-Disposition", "inline; filename=" + fileInfo.FileName);
                return Ok(file);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file");
            }
        }

        #endregion API methods
    }
}