using InvoiceServer.Common;
using Newtonsoft.Json;

namespace InvoiceServer.Business.Models
{
    public class ImportRowError
    {
        [JsonProperty("errorCode", NullValueHandling = NullValueHandling.Ignore)]
        public ResultCode ErrorCode { get; set; }

        [JsonProperty("row", NullValueHandling = NullValueHandling.Ignore)]
        public int? Row { get; set; }

        [JsonProperty("column", NullValueHandling = NullValueHandling.Ignore)]
        public string ColumnName { get; set; }

        [JsonProperty("valueOfColumn", NullValueHandling = NullValueHandling.Ignore)]
        public string ValueOfColumn { get; set; }

        public ImportRowError(ResultCode errorCode, string columnName, int? row, string valueOfColumn = null)
        {
            this.ErrorCode = errorCode;
            this.Row = row;
            this.ColumnName = columnName;
            this.ValueOfColumn = valueOfColumn;
        }
    }
}
