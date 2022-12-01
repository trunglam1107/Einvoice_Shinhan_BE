using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogBusiness'
    public class FileSystemLogBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogBusiness'
    {
        #region Fields, Properties

        private readonly IFileSystemLogBO fileSystemLogBO;
        #endregion Fields, Properties

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogBusiness.FileSystemLogBusiness(IBOFactory)'
        public FileSystemLogBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogBusiness.FileSystemLogBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            this.fileSystemLogBO = boFactory.GetBO<IFileSystemLogBO>(printConfig);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogBusiness.FillterSystemLog(out long, string, string, string, string, int, int)'
        public List<FileSystemLogInfo> FillterSystemLog(out long totalRecords, string dateFrom = null, string dateTo = null, string orderby = null, string orderType = null, int skip = 0, int take = int.MaxValue)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogBusiness.FillterSystemLog(out long, string, string, string, string, int, int)'
        {
            var condition = new ConditionSearchFileSystemLog(dateFrom, dateTo, orderby, orderType);
            totalRecords = this.fileSystemLogBO.Count(condition);
            return this.fileSystemLogBO.Filter(condition, skip, take).ToList();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogBusiness.DownloadFileLogs(string)'
        public ExportFileInfo DownloadFileLogs(string fileName)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'FileSystemLogBusiness.DownloadFileLogs(string)'
        {
            ExportFileInfo fileInfo = this.fileSystemLogBO.DownloadFileLogs(fileName);
            return fileInfo;
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }
        #endregion
    }
}