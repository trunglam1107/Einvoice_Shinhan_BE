using System;

namespace InvoiceServer.Common.Models
{
    public class ImportIndexError
    {
        public int Index { get; set; }
        public Exception Exception { get; set; }
        public string FieldError { get; set; }
        public string ValueFieldError { get; set; }
        public string ValueError { get; set; }

        public ImportIndexError(int index, Exception exception, string fieldError, string valueFieldError, string valueError)
        {
            this.Index = index;
            this.Exception = exception;
            this.FieldError = fieldError;
            this.ValueError = valueError;
            this.ValueFieldError = valueFieldError;
        }
    }
}
