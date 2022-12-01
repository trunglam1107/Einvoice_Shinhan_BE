using Hangfire.Dashboard;

namespace InvoiceServer.API.MessageHandler
{
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public MyAuthorizationFilter(params string[] roles)
        {
        }

        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}