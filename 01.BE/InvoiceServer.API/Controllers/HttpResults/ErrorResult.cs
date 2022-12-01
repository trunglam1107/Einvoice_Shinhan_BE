using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace InvoiceServer.API.Controllers.Results
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult'
    public class ErrorResult : CommonResult
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult'
    {
        #region  Fields, Properties

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.Message'
        public string Message { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.Message'
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.Data'
        public object Data { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.Data'

        #endregion

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.ErrorResult(HttpRequestMessage, HttpStatusCode, string, IDictionary<string, object>)'
        public ErrorResult(HttpRequestMessage request, HttpStatusCode httpStatusCode, string message, IDictionary<string, object> data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.ErrorResult(HttpRequestMessage, HttpStatusCode, string, IDictionary<string, object>)'
            : base(request, httpStatusCode)
        {
            this.Message = message;
            if (data == null || data.Count == 0)
            {
                this.Data = "";
            }
            else
            {
                this.Data = data;
            }
        }

        #endregion

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.CreateResponse()'
        protected override HttpResponseMessage CreateResponse()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.CreateResponse()'
        {
            var response = base.CreateResponse();
            response.Content = new StringContent(GetJsonContent());

            return response;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.ToString()'
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ErrorResult.ToString()'
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("HttpCode={0},", this.StatusCode)
                .AppendFormat("Content={0}", GetJsonContent())
                .Append("}");

            return sb.ToString();
        }

        private string GetJsonContent()
        {
            try
            {
                var dictonary = new Dictionary<string, object>();
                dictonary.Add("Message", this.Message);
                dictonary.Add("Data", this.Data);

                var content = JsonConvert.SerializeObject(dictonary);

                return content;
            }
            catch
            {
                return this.Message;
            }
        }

        #endregion
    }
}