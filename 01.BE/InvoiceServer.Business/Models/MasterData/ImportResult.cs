using InvoiceServer.Common;
using InvoiceServer.Common.Utils;

namespace InvoiceServer.Business.Models
{
    public class ImportResult
    {
        public ResultCode ErrorCode { get; set; }
        public string Message { get; set; }
        public ListError<ResultImportSheet> ResultImportSheet { get; set; }

        public ImportResult()
        {
            ResultImportSheet = new ListError<ResultImportSheet>();
        }
    }
}
