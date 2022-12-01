using InvoiceServer.API.Business;
using InvoiceServer.API.Business.GateWay;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using InvoiceServer.Common.Extensions;
using InvoiceServer.GateWay.Models.MVan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("release")]
    public class ReleaseInvoiceController : BaseController
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly ReleaseInvoiceBusiness business;
        private readonly MVanBusiness businessTWan;
        #endregion // #region Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.ReleaseInvoiceController()'
        public ReleaseInvoiceController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.ReleaseInvoiceController()'
        {
            business = new ReleaseInvoiceBusiness(GetBOFactory());//thêm para true để by pass
            businessTWan = new MVanBusiness(GetServiceFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpPost]
        [Route("")]
        [CustomAuthorize(Roles = UserPermission.InvoiceApproved_Sign + ", " + UserPermission.BundleRelease_Sign
            + ", " + UserPermission.AnnouncementManagement_Sign)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.Create(ReleaseInvoiceMaster)'
        public IHttpActionResult Create(ReleaseInvoiceMaster invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.Create(ReleaseInvoiceMaster)'
        {
            var response = new ApiResult<Result>();

            try
            {
                response.Code = ResultCode.NoError;
                response.Data = this.business.Create(invoiceInfo);
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
        [Route("group")]
        [CustomAuthorize(Roles = UserPermission.InvoiceApproved_Sign + ", " + UserPermission.BundleRelease_Sign
            + ", " + UserPermission.InvoiceManagement_Sign)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.ReleaseGroupInvoice(ReleaseGroupInvoice)'
        public IHttpActionResult ReleaseGroupInvoice(ReleaseGroupInvoice invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.ReleaseGroupInvoice(ReleaseGroupInvoice)'
        {
            var response = new ApiResult<Result>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = this.business.ReleaseGroupInvoice(invoiceInfo);
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
        [Route("{code?}/{invoiceSymbolId?}/{invoiceStatus?}/{invoiceNo?}/{customerName?}/{taxCode?}/{invoiceTypeId?}/{numberAccout?}/{dateFrom?}/{dateTo?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.Fillter(int?, long?, int?, string, string, string, long?, string, string, string, string, string, int, int)'
        public IHttpActionResult Fillter(int? code = null, long? invoiceSymbolId = null, int? invoiceStatus = null, string invoiceNo = null, string customerName = null, string taxCode = null, long? invoiceTypeId = null, string numberAccout = null, string dateFrom = null, string dateTo = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.Fillter(int?, long?, int?, string, string, string, long?, string, string, string, string, string, int, int)'
        {
            var response = new ApiResultList<ReleaseInvoices>();
            try
            {
                var condition = new ConditionSearchReleaseInvoice(orderBy, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    InvoiceSampleId = code,
                    InvoiceSymbol = invoiceSymbolId,
                    InvoiceStatus = invoiceStatus,
                    InvoiceNo = invoiceNo.DecodeUrl(),
                    CustomerName = customerName.DecodeUrl(),
                    TaxCode = taxCode.DecodeUrl(),
                    InvoiceSample = invoiceTypeId,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime()
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
        [Route("list/{status?}/{dateFrom?}/{dateTo?}/{orderBy?}/{orderType?}/{skip?}/{take?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.FillterReleaseList(int?, string, string, string, string, int, int)'
        public IHttpActionResult FillterReleaseList(int? status = null, string dateFrom = null, string dateTo = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.FillterReleaseList(int?, string, string, string, string, int, int)'
        {
            var response = new ApiResultList<ReleaseListInvoice>();
            try
            {
                var condition = new ConditionSearchReleaseList(orderBy, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Status = status,
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                };
                response.Code = ResultCode.NoError;
                response.Data = business.FilterReleaseInvoice(condition);
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
        [Route("list-invoice-detail/{releaseId?}")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.FillterReleaseList(long?, string, string, int, int)'
        public IHttpActionResult FillterReleaseList(long? releaseId = null, string orderBy = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.FillterReleaseList(long?, string, string, int, int)'
        {
            var response = new ApiResultList<InvoicesReleaseDetail>();
            try
            {
                long totalRecords = 0;
                response.Code = ResultCode.NoError;
                response.Data = business.ReleaseInvoiceDetail(out totalRecords, releaseId, orderBy, orderType, skip, take);
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
        [Route("file-invoices/{id}")]
        [ResponseType(typeof(InvoiceFile))]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.FileInvoices(long)'
        public IHttpActionResult FileInvoices(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.FileInvoices(long)'
        {
            try
            {
                ExportFileInfo fileInfo = this.business.FileInvoices(id);
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
        [Route("{id}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.GetFileInvoiceRelease(long)'
        public IHttpActionResult GetFileInvoiceRelease(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.GetFileInvoiceRelease(long)'
        {
            var response = new ApiResultList<InvoicesRelease>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Data = business.GetReleaseInvoices(id);
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
        [Route("upload-release-invoices/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.UploadReleaseInvoices(long)'
        public IHttpActionResult UploadReleaseInvoices(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.UploadReleaseInvoices(long)'
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
                        var listInvoices = this.business.GetInvoiceByReleaseId(id);
                        string sourceFileStandard = uploadFileBusiness.StandardFileUpload(sourceFile, listInvoices);
                        this.business.UpdataReleaseStatus(id, sourceFileStandard);
                        this.business.DeleteFileAfterSign(id);
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
        [Route("client-release-invoices/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.ClientReleaseInvoices(long)'
        public IHttpActionResult ClientReleaseInvoices(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.ClientReleaseInvoices(long)'
        {
            ResultCode resultCode;
            try
            {
                var httpRequest = HttpContext.Current.Request;
                var fileUploadInfo = FileUploadInfo.FromRequest(httpRequest);

                UploadFileBusiness uploadFileBusiness = new UploadFileBusiness();
                if (uploadFileBusiness.IsValidFile(fileUploadInfo))
                {
                    var invoice = this.business.GetInvoiceById(id);
                    var releaseInvoice = this.business.GetCompanyOfInvoice(id);
                    long companyIdOfInvoice = releaseInvoice.COMPANYID.Value;
                    long releaseId = releaseInvoice.ID;
                    var sourceFile = uploadFileBusiness.CreateFilePathByDocId(releaseId, companyIdOfInvoice, fileUploadInfo);
                    resultCode = uploadFileBusiness.SaveFile(fileUploadInfo, sourceFile);
                    if (resultCode == ResultCode.NoError)
                    {
                        uploadFileBusiness.StandardFileUpload(sourceFile, companyIdOfInvoice, invoice, 1);
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
        [Route("update-status/{id}")]
        //[CustomAuthorize(Roles = UserPermission.MyCompany_Update)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.UpdateStatus(long, StatusReleaseInvoice)'
        public IHttpActionResult UpdateStatus(long id, StatusReleaseInvoice statusRelease)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.UpdateStatus(long, StatusReleaseInvoice)'
        {
            var response = new ApiResult<Result>();

            try
            {
                response.Code = this.business.UpdataStatusReleaseDetailInvoice(id, statusRelease);
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
        [Route("serverSign")]
        //[CustomAuthorize(Roles = UserPermission.InvoiceManagement_Read)]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.ServerSign(ReleaseInvoiceMaster)'
        public IHttpActionResult ServerSign(ReleaseInvoiceMaster invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.ServerSign(ReleaseInvoiceMaster)'
        {
            var response = new ApiResult<ServerSignResult>();
            try
            {
                response.Code = ResultCode.NoError;
                response.Message = MsgApiResponse.ExecuteSeccessful;
                var serverSignResult = this.business.SignFile(invoiceInfo);
                if (invoiceInfo.ReleaseInvoiceInfos.Count() > 0)
                {
                    foreach (var item in invoiceInfo.ReleaseInvoiceInfos)
                    {
                        var invoice = this.business.GetInvoicenfo(item.InvoiceId);
                        if (invoice.HTHDon == HTHDon.KhongCoMa)
                        {
                            response.Data = serverSignResult;
                        }
                        else
                        {

                            var result = SendTwanInvoice(item.InvoiceId);
                            serverSignResult.StatusTwan = result.StatusTwan;
                            serverSignResult.MessageError = result.MessageError;
                            response.Data = serverSignResult;
                        }
                    }

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

        #endregion API methods
        #region send twan  with code 
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.SendTwanInvoice(long)'
        public ServerSignResult SendTwanInvoice(long invoiceId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReleaseInvoiceController.SendTwanInvoice(long)'
        {

            var response = new ServerSignResult();
            var invoice = this.business.GetInvoicenfo(invoiceId);

            if (invoice.InvoiceStatus == (int)InvoiceStatus.Released)
            {
                var base64File = Convert.ToBase64String(File.ReadAllBytes(this.business.getNameXml(invoiceId)));

                VanDefaulRequest vanDefaulRequest = new VanDefaulRequest()
                {
                    InvoiceId = invoiceId,
                    XmlData = base64File
                };
                logger.Error(null, null, "Co ma");
                if (invoice.HTHDon == HTHDon.CoMa)
                {
                    logger.Error(null, null, "Co ma");
                    VanDefaulResult vanDefaulResult = this.businessTWan.SendInvoiceWithCode(vanDefaulRequest);

                }

                var messageInvoice = this.business.IscheckResponeCQT(invoiceId);
                if (messageInvoice != null)
                {
                    if (messageInvoice.statusCQT != 1)
                    {
                        response.MessageError = messageInvoice.MessageError;
                        response.StatusTwan = messageInvoice.statusCQT;


                        this.business.UpdataInvoiceStatus(invoice, (int)InvoiceStatus.New);


                    }
                    else
                    {
                        this.business.UpdataInvoiceStatus(invoice, (int)InvoiceStatus.TwanSccess);
                        response.StatusTwan = messageInvoice.statusCQT;
                    }
                }

            }


            return response;



        }
        #endregion
    }
}