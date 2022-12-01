using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.GatewayLog;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace InvoiceServer.API.Controllers
{
    [RoutePrefix("gatewaylogs")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogController'
    public class GatewaylogController : BaseController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogController'
    {
        #region Fields, Properties

        private static readonly Logger logger = new Logger();
        private readonly GatewaylogBusiness business;

        #endregion // #region Fields, Properties

        #region Contructor
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogController.GatewaylogController()'
        public GatewaylogController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogController.GatewaylogController()'
        {
            business = new GatewaylogBusiness(GetBOFactory());
        }

        #endregion Contructor

        #region API methods

        [HttpGet]
        [Route("{name?}/{dateFrom?}/{dateTo?}/{skip?}/{take?}")]
        [CustomAuthorize(Roles = UserPermission.SystemLogTVan_Read )]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogController.FillterClient(string, string, string, string, string, int, int, string)'
        public IHttpActionResult FillterClient(string name = null, string dateFrom = null, string dateTo = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue, string isPersonal = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogController.FillterClient(string, string, string, string, string, int, int, string)'
        {
            var response = new ApiResultList<GatewaylogDetail>();
            try
            {
                var condition = new ConditionSearchGatewaylog(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    GatewaylogName = name.DecodeUrl(),
                    DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                    DateTo = dateTo.DecodeUrl().ConvertDateTime(),
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
        [Route("downloadExcel/{name?}/{dateFrom?}/{dateTo?}/{companyId?}/{language?}")]
        [CustomAuthorize(Roles = UserPermission.SystemLogTVan_Read)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogController.ReportHistoryTVan(string, string, string, long, string)'
        public IHttpActionResult ReportHistoryTVan(string name = null, string dateFrom = null, string dateTo = null,long companyId = 0,string language = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogController.ReportHistoryTVan(string, string, string, long, string)'
        {
            try
            {
                var condition = new ConditionSearchGatewaylog();
                if (name == "null") 
                {
                    condition = new ConditionSearchGatewaylog()
                    {
                        CurrentUser = this.CurrentUser,
                        Skip = 0,
                        Take = int.MaxValue,
                        GatewaylogName = null,
                        DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                        DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                        companyId = companyId,
                        language = language
                    };
                }
                else
                {
                    condition = new ConditionSearchGatewaylog()
                    {
                        CurrentUser = this.CurrentUser,
                        Skip = 0,
                        Take = int.MaxValue,
                        GatewaylogName = name.DecodeUrl(),
                        DateFrom = dateFrom.DecodeUrl().ConvertDateTime(),
                        DateTo = dateTo.DecodeUrl().ConvertDateTime(),
                        companyId = companyId,
                        language = language
                    };
                }
                

                ExportFileInfo fileInfo = this.business.ReportHistoryTVan(condition);
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
            catch(Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
                return Error(HttpStatusCode.InternalServerError, "Error occured while read file");
            }
        }
        #endregion API methods
    }
}
