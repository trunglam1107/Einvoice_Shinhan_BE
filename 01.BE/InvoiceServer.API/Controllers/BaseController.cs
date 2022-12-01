using InvoiceServer.API.Business;
using InvoiceServer.API.Controllers.Results;
using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Http;
using InvoiceServer.GateWay.Services.ServiceFactorys;

namespace InvoiceServer.API.Controllers
{
    /// <summary>
    /// The abstract base class for all controllers
    /// </summary>
    [Authorize]
    public abstract class BaseController : ApiController
    {
        #region  Fields, Properties
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.MsgInternalServerError'
        protected const string MsgInternalServerError = "An error occurred in server.";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.MsgInternalServerError'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.MsgRequestDataInvalid'
        protected const string MsgRequestDataInvalid = "The request data is invalid.";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.MsgRequestDataInvalid'

        private UserSessionInfo currentUser;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.CurrentUser'
        public UserSessionInfo CurrentUser
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.CurrentUser'
        {
            get
            {
                if (this.currentUser == null)
                {
                    this.currentUser = GetCurrentUser();
                }

                return this.currentUser != null ? this.currentUser : new UserSessionInfo();
            }
        }

        private readonly IDbContext dbContext = DbContextManager.GetContext();

        #endregion

        #region Base methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetBOFactory()'
        protected IBOFactory GetBOFactory()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetBOFactory()'
        {
            return new BOFactory(this.dbContext);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetBOFactory(string)'
        protected IBOFactory GetBOFactory(string nameOrSqlConnectionString)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetBOFactory(string)'
        {
            return new BOFactory(DbContextManager.GetContext(nameOrSqlConnectionString));
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetServiceFactory()'
        protected IServiceFactory GetServiceFactory()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetServiceFactory()'
        {
            return new ServiceFactory(this.dbContext);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetAuthenticatedToken()'
        protected string GetAuthenticatedToken()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetAuthenticatedToken()'
        {
            return HttpUtils.GetRequestHeaderValue(this.Request, CustomHttpRequestHeader.AuthorizationToken);
        }

        #endregion

        #region Customize IHttpActionResult

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.NoContent(HttpStatusCode)'
        protected IHttpActionResult NoContent(HttpStatusCode code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.NoContent(HttpStatusCode)'
        {
            return new CommonResult(this.Request, code);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.Success<T>(HttpStatusCode, T)'
        protected IHttpActionResult Success<T>(HttpStatusCode statusCode, T t) where T : class
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.Success<T>(HttpStatusCode, T)'
        {
            return new SuccessResult<T>(this.Request, statusCode, t);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.Error(ResultCode, string)'
        protected IHttpActionResult Error(ResultCode code, string message)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.Error(ResultCode, string)'
        {
            return Ok(new ApiResult(code, message));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.Error(HttpStatusCode, string, IDictionary<string, object>)'
        protected IHttpActionResult Error(HttpStatusCode statusCode, string message, IDictionary<string, object> data = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.Error(HttpStatusCode, string, IDictionary<string, object>)'
        {
            return new ErrorResult(this.Request, statusCode, message, data);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.Text(HttpStatusCode, string, params object[])'
        protected IHttpActionResult Text(HttpStatusCode statusCode, string message, params object[] args)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.Text(HttpStatusCode, string, params object[])'
        {
            return new TextResult(this.Request, statusCode, message, args);
        }
        #endregion

        #region Static methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.SetResponseHeaders(Dictionary<string, string>)'
        protected static void SetResponseHeaders(Dictionary<string, string> responseHeaders)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.SetResponseHeaders(Dictionary<string, string>)'
        {
            foreach (var kvp in responseHeaders)
            {
                HttpContext.Current.Response.AppendHeader(kvp.Key, kvp.Value);
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.SetResponseHeaders(string, string)'
        protected static void SetResponseHeaders(string key, string value)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.SetResponseHeaders(string, string)'
        {
            HttpContext.Current.Response.AppendHeader(key, value);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetEmptyList<T>()'
        protected static IEnumerable<T> GetEmptyList<T>()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'BaseController.GetEmptyList<T>()'
        {
            return new List<T>();
        }

        #endregion

        #region Private methods

        private UserSessionInfo GetCurrentUser()
        {
            var sessionBusiness = new SessionBusiness(this.GetBOFactory());
            string token = string.Empty;
            if (Request != null)
            {
                token = GetAuthenticatedToken();
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                token = "quarzt";
            }

            return sessionBusiness.GetUserSession(token);
        }

        #endregion

    }
}