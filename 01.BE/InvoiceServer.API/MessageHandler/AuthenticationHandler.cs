using InvoiceServer.API.Business;
using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace InvoiceServer.API.MessageHandler
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AuthenticationHandler'
    public class AuthenticationHandler : DelegatingHandler
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AuthenticationHandler'
    {
        class ByPassApi : IEquatable<ByPassApi>
        {
            #region Fields, Properties

            public readonly string LocalRequestUrl;
            public readonly HttpMethod HttpMethod;

            #endregion

            #region Contructor

            public ByPassApi(string url, HttpMethod httpMethod)
            {
                this.LocalRequestUrl = url;
                this.HttpMethod = httpMethod;
            }

            #endregion

            #region Methods

            public bool Equals(ByPassApi other)
            {
                return other.LocalRequestUrl.IndexOf(this.LocalRequestUrl) > -1 && this.HttpMethod.Equals(other.HttpMethod);
            }

            #endregion
        }

        #region Fields, Properties

        private readonly SessionBusiness sessionBusiness;
        private static readonly Logger logger = new Logger();
        private static readonly List<ByPassApi> listByPassApi = new List<ByPassApi>()
        {
            new ByPassApi("/login", HttpMethod.Post),
            new ByPassApi("/client-login", HttpMethod.Post),
            new ByPassApi("/reset-password", HttpMethod.Post),
            new ByPassApi("/session", HttpMethod.Get),
            new ByPassApi("/hangfire", HttpMethod.Get),
            new ByPassApi("/favicon.ico", HttpMethod.Get),
            new ByPassApi("/InvoicePortal", HttpMethod.Get),
            new ByPassApi("/InvoicePortal/update-client-info", HttpMethod.Post),
            new ByPassApi("/InvoicePortal/send-email-client-account-info", HttpMethod.Post),
            new ByPassApi("/InvoicePortal/change-password", HttpMethod.Post),
            new ByPassApi("/InvoicePortal/reset-password", HttpMethod.Post),
            new ByPassApi("/InvoicePortal/Receivedata", HttpMethod.Post),
            new ByPassApi("/InvoicePortal/customer-index", HttpMethod.Get),
            new ByPassApi("/InvoicePortal-dow", HttpMethod.Get),
            new ByPassApi("/VerifyXml", HttpMethod.Post),
            new ByPassApi("/service/sendbymonth", HttpMethod.Get),
            new ByPassApi("/ServiceJob/Approve", HttpMethod.Get),
            new ByPassApi("/service/createInvoice", HttpMethod.Post),
            new ByPassApi("/releaseAnnouncement/client-release-announcements", HttpMethod.Post),
            new ByPassApi("/mVan", HttpMethod.Post),
            new ByPassApi("/mVan", HttpMethod.Get),
            //new ByPassApi("/v1/sessions", HttpMethod.Options),
            //new ByPassApi("/v1/document-images", HttpMethod.Get),
            //new ByPassApi("/v1/document-images", HttpMethod.Options),
            //new ByPassApi("/v1/document-reports", HttpMethod.Get),
            //new ByPassApi("/v1/document-reports", HttpMethod.Options),
        };

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AuthenticationHandler.AuthenticationHandler()'
        public AuthenticationHandler()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AuthenticationHandler.AuthenticationHandler()'
        {
            sessionBusiness = new SessionBusiness(new BOFactory(DbContextManager.GetContext()));
        }

        #endregion

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AuthenticationHandler.SendAsync(HttpRequestMessage, CancellationToken)'
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AuthenticationHandler.SendAsync(HttpRequestMessage, CancellationToken)'
        {
            try
            {
                if (CheckAuthorize(request))
                {
                    string token = HttpUtils.GetRequestHeaderValue(request, CustomHttpRequestHeader.AuthorizationToken);
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        return AuthenticationResult(HttpStatusCode.BadRequest);
                    }

                    ResultCode resultCode = this.sessionBusiness.CheckUserSession(token);

                    if (resultCode != ResultCode.SessionAlive)
                    {
                        return AuthenticationResult(HttpStatusCode.Unauthorized);
                    }

                    UserSessionInfo userInfo = this.sessionBusiness.GetUserSession(token);
                    if (userInfo == null)
                    {
                        return AuthenticationResult(HttpStatusCode.Unauthorized);
                    }

                    IPrincipal principal = new GenericPrincipal(new GenericIdentity(userInfo.UserId), userInfo.RoleUser.Permissions.ToArray());
                    Thread.CurrentPrincipal = principal;
                    HttpContext.Current.User = principal;
                }

                return base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An unknown error occurred.");
                return AuthenticationResult(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Authentication result client call API
        /// </summary>
        /// <param name="httpcode"></param>
        /// <returns></returns>
        private static Task<HttpResponseMessage> AuthenticationResult(HttpStatusCode httpcode)
        {
            var httpResponseMessage = new HttpResponseMessage(httpcode);
            var taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();
            taskCompletionSource.SetResult(httpResponseMessage);
            return taskCompletionSource.Task;
        }

        private static bool CheckAuthorize(HttpRequestMessage requestMessage)
        {
            var requestApi = new ByPassApi(requestMessage.RequestUri.LocalPath, requestMessage.Method);

            return !listByPassApi.Contains(requestApi);
        }

        #endregion
    }
}

