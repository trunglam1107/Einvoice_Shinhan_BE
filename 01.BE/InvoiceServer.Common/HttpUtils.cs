using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace InvoiceServer.Common
{
    public sealed class HttpUtils
    {
        private HttpUtils()
        {
        }

        public static string ToAbsoluteUri(string relateUri, HttpRequest request)
        {
            if (request == null)
            {
                return relateUri;
            }

            string url = request.Url.GetLeftPart(UriPartial.Authority);

            string absolutePath = VirtualPathUtility.ToAbsolute(relateUri);
            if (!string.IsNullOrEmpty(absolutePath))
            {
                url += absolutePath.StartsWith("/") ? absolutePath : "/" + absolutePath;
            }

            return url;
        }

        public static IEnumerable<string> GetRequestHeaderValues(HttpRequestMessage request, string headerName)
        {
            if (request == null || string.IsNullOrWhiteSpace(headerName))
            {
                return new List<string>();
            }

            IEnumerable<string> headerValues = null;

            if (request.Headers.Contains(headerName))
            {
                request.Headers.TryGetValues(headerName, out headerValues);
            }

            return headerValues;
        }

        public static IEnumerable<string> GetRequestHeaderValues(HttpRequest request, string headerName)
        {
            if (request == null || string.IsNullOrWhiteSpace(headerName))
            {
                return new List<string>();
            }

            IEnumerable<string> headerValues = null;

            if (request.Headers.AllKeys.Contains(headerName, StringComparer.OrdinalIgnoreCase))
            {
                var values = request.Headers.GetValues(headerName);
                headerValues = new List<string>(values);
            }

            return headerValues;
        }

        public static string GetRequestHeaderValue(HttpRequestMessage request, string headerName)
        {
            if (request == null || string.IsNullOrWhiteSpace(headerName))
            {
                return string.Empty;
            }

            IEnumerable<string> headerValues = GetRequestHeaderValues(request, headerName);
            return headerValues != null ? headerValues.FirstOrDefault() : string.Empty;
        }

        public static string GetRequestHeaderValue(HttpRequest request, string headerName)
        {
            if (request == null || string.IsNullOrWhiteSpace(headerName))
            {
                return string.Empty;
            }

            IEnumerable<string> headerValues = GetRequestHeaderValues(request, headerName);
            return headerValues != null ? headerValues.FirstOrDefault() : string.Empty;
        }

        public static string GetClientIpAddress()
        {
            string clientIp = "Unknown IP";
            try
            {
                if (HttpContext.Current != null)
                {
                    var serverVariables = HttpContext.Current.Request.ServerVariables;
                    var headersKeysToCheck = new string[]
                                                {
                                            "HTTP_CLIENT_IP",
                                            "HTTP_X_FORWARDED_FOR",
                                            "HTTP_X_FORWARDED",
                                            "HTTP_X_CLUSTER_CLIENT_IP",
                                            "HTTP_FORWARDED_FOR",
                                            "HTTP_FORWARDED",
                                            "REMOTE_ADDR"
                                                };
                    foreach (var thisHeaderKey in headersKeysToCheck)
                    {
                        var value = serverVariables[thisHeaderKey];
                        if (value != null)
                        {
                            IPAddress validAddress = null;
                            if (IPAddress.TryParse(value, out validAddress))
                            {
                                clientIp = value;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger logger = new Logger();
                logger.Error("eInvoice", ex);
            }
            return clientIp;
        }
    }
}