using InvoiceServer.Business.Utils;
using InvoiceServer.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InvoiceServer.Business.Models
{
    public abstract class ApiResultBase
    {
        [JsonProperty("code")]
        [JsonConverter(typeof(JsonEnumConverter<ResultCode>))]
        public ResultCode Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        protected ApiResultBase() { }

        protected ApiResultBase(ResultCode code, string message)
        {
            this.Code = code;
            this.Message = message;
        }
    }
    public class ApiResult : ApiResultBase
    {
#pragma warning disable 0169, 0414
        [JsonProperty("data")]
        private readonly object data;
#pragma warning restore 0169, 0414

        public ApiResult()
            : base()
        {
        }

        public ApiResult(ResultCode code, string message)
            : base(code, message)
        {
        }
    }

    public class ApiResult<T> : ApiResultBase
        where T : class
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        public ApiResult()
            : base()
        {

        }
        public ApiResult(ResultCode code, string message, T data)
            : base(code, message)
        {
            this.Data = data;
        }

        #region Will be deleted
        public ApiResult(ResultCode code, string message)
            : base(code, message)
        {
            this.Code = code;
            this.Message = message;
        }

        public void SetInfo(ResultCode code, string message, T data)
        {
            Code = code;
            Message = message;
            this.Data = data;
        }


        public void SetInfo(ResultCode code, string message)
        {
            Code = code;
            Message = message;
        }
        #endregion

    }

    public class ApiResultList<T> : ApiResultBase
        where T : class
    {
        [JsonProperty("data")]
        public IEnumerable<T> Data { get; set; }

        public ApiResultList()
            : base()
        {

        }
        public ApiResultList(ResultCode code, string message, IEnumerable<T> data)
            : base(code, message)
        {
            this.Data = data;
        }
    }
}
