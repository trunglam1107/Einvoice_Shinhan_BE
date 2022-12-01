using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Annoucement;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("announcement")]

    public class AnnouncementController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly AnnoucementBusiness business;
        private readonly InvoiceBusiness Invoicebs;
        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.AnnouncementController()'
        public AnnouncementController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.AnnouncementController()'
        {
            business = new AnnoucementBusiness(GetBOFactory());
            Invoicebs = new InvoiceBusiness(GetBOFactory());
        }



        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{code?}/{symbol?}/{announcementStatus?}/{invoiceNo?}/{clientName?}/{taxCode?}/{announcementType?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{minutesNo?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Fillter(string, string, int?, string, string, string, int?, string, string, string, string, string, string, string, string, int, int)'
        public IHttpActionResult Fillter(string code = null, string symbol = null, int? announcementStatus = null, string invoiceNo = null, string clientName = null, string taxCode = null, int? announcementType = null, string numberAccout = null, string dateFrom = null, string dateTo = null, string branch = null, string minutesNo = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Fillter(string, string, int?, string, string, string, int?, string, string, string, string, string, string, string, string, int, int)'
        {
            var response = new ApiResultList<AnnouncementMaster>();
            try
            {
                var condition = new ConditionSearchAnnouncement(orderBy, orderType, announcementStatus)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Denominator = code.DecodeUrl(),
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    ClientName = clientName.DecodeUrl(),
                    ClientTaxCode = taxCode.DecodeUrl(),
                    AnnouncementType = announcementType,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch.ToInt(),
                    MinutesNo = minutesNo.DecodeUrl(),
                    CustomerCode = customerCode.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Data = business.Fillter(condition);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                     = new Dictionary<string, string>(){
                     {CustomHttpRequestHeader.AccessControlExposeHeaders, "X-Collection-Total, X-Collection-Skip, X-Collection-Take"},
                     {CustomHttpRequestHeader.CollectionTotal, condition.TotalRecords.ToString()},
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
        [Route("print-view/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.PreviewAnnoucement(long)'
        public IHttpActionResult PreviewAnnoucement(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.PreviewAnnoucement(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.PrintView(id);
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
        [Route("print-view-pdf/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.PreviewAnnoucementPdf(long)'
        public IHttpActionResult PreviewAnnoucementPdf(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.PreviewAnnoucementPdf(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.PrintViewPdf(id);
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
        [HttpPost]
        [Route("approved")]
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Approve + ", " + UserPermission.AnnouncementManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.ApproveAnnoucenment(ApprovedAnnouncement)'
        public IHttpActionResult ApproveAnnoucenment(ApprovedAnnouncement announcementInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.ApproveAnnoucenment(ApprovedAnnouncement)'
        {
            var response = new ApiResult<ResultApproved>();

            try
            {
                var resultImport = this.business.ApproveAnnouncement(announcementInfo);
                response.Message = resultImport.Message;
                response.Code = resultImport.ErrorCode;
                response.Data = resultImport;
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
        [Route("cancel-approved")]
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Approve + ", " + UserPermission.AnnouncementManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.CancelApproveAnnoucenment(ApprovedAnnouncement)'
        public IHttpActionResult CancelApproveAnnoucenment(ApprovedAnnouncement announcementInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.CancelApproveAnnoucenment(ApprovedAnnouncement)'
        {
            var response = new ApiResult<ResultApproved>();

            try
            {
                var resultImport = this.business.CancelApproveAnnouncement(announcementInfo);
                response.Message = resultImport.Message;
                response.Code = resultImport.ErrorCode;
                response.Data = resultImport;
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
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.GetAnnouncementInfo(long)'
        public IHttpActionResult GetAnnouncementInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.GetAnnouncementInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<AnnouncementInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetAnnouncementInfo(id);

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
        [Route("announcement-of-client/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.GetInvoiceOfClient(long)'
        public IHttpActionResult GetInvoiceOfClient(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.GetInvoiceOfClient(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<AnnouncementsRelease>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetAnnouncementOfClient(id);
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
        [Route("GetAnnouncementByInvoiceId/{invoiceId}/{announType}")]
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.GetAnnouncementByInvoiceId(long, int)'
        public IHttpActionResult GetAnnouncementByInvoiceId(long invoiceId, int announType)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.GetAnnouncementByInvoiceId(long, int)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<AnnouncementInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetAnnouncementByInvoiceId(invoiceId, announType);

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
        [Route("checkExistAnnoun/{invoiceId}/{invoiceType}/{announType}")]
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.CheckExistAnnoun(long, bool, int)'
        public IHttpActionResult CheckExistAnnoun(long invoiceId, bool invoiceType, int announType)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.CheckExistAnnoun(long, bool, int)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<AnnouncementInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.CheckExistAnnoun(invoiceId, invoiceType, announType);

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
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Delete + ", " + UserPermission.AnnouncementManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Delete(long)'
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
        [Route("Announcement-SearchCode/{id}")]

        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCheckout_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.SearchCode(long)'
        public IHttpActionResult SearchCode(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.SearchCode(long)'
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
                response.Data = this.business.GetInvoiceData(id);
                //response.Data = this.business.GetInvoiceData_Replace(id);
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
        [Route(" ")]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCheckout_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Save(long)'
        public IHttpActionResult Save(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Save(long)'
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

        [HttpPost]
        [Route("Save")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.SaveMD(AnnouncementMD)'
        public IHttpActionResult SaveMD(AnnouncementMD anInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.SaveMD(AnnouncementMD)'
        {

            if (!ModelState.IsValid || anInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (this.business.checkExist(anInfo.MinutesNo, null, anInfo.CompanyId))
            {
                return Error(ResultCode.AnnouncementMinuIsExist, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult<ANNOUNCEMENT>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = this.business.SaveMD(anInfo);
                response.Message = Authentication.LoginSuccessful;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(anInfo.id.ToString(), ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(anInfo.id.ToString(), ex);
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("Announcement-LoadList/{id?}/{actiontype?}")]

        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.InvoiceCheckout_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.LoadList(long, int)'
        public IHttpActionResult LoadList(long id, int actiontype)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.LoadList(long, int)'
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
                response.Data = this.business.LoadList(id, actiontype, true);// id of announcement is true, id of invoice is false

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
        [Route("Update")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Update(AnnouncementMD)'
        public IHttpActionResult Update(AnnouncementMD anInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Update(AnnouncementMD)'
        {

            if (!ModelState.IsValid || anInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (this.business.checkExist(anInfo.MinutesNo, anInfo.id, anInfo.CompanyId))
            {
                return Error(ResultCode.AnnouncementMinuIsExist, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult<Result>();

            try
            {
                response.Code = this.business.Update(anInfo);
                response.Message = Authentication.LoginSuccessful;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(anInfo.id.ToString(), ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(anInfo.id.ToString(), ex);
            }

            return Ok(response);
        }


        [HttpPost]
        [Route("UpdateAnnouncementStatus")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.UpdateAnnouncementStatus(AnnouncementMD)'
        public IHttpActionResult UpdateAnnouncementStatus(AnnouncementMD anInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.UpdateAnnouncementStatus(AnnouncementMD)'
        {

            if (!ModelState.IsValid || anInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<string>();

            try
            {
                response.Data = this.business.UpdateAnnouncementStatus(anInfo);
                response.Message = Authentication.LoginSuccessful;
                response.Code = ResultCode.NoError;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                response.Data = "";
                logger.Error(anInfo.id.ToString(), ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                response.Data = "";
                logger.Error(anInfo.id.ToString(), ex);
            }

            return Ok(response);
        }


        [HttpPost]
        [Route("Remove")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Remove(AnnouncementMD)'
        public IHttpActionResult Remove(AnnouncementMD anInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.Remove(AnnouncementMD)'
        {
            long annoucementID = anInfo.id;
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<Result>();

            try
            {
                response.Code = this.business.Remove(annoucementID, anInfo.UpdatedBy);
                response.Message = Authentication.LoginSuccessful;
            }
            catch (BusinessLogicException ex)
            {
                response.Code = ex.ErrorCode;
                response.Message = ex.Message;
                logger.Error(annoucementID.ToString(), ex);
            }
            catch (Exception ex)
            {
                response.Code = ResultCode.UnknownError;
                response.Message = MsgInternalServerError;
                logger.Error(annoucementID.ToString(), ex);
            }

            return Ok(response);
        }


        [HttpGet]
        [Route("Announcement-IsExit/{id}/{announcementType}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.IsExit(long, int)'
        public IHttpActionResult IsExit(long id, int announcementType)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.IsExit(long, int)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<string>();

            try
            {
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Code = ResultCode.NoError;
                response.Data = this.business.IsExit(id, announcementType);
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
        [Route("Announcement-IsExistAll/{id}")]
        [AllowAnonymous]
        public IHttpActionResult IsExistAll(long id)
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<string>();

            try
            {
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Code = ResultCode.NoError;
                response.Data = this.business.IsExistAll(id);
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
        [Route("send-verification-code")]
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Read)]
        public IHttpActionResult SendVerificationCode(AnnouncementVerificationCode announcementVerificationCode)
        {
            logger.Error("send announcement " + announcementVerificationCode.Email, new Exception(""));
            var response = new ApiResult();

            try
            {
                response.Code = this.business.SendVerificationCode(announcementVerificationCode);
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
        [Route("Invoice-LoadList/{id?}/{actiontype?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.LoadListByInvoice(long, int)'
        public IHttpActionResult LoadListByInvoice(long id, int actiontype)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.LoadListByInvoice(long, int)'
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
                response.Data = this.business.LoadList(id, actiontype, false);// id of announcement is true, id of invoice is false

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
        [Route("filter-invoice-symbol-by-companyId/{templateId}/{companyId}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.CreateInvoice_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.FilterInvoiceSymbolById(long, long)'
        public IHttpActionResult FilterInvoiceSymbolById(long templateId, long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.FilterInvoiceSymbolById(long, long)'
        {
            var response = new ApiResultList<string>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.FilterInvoiceSymbol(templateId, companyId);
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
        [Route("export-excel/{code?}/{symbol?}/{announcementStatus?}/{invoiceNo?}/{clientName?}/{taxCode?}/{announcementType?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{branch?}/{minutesNo?}/{customerCode?}/{orderBy?}/{orderType?}/{skip?}/{take?}/{language?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.ExportExcel(string, string, int?, string, string, string, int?, string, string, string, string, string, string, string, string, int, int, string)'
        public IHttpActionResult ExportExcel(string code = null, string symbol = null, int? announcementStatus = null, string invoiceNo = null, string clientName = null, string taxCode = null, int? announcementType = null, string numberAccout = null, string dateFrom = null, string dateTo = null, string branch = null, string minutesNo = null, string customerCode = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue, string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.ExportExcel(string, string, int?, string, string, string, int?, string, string, string, string, string, string, string, string, int, int, string)'
        {
            try
            {
                var condition = new ConditionSearchAnnouncement(orderBy, orderType, announcementStatus)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Denominator = code.DecodeUrl(),
                    Symbol = symbol,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    ClientName = clientName.DecodeUrl(),
                    ClientTaxCode = taxCode.DecodeUrl(),
                    AnnouncementType = announcementType,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                    Branch = branch.ToInt(),
                    MinutesNo = minutesNo.DecodeUrl(),
                    CustomerCode = customerCode.DecodeUrl(),
                    Language = language,
                };
                ExportFileInfo fileInfo = this.business.ExportExcel(condition);
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
        [Route("checkAnnouncement/{invoiceId}/{type}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read + ", " + UserPermission.SubstituteInvoice_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.CheckAnnouncement(long, int)'
        public Boolean CheckAnnouncement(long invoiceId, int type)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AnnouncementController.CheckAnnouncement(long, int)'
        {
            var result = this.business.CheckAnnount(invoiceId, type);
            return result;
        }
        #endregion API methods
    }
}