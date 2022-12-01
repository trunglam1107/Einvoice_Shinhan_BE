using System.Net;
using System.Net.Http;
using System.Text;

namespace InvoiceServer.API.Controllers.Results
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>'
    public class TextResult<T> : CommonResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>'
    {
        #region  Fields, Properties

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>.Content'
        public T Content { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>.Content'

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>.TextResult(HttpRequestMessage, HttpStatusCode, T)'
        public TextResult(HttpRequestMessage request, HttpStatusCode httpStatusCode, T content)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>.TextResult(HttpRequestMessage, HttpStatusCode, T)'
            : base(request, httpStatusCode)
        {
            this.Content = content;
        }

        #endregion

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>.CreateResponse()'
        protected override HttpResponseMessage CreateResponse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>.CreateResponse()'
        {
            var response = base.CreateResponse();
            string content = string.Empty;
            if (this.Content != null)
            {
                content = this.Content.ToString();
            }
            response.Content = new StringContent(content);
            return response;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>.ToString()'
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TextResult<T>.ToString()'
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("HttpCode={0},", this.StatusCode)
                .AppendFormat("Content={0}", this.Content)
                .Append("}");

            return sb.ToString();
        }

        #endregion
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TextResult'
    public class TextResult : TextResult<string>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TextResult'
    {
        #region  Fields, Properties

        private object[] arguments { get; set; }

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TextResult.TextResult(HttpRequestMessage, HttpStatusCode, string, params object[])'
        public TextResult(HttpRequestMessage request, HttpStatusCode httpStatusCode, string message, params object[] args)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TextResult.TextResult(HttpRequestMessage, HttpStatusCode, string, params object[])'
            : base(request, httpStatusCode, message)
        {
            this.arguments = args;
        }

        #endregion

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TextResult.CreateResponse()'
        protected override HttpResponseMessage CreateResponse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TextResult.CreateResponse()'
        {
            var response = this.Request.CreateResponse(this.StatusCode);
            var message = string.Format(this.Content, this.arguments);

            response.Content = new StringContent(message, System.Text.Encoding.Unicode);

            return response;
        }

        #endregion

    }
}