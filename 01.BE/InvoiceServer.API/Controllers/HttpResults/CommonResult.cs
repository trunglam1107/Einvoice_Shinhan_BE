using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace InvoiceServer.API.Controllers.Results
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonResult'
    public class CommonResult : IHttpActionResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonResult'
    {
        #region  Fields, Properties

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.StatusCode'
        protected HttpStatusCode StatusCode { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.StatusCode'

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.Request'
        protected HttpRequestMessage Request { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.Request'

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.CommonResult(HttpRequestMessage, HttpStatusCode)'
        public CommonResult(HttpRequestMessage request, HttpStatusCode httpStatusCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.CommonResult(HttpRequestMessage, HttpStatusCode)'
        {
            this.Request = request;
            this.StatusCode = httpStatusCode;
        }

        #endregion

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.CreateResponse()'
        protected virtual HttpResponseMessage CreateResponse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.CreateResponse()'
        {
            return this.Request.CreateResponse(this.StatusCode);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.ExecuteAsync(CancellationToken)'
        public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CommonResult.ExecuteAsync(CancellationToken)'
        {
            var response = this.CreateResponse();
            return Task.FromResult(response);
        }

        #endregion
    }
}