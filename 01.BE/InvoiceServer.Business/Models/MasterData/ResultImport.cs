using InvoiceServer.Common;
using InvoiceServer.Common.Utils;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ResultImportSheet
    {
        [JsonProperty("rowError", NullValueHandling = NullValueHandling.Ignore)]
        public ListError<ImportRowError> RowError { get; set; }

        [JsonProperty("rowSuccess", NullValueHandling = NullValueHandling.Ignore)]
        public int? RowSuccess { get; set; }

        [JsonIgnore()]
        public ResultCode ErrorCode { get; set; }

        [JsonIgnore()]
        public string Message { get; set; }

        public ResultImportSheet()
        {
            RowError = new ListError<ImportRowError>();
        }

    }
}
