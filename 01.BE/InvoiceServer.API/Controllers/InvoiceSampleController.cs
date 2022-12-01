using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("invoice-sample")]
    public class InvoiceSampleController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly InvoiceSampleBusiness business;
        private readonly InvoiceTemplateSampleBusiness invoiceTemplateSampleBusiness;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.InvoiceSampleController()'
        public InvoiceSampleController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.InvoiceSampleController()'
        {
            business = new InvoiceSampleBusiness(GetBOFactory());
            invoiceTemplateSampleBusiness = new InvoiceTemplateSampleBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpGet]
        [Route("{keyword?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceSampleManagement_Read + "," + UserPermission.RegisterTemplate_Read
            + ", " + UserPermission.ReportCancelling_Read + ", " + UserPermission.ReportStatisticOneMonth_Read
            + ", " + UserPermission.ReportStatisticThreeMonths_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.Fillter(string, string, string, int, int)'
        public IHttpActionResult Fillter(string keyword = null, string orderType = null, string orderby = null,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.Fillter(string, string, string, int, int)'
            int skip = 0, int take = int.MaxValue)
        {
            var response = new ApiResultList<InvoiceSampleInfo>();
            try
            {
                var condition = new ConditionSearchInvoiceSample(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Key = keyword.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Data = business.Fillter(condition);
                response.Message = MsgApiResponse.ExecuteSeccessful;
                Dictionary<string, string> responHeaders
                    = new Dictionary<string, string>()
                    {
                        {
                            CustomHttpRequestHeader.AccessControlExposeHeaders,
                            "X-Collection-Total, X-Collection-Skip, X-Collection-Take"
                        },
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
        [Route("{invoiceTypeId}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceSampleManagement_Read + ", " + UserPermission.RegisterTemplate_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.GetInvoiceSampleById(long)'
        public IHttpActionResult GetInvoiceSampleById(long invoiceTypeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.GetInvoiceSampleById(long)'
        {
            var response = new ApiResult<InvoiceSampleInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GeInvoiceSampleById(invoiceTypeId);
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
        [CustomAuthorize(Roles = UserPermission.InvoiceSampleManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.CreateInvoiceSample(InvoiceSampleInfo)'
        public IHttpActionResult CreateInvoiceSample(InvoiceSampleInfo typeInvoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.CreateInvoiceSample(InvoiceSampleInfo)'
        {
            var response = new ApiResult<InvoiceSampleInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.CreateInvoiceSample(typeInvoiceInfo);
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
        [Route("update")]
        [CustomAuthorize(Roles = UserPermission.InvoiceSampleManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.UpdateInvoiceSample(InvoiceSampleInfo)'
        public IHttpActionResult UpdateInvoiceSample(InvoiceSampleInfo typeInvoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.UpdateInvoiceSample(InvoiceSampleInfo)'
        {
            var response = new ApiResult<InvoiceSampleInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.UpdateInvoiceSample(typeInvoiceInfo);
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

        [Route("{id}/uploadInvoiceTemplate")]
        [HttpPost]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.uploadInvoiceTemplate(long)'
        public IHttpActionResult uploadInvoiceTemplate(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.uploadInvoiceTemplate(long)'
        {
            string _filenameUpload;
            string _urlFileUpload;
            try
            {
                var request = HttpContext.Current.Request;
                _filenameUpload = request.Files[0].FileName;
                _urlFileUpload = HttpContext.Current.Server.MapPath("~/Data/InvoiceTemplate/" + _filenameUpload);
                var streamFile = request.Files[0].InputStream;
                using (var fileStream = File.Create(_urlFileUpload))
                {
                    streamFile.CopyTo(fileStream);
                }
                //update URL link to InvoiceSample

                var invoiceSampleInfo = business.GetById(id);
                if (invoiceSampleInfo != null)
                {
                    invoiceSampleInfo.FileName = _filenameUpload;
                    invoiceSampleInfo.TemplateURL = _urlFileUpload;
                }
                business.Update(id, invoiceSampleInfo);

                return Ok(_filenameUpload);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file");
            }

        }

        [Route("uploadInvoiceTemplate/{id}/{templateId}")]
        [HttpPost]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.CopyInvoiceTemplate(long, long)'
        public IHttpActionResult CopyInvoiceTemplate(long id, long templateId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.CopyInvoiceTemplate(long, long)'
        {
            string _filenameUpload;
            string _urlFileUpload;
            try
            {
                var invoiceTemplateSampleViewModel = invoiceTemplateSampleBusiness.GetViewModelById(templateId);
                _filenameUpload = invoiceTemplateSampleViewModel.UrlFile.Split('/').Last();
                _urlFileUpload = invoiceTemplateSampleViewModel.UrlFile;

                var invoiceSampleInfo = business.GetById(id);
                if (invoiceSampleInfo != null)
                {
                    invoiceSampleInfo.FileName = _filenameUpload;
                    invoiceSampleInfo.TemplateURL = _urlFileUpload;
                }
                business.CopyInvoiceTemplate(id, invoiceSampleInfo, invoiceTemplateSampleViewModel);

                return Ok(_filenameUpload);
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file");
            }

        }


        // API sẽ lỗi khi code chứa dấu "/" (EX: 01GTTK/123)
        // Đổi sang method GetById()
        [HttpGet]
        [Route("get-by-code/{code}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.GetByCode(string)'
        public IHttpActionResult GetByCode(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.GetByCode(string)'
        {
            if (string.IsNullOrEmpty(code))
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<InvoiceSampleInfo>();

            try
            {
                response.Data = this.business.GetByCode(code);
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
        [Route("get-by-id/{id}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.GetById(long)'
        public IHttpActionResult GetById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.GetById(long)'
        {
            var response = new ApiResult<InvoiceSampleInfo>();

            try
            {
                response.Data = this.business.GetById(id);
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
        [Route("using/{id}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.MyInvoiceSampleUsing(long)'
        public IHttpActionResult MyInvoiceSampleUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.MyInvoiceSampleUsing(long)'
        {
            var response = new ApiResult<MyUsingModel>();

            try
            {
                response.Data = this.business.MyInvoiceSampleUsing(id);
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

        // API sẽ lỗi khi code chứa dấu "/" (EX: 01GTTK/123)
        // Đổi sang method DeleteById()
        [HttpDelete]
        [Route("{code}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceSampleManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.Delete(string)'
        public IHttpActionResult Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.Delete(string)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.Delete(code);
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
        [Route("delete-by-id/{id}")]
        [CustomAuthorize(Roles = UserPermission.InvoiceSampleManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.DeleteById(long)'
        public IHttpActionResult DeleteById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.DeleteById(long)'
        {
            var response = new ApiResult();
            try
            {
                response.Code = this.business.DeleteById(id);
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
        [Route("getByTypeTax/{invoiceTypeId}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.GetByInvoiceTypeId(long)'
        public IHttpActionResult GetByInvoiceTypeId(long invoiceTypeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleController.GetByInvoiceTypeId(long)'
        {
            var response = new ApiResultList<InvoiceSampleInfo>();
            try
            {
                response.Data = this.business.GetByInvoiceTypeId(invoiceTypeId);
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