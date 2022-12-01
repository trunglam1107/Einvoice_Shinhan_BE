using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
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
    [RoutePrefix("clients")]
    public class ClientController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ClientBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.ClientController()'
        public ClientController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.ClientController()'
        {
            business = new ClientBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpGet]
        [Route("{clientName?}/{taxCode?}/{customerCode?}/{useTotalInvoice?}/{taxIncentives?}/{sendInvoiceByMonth?}/{skip?}/{take?}/{isPersonal?}")]
        [CustomAuthorize(Roles = UserPermission.ClientManagement_Read + ", " + UserPermission.CreateInvoice_Create
            + ", " + UserPermission.InvoiceAdjustment_Create + ", " + UserPermission.SubstituteInvoice_Create
            + ", " + UserPermission.InvoiceManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.FillterClient(string, string, string, bool?, bool?, bool?, string, string, int, int, string)'
        public IHttpActionResult FillterClient(string clientName = null, string taxCode = null, string customerCode = null, bool? useTotalInvoice = null, bool? taxIncentives = null, bool? sendInvoiceByMonth = null,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.FillterClient(string, string, string, bool?, bool?, bool?, string, string, int, int, string)'
            string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue, string isPersonal = null)
        {
            var response = new ApiResultList<ClientDetail>();
            try
            {
                var condition = new ConditionSearchClient(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    TaxCode = taxCode.DecodeUrl(),
                    ClientName = clientName.DecodeUrl(),
                    CustomerCode = customerCode.DecodeUrl(),
                    UseTotalInvoice = useTotalInvoice,
                    TaxIncentives = taxIncentives,
                    SendInvoiceByMonth = sendInvoiceByMonth,
                    IsPersonal = isPersonal.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FillterClient(condition);
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
        [CustomAuthorize(Roles = UserPermission.ClientManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.GetClientInfo(long)'
        public IHttpActionResult GetClientInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.GetClientInfo(long)'
        {
            if (!ModelState.IsValid)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult<ClientAddInfo>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                response.Data = this.business.GetClientDetail(id);
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
        [CustomAuthorize(Roles = UserPermission.ClientManagement_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.MyClientUsing(long)'
        public IHttpActionResult MyClientUsing(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.MyClientUsing(long)'
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
                response.Data = this.business.MyClientUsing(id);
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
        [CustomAuthorize(Roles = UserPermission.ClientManagement_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.CreateClient(ClientAddInfo)'
        public IHttpActionResult CreateClient(ClientAddInfo clientInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.CreateClient(ClientAddInfo)'
        {
            if (!ModelState.IsValid || clientInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();

            try
            {
                response.Code = this.business.Create(clientInfo);
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
        [CustomAuthorize(Roles = UserPermission.ClientManagement_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.UpdateClient(long, ClientAddInfo)'
        public IHttpActionResult UpdateClient(long id, ClientAddInfo clientInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.UpdateClient(long, ClientAddInfo)'
        {
            if (!ModelState.IsValid || clientInfo == null)
            {
                return Error(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var response = new ApiResult();
            try
            {
                response.Code = this.business.Update(id, clientInfo);
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
        [CustomAuthorize(Roles = UserPermission.ClientManagement_Delete)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.DeleteClient(long)'
        public IHttpActionResult DeleteClient(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.DeleteClient(long)'
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
        [CustomAuthorize(Roles = UserPermission.UploadClient_Create)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.UploadData(FileImport)'
        public IHttpActionResult UploadData(FileImport fileImport)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.UploadData(FileImport)'
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
        [Route("download/{clientName?}/{taxCode?}/{customerCode?}/{useTotalInvoice?}/{taxIncentives?}/{sendInvoiceByMonth?}")]
        [ResponseType(typeof(InvoiceFile))]
        //[CustomAuthorize(Roles = UserPermission.UploadProduct_Read + ", " + UserPermission.ProductManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.DownloadData(string, string, string, bool?, bool?, bool?)'
        public IHttpActionResult DownloadData(string clientName = null, string taxCode = null, string customerCode = null, bool? useTotalInvoice = null, bool? taxIncentives = null, bool? sendInvoiceByMonth = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.DownloadData(string, string, string, bool?, bool?, bool?)'
        {
            try
            {
                var condition = new ConditionSearchClient(null, null)
                {
                    CurrentUser = this.CurrentUser,
                    OrderType = "ASC",
                    //ColumnOrder = clientName.DecodeUrl(),
                    ClientName = clientName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    CustomerCode = customerCode.DecodeUrl(),
                    UseTotalInvoice = useTotalInvoice,
                    TaxIncentives = taxIncentives,
                    SendInvoiceByMonth = sendInvoiceByMonth,
                };
                ExportFileInfo fileInfo = this.business.DownloadDataClients(condition);
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
        [Route("ExportExcel/{branch?}/{userid?}/{branchId?}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.ExportExcel(string, string, string)'
        public IHttpActionResult ExportExcel(string branch = null, string keyword = null,string branchId = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.ExportExcel(string, string, string)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.ExportExcel(branch, keyword, branchId);
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
        [Route("importClient")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ClientController.autoImport()'
        public IHttpActionResult autoImport()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ClientController.autoImport()'
        {
            var response = new ApiResult<ResultImportSheet>();

            try
            {
                response.Code = this.business.ImportClient();
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