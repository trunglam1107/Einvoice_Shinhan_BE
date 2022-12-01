using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;

namespace InvoiceServer.API.Controllers.Results
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SuccessResult<T>'
    public class SuccessResult<T> : CommonResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SuccessResult<T>'
        where T : class
    {
        #region  Fields, Properties

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SuccessResult<T>.Object'
        public T Object { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SuccessResult<T>.Object'

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SuccessResult<T>.SuccessResult(HttpRequestMessage, HttpStatusCode, T)'
        public SuccessResult(HttpRequestMessage request, HttpStatusCode httpStatusCode, T t)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SuccessResult<T>.SuccessResult(HttpRequestMessage, HttpStatusCode, T)'
            : base(request, httpStatusCode)
        {
            this.Object = t;
        }

        #endregion

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SuccessResult<T>.CreateResponse()'
        protected override HttpResponseMessage CreateResponse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SuccessResult<T>.CreateResponse()'
        {
            var response = base.CreateResponse();
            response.Content = new StringContent(GetJsonContent());

            return response;
        }

        private string GetJsonContent()
        {
            try
            {
                var content = JsonConvert.SerializeObject(this.Object);

                return content;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion
    }
}