using InvoiceServer.API.Controllers;
using InvoiceServer.Common;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(InvoiceServer.API.Startup))]

namespace InvoiceServer.API
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup'
    public class Startup
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup'
    {
        readonly Logger logger = new Logger();
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'Startup.Configuration(IAppBuilder)'
        public void Configuration(IAppBuilder app)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'Startup.Configuration(IAppBuilder)'
        {
            InitQuarztJob();
        }
        private void InitQuarztJob()
        {
            logger.QuarztJob(false, "Startup start Jobs");
            QuarztSingleton.Instance.onStart();
            QuarztJobController controller = new QuarztJobController();
            controller.onload();
        }

    }
}
