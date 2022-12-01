using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
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
    [RoutePrefix("register-templates")]
    public class RegisterTemplateController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly RegisterTemplateBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.RegisterTemplateController()'
        public RegisterTemplateController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.RegisterTemplateController()'
        {
            business = new RegisterTemplateBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods
        [HttpGet]
        [Route("{companyId?}/{keyword?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.RegisterTemplate_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.Fillter(long, string, string, string, int, int)'
        public IHttpActionResult Fillter(long companyId = 0, string keyword = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.Fillter(long, string, string, string, int, int)'
        {
            var response = new ApiResultList<RegisterTemplateMaster>();
            try
            {
                var condition = new ConditionSearchRegisterTemplate(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Key = keyword,
                    Branch = companyId,
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
        [Route("{id}")]
        [CustomAuthorize(Roles = UserPermission.RegisterTemplate_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.GetTemplateCompanyUseInfo(long)'
        public IHttpActionResult GetTemplateCompanyUseInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.GetTemplateCompanyUseInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<RegisterTemplateInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetTemplateCompanyUseInfo(id);
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
        [CustomAuthorize(Roles = UserPermission.RegisterTemplate_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.Create(RegisterTemplateInfo)'
        public IHttpActionResult Create(RegisterTemplateInfo invoiceReleasesInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.Create(RegisterTemplateInfo)'
        {
            if (!ModelState.IsValid || invoiceReleasesInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(invoiceReleasesInfo);
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
        [CustomAuthorize(Roles = UserPermission.RegisterTemplate_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.Update(long, RegisterTemplateInfo)'
        public IHttpActionResult Update(long id, RegisterTemplateInfo invoiceReleasesInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.Update(long, RegisterTemplateInfo)'
        {
            if (!ModelState.IsValid || invoiceReleasesInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var response = new ApiResult();
            try
            {
                response.Code = this.business.Update(id, invoiceReleasesInfo);
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
        [CustomAuthorize(Roles = UserPermission.RegisterTemplate_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.Delete(long)'
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
        [Route("print-view/{code?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.PreviewInvoice(string)'
        public IHttpActionResult PreviewInvoice(string code = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.PreviewInvoice(string)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.PrintView(code);
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
        [Route("registered-template-sample/{companyID}")]
        [CustomAuthorize(Roles = UserPermission.RegisterTemplate_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.GetRegisteredTemplateSample(int)'
        public IHttpActionResult GetRegisteredTemplateSample(int companyID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'RegisterTemplateController.GetRegisteredTemplateSample(int)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResultList<RegisterTemplateSample>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetRegisterTemplateSampleOfCompany(companyID);
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