using InvoiceServer.API.Business;
using InvoiceServer.API.Business.GateWay;
using InvoiceServer.API.Business.Portal;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Annoucement;
using InvoiceServer.Business.Models.Portal;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.GateWay.Models.MVan;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace InvoiceServer.API.Controllers.Portal
{
    [RoutePrefix("InvoicePortal")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController'
    public class InvoicePortalController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController'
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoicePortalBusiness business;
        private readonly InvoiceBusiness Invoicebs;
        private readonly InvoiceTemplateBusiness TemplateBusiness;
        private readonly SystemSettingBusiness SettingBusiness;
        private readonly SystemSettingBusiness systemSettingusiness;
        private readonly AnnoucementBusiness announcementBusiness;
        private readonly CompanyBusiness companyBusiness;
        private readonly MVanBusiness mVanbusiness;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.InvoicePortalController()'
        public InvoicePortalController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.InvoicePortalController()'
        {
            business = new InvoicePortalBusiness(GetBOFactory());
            Invoicebs = new InvoiceBusiness(GetBOFactory(), true);
            TemplateBusiness = new InvoiceTemplateBusiness(GetBOFactory(), true);
            SettingBusiness = new SystemSettingBusiness(GetBOFactory());
            announcementBusiness = new AnnoucementBusiness(GetBOFactory());
            systemSettingusiness = new SystemSettingBusiness(GetBOFactory());
            companyBusiness = new CompanyBusiness(GetBOFactory());
            mVanbusiness = new MVanBusiness(GetServiceFactory());
            _ = DefaultFields.ASSET_FOLDER;  // dùng để khởi tạo giá trị mặc định, không được xóa
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("SearchInvoiceByVeriCode/{strSearch?}/{clientId?}/{dateFrom?}/{dateTo?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetInvoiceByVeriCode(string, long, string, string)'
        public IHttpActionResult GetInvoiceByVeriCode(string strSearch = null, long clientId = 0, string dateFrom = null, string dateTo = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetInvoiceByVeriCode(string, long, string, string)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult<IEnumerable>();
            try
            {
                //logger.Error("Check client id : " + clientId, new Exception("Clientid"));
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                if (clientId == 0)
                {
                    response.Data = this.business.GetInvoiceByVeriCode(strSearch);
                }
                else
                {
                    response.Data = this.business.GetInvoiceByVeriCodeAndClientId(strSearch, clientId, dateFrom, dateTo);
                }

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
        [Route("Invoice-download-pdf/{id?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTDownPDF(long)'
        public IHttpActionResult PTDownPDF(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTDownPDF(long)'
        {
            try
            {
                ExportFileInfo fileInfo = Invoicebs.PrintView(id);
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
        [Route("Invoice-download-xml/{id?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTDownXML(long)'
        public IHttpActionResult PTDownXML(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTDownXML(long)'
        {
            try
            {
                ExportFileInfo fileInfo = Invoicebs.GetXmlFile(id);
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
        [Route("InvoicePortal-preview/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTPreviewInvoiceData(long)'
        public IHttpActionResult PTPreviewInvoiceData(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTPreviewInvoiceData(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoicePrintInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.Invoicebs.GetInvoiceData(id);
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
        [Route("InvoicePortal-release-info/{id?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTGetInvoiceReleaseInfo(long)'
        public IHttpActionResult PTGetInvoiceReleaseInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTGetInvoiceReleaseInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceReleaseInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.Invoicebs.PTGetInvoiceReleaseInfo(id);
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
        [Route("get-list-template")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetList()'
        public IHttpActionResult GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetList()'
        {
            var response = new ApiResultList<RegisterTemplateMaster>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = TemplateBusiness.GetList();
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
        [Route("InvoicePortal-templates/{cpn?}")]
        [AllowAnonymous]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetList(long)'
        public IHttpActionResult GetList(long cpn)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetList(long)'
        {
            var response = new ApiResultList<RegisterTemplateMaster>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = TemplateBusiness.PTGetList(cpn);
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
        [Route("InvoicePortal-get-companyID/{Type?}/{VerifiCode?}")]
        [AllowAnonymous]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetCompanyId(string, string)'
        public IHttpActionResult GetCompanyId(string Type, string VerifiCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetCompanyId(string, string)'
        {
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                if (Type == "1")
                {
                    response.Data = business.GetCompanyIdByInvoiceVerifiCode(VerifiCode).ToString();
                }
                else
                {
                    response.Data = business.GetCompanyIdByAnnounVerifiCode(VerifiCode).ToString();
                }
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
        [Route("InvoicePortal-systemSettings/{cpn?}")]
        [AllowAnonymous]
        //[CustomAuthorize(Roles = UserPermission.SystemSetting_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTGetSystemSettingDetail(long)'
        public IHttpActionResult PTGetSystemSettingDetail(long cpn)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PTGetSystemSettingDetail(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<SystemSettingInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.SettingBusiness.GetSystemSettingDetail(cpn);

                // xóa bớt các thông tin khi đưa lên portal
                // chỉ để lại phần format số
                response.Data.EmailReceiveWarning = null;
                response.Data.FileCA = null;
                response.Data.IsWarningMinimum = false;
                response.Data.NumberMinimum = 0;
                response.Data.ServerSign = false;
                response.Data.SignAfterApprove = null;
                response.Data.StepToCreateInvoiceNo = 0;
                response.Data.TypeSignServer = null;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Code"></param>
        /// <param name="cpn"></param>

        /// <returns></returns>
        [HttpGet]
        [Route("InvoicePortal-GetToken/{Code?}/{cpn?}")]
        [AllowAnonymous]
        public IHttpActionResult GetToken(string Code, long cpn)
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = business.GetToken(Code, cpn);
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
        [Route("SearchInvoiceByInvoiceNo/{taxCode?}/{strSearch?}/{dateFrom?}/{dateTo?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.SearchInvoiceByInvoiceNo(string, string, string, string)'
        public IHttpActionResult SearchInvoiceByInvoiceNo(string taxCode, string strSearch = null, string dateFrom = null, string dateTo = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.SearchInvoiceByInvoiceNo(string, string, string, string)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<IEnumerable>();

            try
            {
                //logger.Error("SearchInvoiceByInvoiceNo taxcode : " + taxCode, new Exception("SearchInvoiceByInvoiceNo"));
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.SearchInvoiceByInvoiceNo(strSearch, taxCode, dateFrom, dateTo);
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
        [Route("ExcelInvoiceNoClient/{clientID?}/{strSearch?}/{dateFrom?}/{dateTo?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.ExcelInvoiceNoClient(long, string, string, string)'
        public IHttpActionResult ExcelInvoiceNoClient(long clientID, string strSearch = null, string dateFrom = null, string dateTo = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.ExcelInvoiceNoClient(long, string, string, string)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ExcelInvoiceNoClient(strSearch, clientID, dateFrom, dateTo);
                if (!File.Exists(fileInfo.FullPathFileName))
                {
                    return NotFound();
                }
                InvoiceFile file = new InvoiceFile()
                {
                    FilePath = fileInfo.FullPathFileName,
                    FileName = null,
                };
                SetResponseHeaders("Content-Disposition", "inline; filename=" + fileInfo.FileName);
                return Ok(file);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message);
            }
        }
        [HttpGet]
        [Route("ExcelInvoiceByVeriCodeLogin/{clientID?}/{strSearch?}/{dateFrom?}/{dateTo?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.ExcelInvoiceByVeriCodeLogin(long, string, string, string)'
        public IHttpActionResult ExcelInvoiceByVeriCodeLogin(long clientID, string strSearch = null, string dateFrom = null, string dateTo = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.ExcelInvoiceByVeriCodeLogin(long, string, string, string)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ExcelInvoiceByVeriCodeLogin(strSearch, clientID, dateFrom, dateTo);
                if (!File.Exists(fileInfo.FullPathFileName))
                {
                    return NotFound();
                }
                InvoiceFile file = new InvoiceFile()
                {
                    FilePath = fileInfo.FullPathFileName,
                    FileName = null,
                };
                SetResponseHeaders("Content-Disposition", "inline; filename=" + fileInfo.FileName);
                return Ok(file);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message);
            }
        }
        [HttpGet]
        [Route("InvoicePortal-GetTokenByInvoiceCode/{Code?}/{cpn?}/{CLID?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetTokenByInvoiceCode(string, long, long)'
        public IHttpActionResult GetTokenByInvoiceCode(string Code, long cpn, long CLID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetTokenByInvoiceCode(string, long, long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = business.GetTokenByInvoiceCode(Code, cpn, CLID);
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
        [Route("InvoicePortal-UpdateStatusClientSign/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.UpdateStatus(long, StatusReleaseInvoice)'
        public IHttpActionResult UpdateStatus(long id, StatusReleaseInvoice statusRelease)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.UpdateStatus(long, StatusReleaseInvoice)'
        {
            var response = new ApiResult<Result>();
            try
            {
                response.Code = this.business.UpdataStatusReleaseDetailInvoice(id);
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
        [Route("announcement-update-status-client-sign/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.AnnounUpdateStatusClientSign(long, StatusReleaseAnnouncement)'
        public IHttpActionResult AnnounUpdateStatusClientSign(long id, StatusReleaseAnnouncement statusRelease)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.AnnounUpdateStatusClientSign(long, StatusReleaseAnnouncement)'
        {
            var response = new ApiResult<Result>();
            try
            {
                response.Code = this.business.AnnouncementUpdataStatusClientSign(id);
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
        [Route("VerifyXml")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.VerifyXml()'
        public IHttpActionResult VerifyXml()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.VerifyXml()'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult<string>();
            try
            {
                var request = HttpContext.Current.Request;
                string _filenameUpload = request.Files[0].FileName;
                string fordelUpload = HttpContext.Current.Server.MapPath("~/Data/Asset/xmlSigned/");
                if (!Directory.Exists(fordelUpload))
                {
                    Directory.CreateDirectory(fordelUpload);
                }
                string _urlFileUpload = Path.Combine(fordelUpload, _filenameUpload);
                var streamFile = request.Files[0].InputStream;
                using (var fileStream = File.Create(_urlFileUpload))
                {
                    streamFile.CopyTo(fileStream);
                }
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.Invoicebs.Verify(_urlFileUpload).ToString();
                if (File.Exists(_urlFileUpload))
                {
                    File.Delete(_urlFileUpload);
                }
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
        [Route("Announcement-LoadList/{id?}/{actiontype?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.LoadList(long, int)'
        public IHttpActionResult LoadList(long id, int actiontype)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.LoadList(long, int)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<AnnouncementMD>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.announcementBusiness.LoadList(id, actiontype, true); // id of announcement is true, id of invoice is false

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
        [Route("systemSettings")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetSystemSettingDetail()'
        public IHttpActionResult GetSystemSettingDetail()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetSystemSettingDetail()'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<SystemSettingInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.systemSettingusiness.GetSystemSettingDetail((long)this.CurrentUser.Company.Id);
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
        [Route("Search-Announ-By-VeriCode/{strSearch?}/{clientId?}/{dateFrom?}/{dateTo?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetAnnounByVeriCode(string, long, string, string)'
        public IHttpActionResult GetAnnounByVeriCode(string strSearch = null, long clientId = 0, string dateFrom = null, string dateTo = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetAnnounByVeriCode(string, long, string, string)'
        {
            
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult<IEnumerable>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                if (clientId == 0)
                {
                    response.Data = this.business.GetAnnounByVeriCode(strSearch);
                }
                else
                {
                    response.Data = this.business.GetAnnounByVeriCodeAndClientId(strSearch, clientId, dateFrom, dateTo);
                }

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
        [Route("Search-Announ-By-InvoiceNo/{strSearch?}/{clientId?}/{dateFrom?}/{dateTo?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetAnnounByInvoiceNo(string, long, string, string)'
        public IHttpActionResult GetAnnounByInvoiceNo(string strSearch = null, long clientId = 0, string dateFrom = null, string dateTo = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetAnnounByInvoiceNo(string, long, string, string)'
        {
            //logger.Error("Current client id GetAnnounByInvoiceNo " + clientId + " InvoiceNo: " + strSearch, new Exception("Current client id GetAnnounByInvoiceNo"));

            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<IEnumerable>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetAnnounByInvoiceNo(strSearch, clientId, dateFrom, dateTo);

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
        [Route("Announcement-download-pdf/{id}/{companyId}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PreviewAnnoucementPdf(long, long)'
        public IHttpActionResult PreviewAnnoucementPdf(long id, long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.PreviewAnnoucementPdf(long, long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.announcementBusiness.PrintViewPdfPortal(id, companyId);
                if (!File.Exists(fileInfo.FullPathFileName))
                {
                    return NotFound();
                }

                InvoiceFile file = new InvoiceFile()
                {
                    FilePath = fileInfo.FullPathFileName,
                    FileName = null,
                };

                SetResponseHeaders("Content-Disposition", "inline; filename=" + fileInfo.FileName);
                return Ok(file);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file. Exception: " + ex.Message);
            }
        }


        [HttpGet]
        [Route("companies/mycompany/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetMyCompanyInfo(long)'
        public IHttpActionResult GetMyCompanyInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetMyCompanyInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<MyCompanyInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetMyCompanyInfo(id);
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
        [Route("filter-invoice-symbol/{templateId}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.FilterInvoiceSymbol(long)'
        public IHttpActionResult FilterInvoiceSymbol(long templateId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.FilterInvoiceSymbol(long)'
        {
            var response = new ApiResultList<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = Invoicebs.FilterInvoiceSymbol(templateId);
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
        [Route("get-client-info/{ClientId}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetClientInfo(long)'
        public IHttpActionResult GetClientInfo(long ClientId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetClientInfo(long)'
        {
            var response = new ApiResult<ClientAddInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetClientInfo(ClientId);
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
        [Route("update-client-info/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.UpdateClient(long, ClientAddInfo)'
        public IHttpActionResult UpdateClient(long id, ClientAddInfo clientInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.UpdateClient(long, ClientAddInfo)'
        {
            if (!ModelState.IsValid || clientInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                response.Code = this.business.UpdateClientInfo(id, clientInfo);
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
        [Route("change-password/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.UpdateClientPassword(long, ChangePassword)'
        public IHttpActionResult UpdateClientPassword(long id, ChangePassword updatePasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.UpdateClientPassword(long, ChangePassword)'
        {
            if (!ModelState.IsValid || updatePasswordInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                this.business.UpdateClientPassword(id, updatePasswordInfo);
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(id.ToString(), ex);
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
        [Route("reset-password")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.ResetClientPassword(ResetPassword)'
        public IHttpActionResult ResetClientPassword(ResetPassword resetPasswordInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.ResetClientPassword(ResetPassword)'
        {
            if (!ModelState.IsValid || resetPasswordInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                //logger.Error("ResetClientPassword with userid : " + resetPasswordInfo.UserId, new Exception("ResetClientPassword"));
                this.business.ResetClientPassword(resetPasswordInfo);
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(resetPasswordInfo.UserId, ex);
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
        [Route("get-footer-info")]
        [AllowAnonymous]
        // Get từ web.config để insert vào footer phần xem hóa đơn mẫu
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetFooterInfo()'
        public IHttpActionResult GetFooterInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetFooterInfo()'
        {
            var response = new ApiResult<FooterInvoiceInfo>();

            try
            {
                response.Data = this.companyBusiness.GetFooterInfo();
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Code = ResultCode.NoError;
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
        [Route("portal-get-footer-info")]
        [AllowAnonymous]
        // Get thong tin cong ty từ web.config để hien thi len phan footer cua trang portal
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetFooterInfoPortal()'
        public IHttpActionResult GetFooterInfoPortal()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetFooterInfoPortal()'
        {
            var response = new ApiResult<PTFooterInfo>();

            try
            {
                response.Data = this.business.GetFooterInfo();
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Code = ResultCode.NoError;
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
        [Route("send-email-client-account-info")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.SendEmailClientAccountInfo()'
        public IHttpActionResult SendEmailClientAccountInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.SendEmailClientAccountInfo()'
        {
            var response = new ApiResult<Result>();
            var isJob = 1;
            try
            {
                response.Code = this.business.SendEmailClientAccountInfo(isJob);
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
        [Route("getListStatiscal/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetListStatiscal(long)'
        public IHttpActionResult GetListStatiscal(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetListStatiscal(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResultList<InvoiceStatistical>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.Invoicebs.GetListStatisticals(id);
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
        [Route("getListGift/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetListGift(long)'
        public IHttpActionResult GetListGift(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetListGift(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResultList<InvoiceGift>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.Invoicebs.GetListGift(id);
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
        [Route("downloadStatistical/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.DownloadStatistical(long)'
        public IHttpActionResult DownloadStatistical(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.DownloadStatistical(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.Invoicebs.DownloadStatistical(id);
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
        [Route("downloadInvoiceGift/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.DownloadInvoiceGift(long)'
        public IHttpActionResult DownloadInvoiceGift(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.DownloadInvoiceGift(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.Invoicebs.DownloadGift(id);
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
        [Route("get-header-info")]
        [AllowAnonymous]
        // Get phần tên công ty của Hua Nan Bank
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetHeaderInfo()'
        public IHttpActionResult GetHeaderInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetHeaderInfo()'
        {
            var response = new ApiResult<dynamic>();

            try
            {
                response.Data = this.companyBusiness.GetHeaderInfo();
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Code = ResultCode.NoError;
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
        [Route("customer-index")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetCustomerIndex()'
        public IHttpActionResult GetCustomerIndex()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.GetCustomerIndex()'
        {
            var response = new ApiResult<string>();

            try
            {
                response.Data = Config.ApplicationSetting.Instance.CustomerIndex;
                response.Code = ResultCode.NoError;
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
        [Route("just-to-do-keep-session")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.JustToDoKeepSession()'
        public IHttpActionResult JustToDoKeepSession()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.JustToDoKeepSession()'
        {
            var response = new ApiResult<string>();

            try
            {
                response.Data = "success";
                response.Code = ResultCode.NoError;
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
        [Route("Receivedata")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.MVanReceivedata(MVanResponse)'
        public IHttpActionResult MVanReceivedata(MVanResponse input)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.MVanReceivedata(MVanResponse)'
        {
            var response = new MVanResult();
            try
            {
                response.code = "00";
                response.message = "";
                response.data = this.mVanbusiness.MVanReceivedata(input);
            }
            catch (Exception ex)
            {
                response.code = "01";
                response.message = MsgInternalServerError;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            return Ok(response);
        }

        [HttpGet]
        [Route("hide-sign")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.HidedSign()'
        public IHttpActionResult HidedSign()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoicePortalController.HidedSign()'
        {
            var response = new ApiResult<string>();

            try
            {
                response.Data = this.business.GetConfigSignPortal();
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

        #endregion API mdethods
    }
}