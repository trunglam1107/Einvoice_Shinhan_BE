using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Extensions;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class FileSystemLogBO : IFileSystemLogBO
    {
        #region Fields, Properties
        private readonly string pathLogs;

        #endregion

        #region Contructor
        public FileSystemLogBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.pathLogs = Path.Combine(DefaultFields.BE_FOLDER, "logs");
        }

        #endregion

        #region Methods
        public IEnumerable<FileSystemLogInfo> Filter(ConditionSearchFileSystemLog condition, int skip = 0, int take = int.MaxValue)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(this.pathLogs);
            FileInfo[] childFiles = dirInfo.GetFiles();
            var res = childFiles.Select(p => new FileSystemLogInfo(p)).ToList();
            if (condition.DateFrom.HasValue)
            {
                res = res.FindAll(p => p.modifyDate >= condition.DateFrom.Value);
            }
            if (condition.DateTo.HasValue)
            {
                res = res.FindAll(p => p.modifyDate <= condition.DateTo.Value.AddDays(1).AddMilliseconds(-1));
            }
            return res.AsQueryable().OrderBy(condition.ColumnOrder, condition.OrderType.Equals(OrderTypeConst.Desc)).Skip(skip).Take(take).ToList();
        }

        public int Count(ConditionSearchFileSystemLog condition)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(this.pathLogs);
            FileInfo[] childFiles = dirInfo.GetFiles();
            var res = childFiles.Select(p => new FileSystemLogInfo(p)).ToList();
            if (condition.DateFrom.HasValue)
            {
                res = res.FindAll(p => p.modifyDate >= condition.DateFrom);
            }
            if (condition.DateTo.HasValue)
            {
                res = res.FindAll(p => p.modifyDate <= condition.DateTo);
            }
            return res.Count;
        }
        public ExportFileInfo DownloadFileLogs(string fileName)
        {
            string filePath = string.Format(@"{0}\{1}", this.pathLogs, fileName);
            ExportFileInfo fileInfo = new ExportFileInfo();
            if (File.Exists(filePath))
            {
                fileInfo.FileName = fileName;
                fileInfo.FullPathFileName = filePath;
            }
            return fileInfo;
        }
        #endregion Methods
    }
}