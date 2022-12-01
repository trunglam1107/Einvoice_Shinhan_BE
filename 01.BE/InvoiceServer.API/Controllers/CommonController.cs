using InvoiceServer.API.Business;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.InvoiceSample_Type;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Http;


namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// This class provides APIs which handle user session: login, logout, check session alive
    /// </summary>
    [RoutePrefix("common")]
    public class CommonController : BaseController
    {
        private static readonly Logger logger = new Logger();
        private readonly CurrencyListBusiness currencyBusiness;
        private readonly ClientBusiness clientBusiness;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonController.CommonController()'
        public CommonController()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonController.CommonController()'
        {
            currencyBusiness = new CurrencyListBusiness(GetBOFactory());
            clientBusiness = new ClientBusiness(GetBOFactory());
        }

        [HttpGet]
        [Route("currency-list/{keyword?}/{orderType?}/{orderby?}/{skip?}/{take?}")]
        [AllowAnonymous]

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonController.CurrencyList(string, string, string, int, int)'
        public IHttpActionResult CurrencyList(string keyword = null, string orderType = null, string orderby = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonController.CurrencyList(string, string, string, int, int)'
        {
            var response = new ApiResultList<CurrencyListViewModel>();
            try
            {
                var condition = new ConditionSearchInvoiceType(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    Key = keyword.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Data = currencyBusiness.Fillter(condition);
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
        [Route("client-list-suggestion/{clientName?}/{taxCode?}/{customerCode?}/{useTotalInvoice?}/{taxIncentives?}/{sendInvoiceByMonth?}/{skip?}/{take?}/{isPersonal?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonController.ClientListSuggestion(string, string, string, bool?, bool?, bool?, string, string, int, int, string)'
        public IHttpActionResult ClientListSuggestion(string clientName = null, string taxCode = null, string customerCode = null, bool? useTotalInvoice = null, bool? taxIncentives = null, bool? sendInvoiceByMonth = null,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonController.ClientListSuggestion(string, string, string, bool?, bool?, bool?, string, string, int, int, string)'
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
                response.Data = clientBusiness.ClientListSuggestion(condition);
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
        [Route("client-list-suggestion-by-CIF/{customerCode?}/{isPersonal?}/{skip?}/{take?}")]
        [AllowAnonymous]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonController.FillterClientByCIF(string, string, string, string, int, int)'
        public IHttpActionResult FillterClientByCIF(string customerCode = null, string isPersonal = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonController.FillterClientByCIF(string, string, string, string, int, int)'
        {
            var response = new ApiResultList<ClientDetail>();
            try
            {
                var condition = new ConditionSearchClient(orderby, orderType)
                {
                    CurrentUser = this.CurrentUser,
                    Skip = skip,
                    Take = take,
                    CustomerCode = customerCode.DecodeUrl(),
                    IsPersonal = isPersonal.DecodeUrl(),
                };
                response.Code = ResultCode.NoError;
                response.Data = clientBusiness.FillterClientByCIF(condition);
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
    }
}