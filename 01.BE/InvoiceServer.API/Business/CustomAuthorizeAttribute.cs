using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace InvoiceServer.API.Business
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomAuthorizeAttribute'
    public class CustomAuthorizeAttribute : AuthorizeAttribute
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomAuthorizeAttribute'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CustomAuthorizeAttribute.HandleUnauthorizedRequest(HttpActionContext)'
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CustomAuthorizeAttribute.HandleUnauthorizedRequest(HttpActionContext)'
        {
            if (!actionContext.RequestContext.Principal.Identity.IsAuthenticated)
                base.HandleUnauthorizedRequest(actionContext);
            else
            {
                // Authenticated, but not AUTHORIZED.  Return 403 instead!
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
            }
        }
    }
}