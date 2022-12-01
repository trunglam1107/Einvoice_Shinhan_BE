using InvoiceServer.Business.Models;
using System.Collections.Generic;

namespace InvoiceServer.Business.BL
{
    public interface IFileSystemLogBO
    {
        /// <summary>
        /// Search systemlog with condition
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        IEnumerable<FileSystemLogInfo> Filter(ConditionSearchFileSystemLog condition, int skip = 0, int take = int.MaxValue);
        int Count(ConditionSearchFileSystemLog condition);
        ExportFileInfo DownloadFileLogs(string fileName);
    }
}
