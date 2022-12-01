using System;
using System.Runtime.Serialization;

namespace InvoiceServer.Common
{
    [Serializable]
    public class BusinessImportException : BusinessLogicException
    {
        public string SheetName { get; private set; }
        public string ColumnName { get; private set; }
        public int RowIndex { get; set; }
        public BusinessImportException(ResultCode errorCode, string message, string sheetName, string columnName, int rowIndex)
            : base(errorCode, message)
        {
            this.SheetName = sheetName;
            this.ColumnName = columnName;
            this.RowIndex = rowIndex;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected BusinessImportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
