using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
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
    [RoutePrefix("customers")]
    public class CustomerController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly CustomerBusiness business;

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.CustomerController()'
        public CustomerController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.CustomerController()'
        {
            business = new CustomerBusiness(GetBOFactory());
        }

        #endregion

        #region API methods
        [HttpGet]
        [Route("{customerName?}/{taxCode?}/{nameDelegate?}/{tel?}/{status?}/{branch?}/{branchId?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.FillterCustomer(string, string, string, string, string, int?, long?, string, string, int, int)'
        public IHttpActionResult FillterCustomer(string customerName = null, string taxCode = null, string nameDelegate = null, string branchId = null, string tel = null, int? status = null, long? branch = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.FillterCustomer(string, string, string, string, string, int?, long?, string, string, int, int)'
        {
            var response = new ApiResultList<CustomerInfo>();
            try
            {
                var condition = new ConditionSearchCustomer(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    Delegate = nameDelegate.DecodeUrl(),
                    Tel = tel.DecodeUrl(),
                    Status = status,
                    Branch = branch,
                    BranchId = branchId,

                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterCustomer(condition);
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
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Read + ", " + UserPermission.InvoiceAdjustment_Create + ", "
                               + UserPermission.SubstituteInvoice_Create + ", " + UserPermission.InvoiceManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetCustomerInfo(long)'
        public IHttpActionResult GetCustomerInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetCustomerInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<CustomerInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetCustomerInfo(id);
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
        [Route("get-company-info/{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetCustomerInfoNew(long)'
        public IHttpActionResult GetCustomerInfoNew(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetCustomerInfoNew(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<CustomerInfo>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetCustomerInfo(id);
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
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.Create(CustomerInfo)'
        public IHttpActionResult Create(CustomerInfo customerInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.Create(CustomerInfo)'
        {
            if (!ModelState.IsValid || customerInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(customerInfo);
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
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.Update(long, CustomerInfo)'
        public IHttpActionResult Update(long id, CustomerInfo customerInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.Update(long, CustomerInfo)'
        {
            if (!ModelState.IsValid || customerInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Update(id, customerInfo);
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
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.Delete(long)'
        public IHttpActionResult Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.Delete(long)'
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
        [Route("company/{taxCode}")]
        //[CustomAuthorize(Roles = UserPermission.CustomerManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetCustomerByTaxCode(string)'
        public IHttpActionResult GetCustomerByTaxCode(string taxCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetCustomerByTaxCode(string)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ExternalCustomer>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetCustomerByTaxCode(taxCode);
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
        [Route("branchs")]
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetBranchs()'
        public IHttpActionResult GetBranchs()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetBranchs()'
        {

            var response = new ApiResultList<Branch>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetBranchs();
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
        [Route("get-list-branchs/{isGetAll?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListBranchs(int)'
        public IHttpActionResult GetListBranchs(int isGetAll)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListBranchs(int)'
        {

            var response = new ApiResultList<BranchLevel>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetListBranchs(isGetAll);
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
        [Route("get-list-branchs-v2/{isGetAll?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListBranchs_V2(int)'
        public IHttpActionResult GetListBranchs_V2(int isGetAll)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListBranchs_V2(int)'
        {

            var response = new ApiResultList<BranchLevel>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetListBranchs_V2(isGetAll);
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
        [Route("get-list-branchs-2/{companyId?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListBranchs_2(long)'
        public IHttpActionResult GetListBranchs_2(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListBranchs_2(long)'
        {

            var response = new ApiResultList<BranchLevel>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetListBranchs_2(companyId);
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
        [Route("get-list-all-branchs-2")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListAllBranchs_2()'
        public IHttpActionResult GetListAllBranchs_2()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListAllBranchs_2()'
        {

            var response = new ApiResultList<BranchLevel>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetListAllBranchs();
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
        [Route("get-list-branchs-only/{isGetAll?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListBranchsOnly(int)'
        public IHttpActionResult GetListBranchsOnly(int isGetAll)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.GetListBranchsOnly(int)'
        {
            var response = new ApiResultList<BranchLevel>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetListBranchsOnly(isGetAll);
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

        //add import export excel
        [HttpPost]
        [Route("upload")]
        [CustomAuthorize(Roles = UserPermission.UploadCustomer_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.UploadData(FileImport)'
        public IHttpActionResult UploadData(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.UploadData(FileImport)'
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
        [Route("download/{customerName?}/{taxCode?}/{nameDelegate?}/{tel?}/{status?}/{branch?}/{BranchId?}/{orderType?}/{orderby?}")]
        [CustomAuthorize(Roles = UserPermission.CustomerManagement_Read + ", " + UserPermission.CustomerManagement_Create)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.DownloadData(string, string, string, string, string, int?, long?, string, string)'
        public IHttpActionResult DownloadData(string customerName = null, string taxCode = null, string nameDelegate = null, string BranchId = null, string tel = null, int? status = null, long? branch = null, string orderType = null, string orderby = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomerController.DownloadData(string, string, string, string, string, int?, long?, string, string)'
        {
            try
            {
                var condition = new ConditionSearchCustomer(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    Delegate = nameDelegate.DecodeUrl(),
                    Tel = tel.DecodeUrl(),
                    Status = status,
                    Branch = branch,
                    BranchId = BranchId,
                };
                ExportFileInfo fileInfo = this.business.DownloadDataCustomer(condition);
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
        #endregion
    }
}