using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.IO;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("invoice-templates")]
    public class InvoiceTemplateController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private static readonly object lockDownloadTemplate = new object();
        private readonly InvoiceTemplateBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.InvoiceTemplateController()'
        public InvoiceTemplateController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.InvoiceTemplateController()'
        {
            business = new InvoiceTemplateBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GetList()'
        public IHttpActionResult GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GetList()'
        {
            var response = new ApiResultList<RegisterTemplateMaster>();
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


        [HttpGet]
        [Route("{companyId}")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GetListByCompanyId(long)'
        public IHttpActionResult GetListByCompanyId(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GetListByCompanyId(long)'
        {
            var response = new ApiResultList<RegisterTemplateMaster>();
            try
            {
                response.Code = ResultCode.NoError;
                if (companyId > 0)
                {
                    response.Data = business.PTGetList(companyId);
                }
                else
                {
                    response.Data = business.GetList();
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
        [Route("denominator/{branch?}")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GetListByBranch(long?)'
        public IHttpActionResult GetListByBranch(long? branch = 0)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GetListByBranch(long?)'
        {
            var response = new ApiResultList<RegisterTemplateDenominator>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetListDenominator(branch);
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
        [Route("registers/{id}")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GeInvoiceRegisters(long)'
        public IHttpActionResult GeInvoiceRegisters(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GeInvoiceRegisters(long)'
        {
            var response = new ApiResultList<RegisterTemplateMaster>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetTemplateList(id);
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
        [Route("use/{id}")]
        //[CustomAuthorize(Roles = UserPermission.LoginUser_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GeInvoiceUse(long)'
        public IHttpActionResult GeInvoiceUse(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.GeInvoiceUse(long)'
        {
            var response = new ApiResultList<TemplateAccepted>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetListInvoiceTemplateApproved(id);
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
        [Route("print-view/{id?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.PreviewInvoice(string)'
        public IHttpActionResult PreviewInvoice(string id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.PreviewInvoice(string)'
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

        [HttpGet]
        [Route("print-viewPdf/{id?}/{parentId?}/{companyId?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.PreviewInvoicePdf(long, long)'
        public IHttpActionResult PreviewInvoicePdf(long id, long parentId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.PreviewInvoicePdf(long, long)'
        {
            try
            {
                lock (lockDownloadTemplate)
                {
                    ExportFileInfo fileInfo = this.business.PrintViewPdf(id, parentId);
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
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file");
            }
        }

        [HttpGet]
        [Route("print-viewReleasePdf/{id?}/{parentId?}/{companyId?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.PreviewInvoiceReleasePdf(long, long)'
        public IHttpActionResult PreviewInvoiceReleasePdf(long id, long parentId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.PreviewInvoiceReleasePdf(long, long)'
        {
            try
            {
                lock (lockDownloadTemplate)
                {
                    ExportFileInfo fileInfo = this.business.PrintViewReleasePdf(id, parentId);
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
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file");
            }
        }

        [HttpGet]
        [Route("print-view2/{id?}/{companyId?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.PreviewInvoice(string, long)'
        public IHttpActionResult PreviewInvoice(string id, long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateController.PreviewInvoice(string, long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.PrintView(id, companyId);
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