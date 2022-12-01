using InvoiceServer.API.Business;
using InvoiceServer.API.Config;
using InvoiceServer.API.Controllers;
using InvoiceServer.Business.DAO;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace InvoiceServer.API
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WebApiApplication'
    public class WebApiApplication : System.Web.HttpApplication
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WebApiApplication'
    {
        private static readonly Logger logger = new Logger();

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WebApiApplication.Application_Start()'
        protected void Application_Start()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WebApiApplication.Application_Start()'
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            logger.Trace("API server started");
            Httpcontext.Instance.ulti = HttpContext.Current;
            if (ApplicationSetting.Instance.RunQuartzJob == "true")
            {
                QuarztSingleton.Instance.onStart();
                QuarztJobController controller = new QuarztJobController();
                controller.onload();
            }
            SingletonAuthenticationLdap.Instance.RootPath = WebConfigurationManager.AppSettings["LDAPRootPath"];
            _ = DefaultFields.ASSET_FOLDER;  // dùng để khởi tạo giá trị mặc định, không được xóa
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WebApiApplication.Application_End()'
        protected void Application_End()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WebApiApplication.Application_End()'
        {
            logger.Trace("API server stopped");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'WebApiApplication.Application_BeginRequest(object, EventArgs)'
        protected void Application_BeginRequest(object sender, EventArgs e)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'WebApiApplication.Application_BeginRequest(object, EventArgs)'
        {
            if (SystemConfig.Instance.EnableCORS
                && Request.Headers.AllKeys.Contains("Origin", StringComparer.OrdinalIgnoreCase)
                && Utility.Equals(Request.HttpMethod, "OPTIONS"))
            {
                HttpContext.Current.Response.End();
            }
        }

    }
}
