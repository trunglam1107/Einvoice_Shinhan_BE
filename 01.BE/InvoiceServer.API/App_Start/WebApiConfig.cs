using InvoiceServer.API.Config;
using InvoiceServer.API.MediaFormatter;
using InvoiceServer.API.MessageHandler;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace InvoiceServer.API
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WebApiConfig'
    public static class WebApiConfig
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WebApiConfig'
    {
        /// <summary>
        /// // Web API configuration and services
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            InitMessageHandlers(config);
            InitMediaFormatters(config);

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static void InitMessageHandlers(HttpConfiguration config)
        {
            if (SystemConfig.Instance.EnableCompressResponse)
            {
                // Refer from http://benfoster.io/blog/aspnet-web-api-compression
                config.MessageHandlers.Insert(0, new CompressionHandler()); // must set first
            }

            // Web Authentication
            config.MessageHandlers.Add(new AuthenticationHandler());
            config.MessageHandlers.Add(new OperationLogHandler());
        }

        private static void InitMediaFormatters(HttpConfiguration config)
        {
            //remove any default formatters such as xml
            config.Formatters.Clear();
            config.Formatters.Add(new ZipFileFormatter());
            config.Formatters.Add(new XmlFileFormatter());
            config.Formatters.Add(new PdfFileFormatter());
            config.Formatters.Add(new XlsFileFormatter());
            //order of formatters matter!
            //let's put JSON in as the default first
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.Add(new XmlMediaTypeFormatter());
        }
    }
}