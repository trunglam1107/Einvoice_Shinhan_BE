using InvoiceServer.API.Business;
using InvoiceServer.API.Business.GateWay;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using InvoiceServer.GateWay.Models.MVan;
using InvoiceServer.Common.Extensions;

namespace InvoiceServer.API.Controllers
{

    [RoutePrefix("invoice-declaration")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController'
    public class DeclarationController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController'
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly DeclarationBusiness business;
        private readonly InvoiceBusiness invoiceBusiness;
        private readonly MVanBusiness businessTWan;


        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.DeclarationController()'
        public DeclarationController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.DeclarationController()'
        {
            business = new DeclarationBusiness(GetBOFactory());
            invoiceBusiness = new InvoiceBusiness(GetBOFactory());
            businessTWan = new MVanBusiness(GetServiceFactory());

        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{status?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        public IHttpActionResult Fillter(string status = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue, int companyID = 0)
        {
            var response = new ApiResultList<DeclarationInfo>();
            try
            {
                long totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.Fillter(out totalRecords, status, orderby, orderType, skip, take, companyID); 
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

        [HttpGet]
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetDeclarationInfo(long)'
        public IHttpActionResult GetDeclarationInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetDeclarationInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<DeclarationInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetDeclarationInfo(id);
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
        // add get webconfig info
        [HttpGet]
        [Route("get-config")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetConfigInfo()'
        public IHttpActionResult GetConfigInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetConfigInfo()'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<FooterConfig>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.invoiceBusiness.GetConfigInfo();
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
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.Create(DeclarationInfo)'
        public IHttpActionResult Create(DeclarationInfo DeclarationInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.Create(DeclarationInfo)'
        {
            if (!ModelState.IsValid || DeclarationInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(DeclarationInfo);
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.Update(long, DeclarationInfo)'
        public IHttpActionResult Update(long id, DeclarationInfo DeclarationInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.Update(long, DeclarationInfo)'
        {
            if (!ModelState.IsValid || DeclarationInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(id, DeclarationInfo);
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
        [Route("updateSymbol/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.UpdateSymbol(long, DeclarationInfo)'
        public IHttpActionResult UpdateSymbol(long id, DeclarationInfo DeclarationInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.UpdateSymbol(long, DeclarationInfo)'
        {
            if (!ModelState.IsValid || DeclarationInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.UpdateSymbol(id, DeclarationInfo);
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


        [HttpDelete]
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.Delete(long)'
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
        [Route("getLastDeclarationByCompanyId/{companyId}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetDeclarationInfoLast(long)'
        public IHttpActionResult GetDeclarationInfoLast(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetDeclarationInfoLast(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<DeclarationInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetDeclarationLast(companyId);
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
        [Route("change-status/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.UpdateStatus(long, int?)'
        public IHttpActionResult UpdateStatus(long id, int? status)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.UpdateStatus(long, int?)'
        {
            var response = new ApiResult();

            try
            {
                response.Code = this.business.UpdateStatus(id, status);
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
        [Route("declaration-register-type")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetRegisterType()'
        public IHttpActionResult GetRegisterType()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetRegisterType()'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResultList<RegisterType>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetRegisterTypes();
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
        [Route("DeclarationSymbol/{companyId}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetDeclarationSymbol(long?)'
        public IHttpActionResult GetDeclarationSymbol(long? companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetDeclarationSymbol(long?)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<DeclarationMaster>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetDeclarationSymbol(companyId);
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
        [Route("getXMLfile/{id}")]
        [ResponseType(typeof(InvoiceFile))]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.getXMLFile(long)'
        public IHttpActionResult getXMLFile(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.getXMLFile(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.GetFileXML(id);
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
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message); // + 
            }
        }

        [HttpGet]
        [Route("downloadxml/{id}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.DownloadXML(long)'
        public IHttpActionResult DownloadXML(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.DownloadXML(long)'
        {

            var response = new ApiResult();

            try
            {
                ExportFileInfo fileInfo = new ExportFileInfo();
                DeclarationInfo declaration = this.business.GetDeclarationInfo(id);
                if (declaration.Status == 4 || declaration.Status == 5)
                {
                    fileInfo = this.business.getFileSignXML(id);
                }
                else
                {
                    fileInfo = this.business.GetFileXML(id);
                }


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
        [Route("downloadNotixml/{id}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.DownloadNotiXML(long)'
        public IHttpActionResult DownloadNotiXML(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.DownloadNotiXML(long)'
        {

            var response = new ApiResult();

            try
            {
                ExportFileInfo fileInfo = this.business.GetFileNotiXML();
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
        [Route("getXmlString/{id}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetXmlString(long)'
        public IHttpActionResult GetXmlString(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.GetXmlString(long)'
        {
            var response = new ApiResult<string>();
            try
            {
                ExportFileInfo fileInfo = this.business.GetFileXML(id);
                string textXml = File.ReadAllText(fileInfo.FullPathFileName);
                response.Code = ResultCode.NoError;
                response.Data = textXml;
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
        [Route("sign/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Sign)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.SignDeclaration(long)'
        public IHttpActionResult SignDeclaration(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.SignDeclaration(long)'
        {

            var response = new ApiResult();

            try
            {
                response.Code = this.business.SignDeclaration(id);
                SendTWAN(id);
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
        [Route("uploadFileSign")]
        [AllowAnonymous]
        [HttpPost]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.UploadFileSign(DeclarationInfo)'
        public IHttpActionResult UploadFileSign(DeclarationInfo declarationInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.UploadFileSign(DeclarationInfo)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.UpdateAfterSign(declarationInfo);
                SendTWAN(declarationInfo.ID);

                response.Message = MsgApiResponse.ExecuteSeccessful;
            }
            catch (Exception ex)
            {
                logger.Error("signXml", ex);
            }
            return Ok(response);
        }

        [HttpGet]
        [Route("sendTwan/{id}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.SendTWAN(long)'
        public IHttpActionResult SendTWAN(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.SendTWAN(long)'
        {
            var response = new ApiResult();
            try
            {
                ExportFileInfo fileInfo = this.business.getFileSignXML(id);

                var base64File = Convert.ToBase64String(File.ReadAllBytes(fileInfo.FullPathFileName));

                VanDefaulRequest vanDefaulRequest = new VanDefaulRequest()
                {
                    DeclarationId = id,
                    XmlData = base64File
                };
                VanDefaulResult vanDefaulResult = this.businessTWan.VanRegister(vanDefaulRequest);
                response.Code = this.business.CheckSendTwan(id);
                response.Message = MsgApiResponse.ExecuteSeccessful;
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
        [Route("approve/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Approve)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.ApproveInvoiceDeclaration(long)'
        public IHttpActionResult ApproveInvoiceDeclaration(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.ApproveInvoiceDeclaration(long)'
        {
            var response = new ApiResult();

            try
            {
                var result = this.business.Approve(id);
                response.Message = result;
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
        [Route("revert-approve/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceRegister_Approve)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.RevertApproveDeclaration(long)'
        public IHttpActionResult RevertApproveDeclaration(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.RevertApproveDeclaration(long)'
        {
            var response = new ApiResult();

            try
            {
                var result = this.business.RevertApprove(id);
                response.Message = result;
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
        [Route("download-pdf/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.DownloadPDF(long)'
        public IHttpActionResult DownloadPDF(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.DownloadPDF(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ExportPdf(id);
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
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message); // + 
            }
        }


        [HttpGet]
        [Route("uploadFile/{companyId}")]
        [CustomAuthorize(Roles = UserPermission.CertificateAuthorityManagement_Create)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.UploadFileCertificate(DeclarationInfo, long)'
        public IHttpActionResult UploadFileCertificate(DeclarationInfo declarationInfo, long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'DeclarationController.UploadFileCertificate(DeclarationInfo, long)'
        {
            var response = new ApiResult<DeclarationReleaseInfo>();

            try
            {

                response.Data = this.business.UploadFile(companyId);
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
        [Route("dowloadExcelDeclare/{status?}/{companyID?}")]
        public IHttpActionResult DowloadExcelDeclare(int? status = null,int companyID = 0)
        {
            var response = new ApiResult();
            try
            {
                var condition = new ConditionSearchDeclaration(status,companyID);
                ExportFileInfo fileInfo = business.DownloadExcelDeclare(condition);


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
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }
        [HttpGet]
        [Route("symbols/{companyId?}")]
        public IHttpActionResult FillterSymbol(int companyId = 0)
        {
            var response = new ApiResultList<CompanySymbolInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.ListSymbol(companyId);
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
        [Route("dowloadSymbol/{companyId?}")]
        public IHttpActionResult DowloadSymbol(int companyId = 0)
        {
            var response = new ApiResult();
            try
            {
                ExportFileInfo fileInfo = business.DownloadExcelSymbol(companyId);


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
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }
        #endregion API methods
    }
}