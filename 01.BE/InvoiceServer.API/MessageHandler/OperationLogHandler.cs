using InvoiceServer.API.Business;
using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceServer.API.MessageHandler
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'OperationLogHandler'
    public class OperationLogHandler : DelegatingHandler
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'OperationLogHandler'
    {
        class ApiInfo : IEquatable<ApiInfo>
        {
            #region Fields, Properties

            private readonly string _requestUrl;
            private readonly string _httpMethod;

            #endregion

            #region Contructor

            public ApiInfo(string urlPattern, HttpMethod httpMethod)
            {
                this._requestUrl = urlPattern;
                this._httpMethod = httpMethod != null ? httpMethod.Method.ToUpperInvariant() : string.Empty;
            }

            #endregion

            #region Methods

            public bool Equals(ApiInfo other)
            {
                return other._requestUrl.IndexOf(this._requestUrl) > -1 && Utility.Equals(this._httpMethod, other._httpMethod);
            }

            public bool IsMatch(ApiInfo other)
            {
                if (this.Equals(other))
                {
                    return true;
                }

                bool matchUrl = true;
                if (!this._requestUrl.Equals(FilterPattern.AllValues))
                {
                    matchUrl = other._requestUrl.IndexOf(this._requestUrl) > -1;
                }
                bool matchMethod = true;
                if (!this._httpMethod.Equals(FilterPattern.AllValues))
                {
                    matchMethod = Utility.Equals(other._httpMethod, this._httpMethod);
                }

                return matchUrl && matchMethod;
            }

            #endregion
        }

        #region Fields, Properties

        private static readonly Logger _logger = new Logger(LogTypes.UserAction);
        private static readonly List<ApiInfo> _processApi = new List<ApiInfo>()
        {
            new ApiInfo(FilterPattern.AllValues, HttpMethod.Post),
            new ApiInfo(FilterPattern.AllValues, HttpMethod.Delete),
        };
        private static readonly List<ApiInfo> _ignoreApi = new List<ApiInfo>()
        {
            //new ApiInfo("/sessions", FilterPattern.AllValues),
            new ApiInfo("/sessions", HttpMethod.Get),
            new ApiInfo("/upload", HttpMethod.Post),     // Upload .zip file of XHTML data
            new ApiInfo("/history-log", HttpMethod.Post),
        };

        /// <summary>
        /// Update URL will be in format {prefix}/{id}
        /// </summary>
        private static readonly Regex _updateUrlFormat = new Regex(@"\/\d+\z");

        private readonly SessionBusiness sessionBusiness;

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'OperationLogHandler.OperationLogHandler()'
        public OperationLogHandler()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'OperationLogHandler.OperationLogHandler()'
        {
            sessionBusiness = new SessionBusiness(new BOFactory(DbContextManager.GetContext()));
        }

        #endregion

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'OperationLogHandler.SendAsync(HttpRequestMessage, CancellationToken)'
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'OperationLogHandler.SendAsync(HttpRequestMessage, CancellationToken)'
        {
            try
            {
                if (!Ignore(request))
                {
                    string token = HttpUtils.GetRequestHeaderValue(request, CustomHttpRequestHeader.AuthorizationToken);
                    if (token.IsNotNullOrEmpty() &&
                        this.sessionBusiness.CheckUserSession(token) == ResultCode.SessionAlive)
                    {
                        UserSessionInfo user = this.sessionBusiness.GetUserSession(token);
                        WriteLog(user.UserId, request);
                    }
                    else
                    {
                        WriteLog(string.Empty, request);
                    }
                }
            }
            catch
            {
                // Do nothing
            }

            return base.SendAsync(request, cancellationToken);
        }

        private static bool Ignore(HttpRequestMessage requestMessage)
        {
            var requestApi = new ApiInfo(requestMessage.RequestUri.LocalPath, requestMessage.Method);

            foreach (var api in _ignoreApi)
            {
                if (api.IsMatch(requestApi))
                    return true;
            }

            foreach (var api in _processApi)
            {
                if (api.IsMatch(requestApi))
                    return false;
            }

            return true;
        }

        private static string GetActionType(HttpRequestMessage requestMessage)
        {
            HttpMethod method = requestMessage.Method;
            string url = requestMessage.RequestUri.LocalPath;

            string action = method.Method;
            if (method == HttpMethod.Post)
            {
                if (_updateUrlFormat.Match(url).Success)
                {
                    action = "UPDATE";
                }
                else
                {
                    action = "INSERT";
                }
            }

            return action.ToUpperInvariant();
        }

        private static void WriteLog(string userId, HttpRequestMessage requestMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(requestMessage.RequestUri.LocalPath);
            if (requestMessage.Content != null)
            {
                string content = requestMessage.Content.ReadAsStringAsync().Result;
                if (content.IsNotNullOrEmpty())
                {
                    sb.Append(" ");
                    sb.Append(content);
                }
            }

            string action = GetActionType(requestMessage);
            _logger.UserAction(userId, action, sb.ToString());
        }

        #endregion
    }
}

