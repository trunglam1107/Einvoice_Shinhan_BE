using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
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
    [RoutePrefix("products")]
    public class ProductController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ProductBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.ProductController()'
        public ProductController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.ProductController()'
        {
            business = new ProductBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpGet]
        [Route("{productCode?}/{productName?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.ProductManagement_Read + ", " + UserPermission.CreateInvoice_Create
            + ", " + UserPermission.InvoiceAdjustment_Create + ", " + UserPermission.SubstituteInvoice_Create
            + ", " + UserPermission.InvoiceManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.FillterProduct(string, string, string, string, int, int)'
        public IHttpActionResult FillterProduct(string productCode = null, string productName = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.FillterProduct(string, string, string, string, int, int)'
        {
            var response = new ApiResultList<ProductInfo>();
            try
            {
                long totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.FillterProduct(out totalRecords, productCode, productName, orderby, orderType, skip, take);
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
        [CustomAuthorize(Roles = UserPermission.ProductManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.GetProductDetail(long)'
        public IHttpActionResult GetProductDetail(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.GetProductDetail(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ProductInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetProductDetail(id);
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
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.MyProductUsing(long)'
        public IHttpActionResult MyProductUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.MyProductUsing(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<MyUsingModel>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.MyProductUsing(id);
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
        [CustomAuthorize(Roles = UserPermission.ProductManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.CreateProduct(ProductInfo)'
        public IHttpActionResult CreateProduct(ProductInfo productInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.CreateProduct(ProductInfo)'
        {
            if (!ModelState.IsValid || productInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(productInfo);
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
        [CustomAuthorize(Roles = UserPermission.ProductManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.UpdateProduct(long, ProductInfo)'
        public IHttpActionResult UpdateProduct(long id, ProductInfo productInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.UpdateProduct(long, ProductInfo)'
        {
            if (!ModelState.IsValid || productInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                response.Code = this.business.Update(id, productInfo);
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
        [CustomAuthorize(Roles = UserPermission.ProductManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.DeleteProduct(long)'
        public IHttpActionResult DeleteProduct(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.DeleteProduct(long)'
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

        [HttpPost]
        [Route("upload")]
        [CustomAuthorize(Roles = UserPermission.UploadProduct_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.UploadData(FileImport)'
        public IHttpActionResult UploadData(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.UploadData(FileImport)'
        {
            if (fileImport == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ResultImportSheet>();

            try
            {
                var resultImport = this.business.ImportData(fileImport);
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
        [Route("download")]
        [ResponseType(typeof(InvoiceFile))]
        [CustomAuthorize(Roles = UserPermission.UploadProduct_Read + ", " + UserPermission.ProductManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ProductController.DownloadData(long, string, string)'
        public IHttpActionResult DownloadData(long companyId = 0, string code = null, string name = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ProductController.DownloadData(long, string, string)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.DownloadDataProduct(code, name);
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