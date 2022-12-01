using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ResultApproved
    {
        [JsonProperty("rowError", NullValueHandling = NullValueHandling.Ignore)]
        public ListError<ApprovedRowError> RowError { get; set; }

        [JsonIgnore()]
        public ResultCode ErrorCode { get; set; }

        [JsonIgnore()]
        public string Message { get; set; }
        public ResultApproved()
        {
            RowError = new ListError<ApprovedRowError>();
            ErrorCode = ResultCode.NoError;
            Message = MsgApiResponse.ExecuteSeccessful;
        }

    }
}
