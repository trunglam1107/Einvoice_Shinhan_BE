using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Net;
using System.Web;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("releaseAnnouncement")]
    public class ReleaseAnnouncementController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ReleaseAnnouncementBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.ReleaseAnnouncementController()'
        public ReleaseAnnouncementController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.ReleaseAnnouncementController()'
        {
            business = new ReleaseAnnouncementBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpPost]
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.AnnouncementManagement_Sign)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.Create(ReleaseAnnouncementMaster)'
        public IHttpActionResult Create(ReleaseAnnouncementMaster announcementInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.Create(ReleaseAnnouncementMaster)'
        {
            var response = new ApiResult<Result>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Data = this.business.Create(announcementInfo);
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
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.GetFileAnnouncementRelease(long)'
        public IHttpActionResult GetFileAnnouncementRelease(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.GetFileAnnouncementRelease(long)'
        {
            var response = new ApiResultList<AnnouncementsRelease>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetReleaseAnnouncements(id);
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
        [Route("update-status/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.UpdateStatus(long, StatusReleaseAnnouncement)'
        public IHttpActionResult UpdateStatus(long id, StatusReleaseAnnouncement statusRelease)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.UpdateStatus(long, StatusReleaseAnnouncement)'
        {
            var response = new ApiResult<Result>();

            try
            {
                response.Code = this.business.UpdataStatusReleaseDetailAnnouncement(id, statusRelease);
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
        [Route("upload-release-announcements/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.UploadReleaseAnnouncements(long)'
        public IHttpActionResult UploadReleaseAnnouncements(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.UploadReleaseAnnouncements(long)'
        {
            ResultCode resultCode;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var fileUploadInfo = FileUploadInfo.FromRequest(httpRequest);

                UploadFileBusiness uploadFileBusiness = new UploadFileBusiness();
                if (uploadFileBusiness.IsValidFile(fileUploadInfo))
                {
                    var sourceFile = uploadFileBusiness.CreateFilePathByDocId(id, fileUploadInfo);
                    resultCode = uploadFileBusiness.SaveFile(fileUploadInfo, sourceFile);
                    if (resultCode == ResultCode.NoError)
                    {
                        string sourceFileStandard = uploadFileBusiness.StandardAnnounFileUpload(sourceFile);
                        this.business.UpdataReleaseStatus(id, sourceFileStandard);
                    }
                }
                else
                {
                    resultCode = ResultCode.RequestDataInvalid;
                }
            }
            catch (BusinessLogicException ex)
            {
                resultCode = ex.ErrorCode;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                resultCode = ResultCode.UnknownError;
                logger.Error(this.CurrentUser.UserId, ex);
            }

            IHttpActionResult response = null;
            switch (resultCode)
            {
                case ResultCode.WaitNextRequest:
                case ResultCode.NoError:
                    {
                        response = NoContent(HttpStatusCode.NoContent);
                        break;
                    }
                case ResultCode.RequestDataInvalid:
                    {
                        response = Text(HttpStatusCode.BadRequest, MsgRequestDataInvalid);
                        break;
                    }
                case ResultCode.UnknownError:
                default:
                    {
                        response = Text(HttpStatusCode.InternalServerError, MsgInternalServerError);
                        break;
                    }
            }

            return response;
        }

        [HttpPost]
        [Route("getPathAnnounFile")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.GetAnnounPathFile(ReleaseAnnouncementMaster)'
        public IHttpActionResult GetAnnounPathFile(ReleaseAnnouncementMaster announcementInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.GetAnnounPathFile(ReleaseAnnouncementMaster)'
        {
            var response = new ApiResult<ServerSignAnnounResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.SignAnnounFile(announcementInfo);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }

            return Ok(response);
        }


        [HttpPost]
        [Route("client-release-announcements/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.ClientReleaseAnnouncements(long)'
        public IHttpActionResult ClientReleaseAnnouncements(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseAnnouncementController.ClientReleaseAnnouncements(long)'
        {
            ResultCode resultCode;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var fileUploadInfo = FileUploadInfo.FromRequest(httpRequest);

                UploadFileBusiness uploadFileBusiness = new UploadFileBusiness();
                if (uploadFileBusiness.IsValidFile(fileUploadInfo))
                {
                    var releaseAnnouncement = this.business.GetReleaseAnnouncementByAnnouncementId(id);
                    long companyId = releaseAnnouncement.COMPANYID.Value;
                    long releaseId = releaseAnnouncement.ID;
                    var sourceFile = uploadFileBusiness.CreateFilePathByDocId(releaseId, companyId, fileUploadInfo);
                    resultCode = uploadFileBusiness.SaveFile(fileUploadInfo, sourceFile);
                }
                else
                {
                    resultCode = ResultCode.RequestDataInvalid;
                }
            }
            catch (BusinessLogicException ex)
            {
                resultCode = ex.ErrorCode;
                logger.Error(this.CurrentUser.UserId, ex);
            }
            catch (Exception ex)
            {
                resultCode = ResultCode.UnknownError;
                logger.Error(this.CurrentUser.UserId, ex);
            }

            IHttpActionResult response = null;
            switch (resultCode)
            {
                case ResultCode.WaitNextRequest:
                case ResultCode.NoError:
                    {
                        response = NoContent(HttpStatusCode.NoContent);
                        break;
                    }
                case ResultCode.RequestDataInvalid:
                    {
                        response = Text(HttpStatusCode.BadRequest, MsgRequestDataInvalid);
                        break;
                    }
                case ResultCode.UnknownError:
                default:
                    {
                        response = Text(HttpStatusCode.InternalServerError, MsgInternalServerError);
                        break;
                    }
            }

            return response;
        }

        #endregion API methods
    }
}