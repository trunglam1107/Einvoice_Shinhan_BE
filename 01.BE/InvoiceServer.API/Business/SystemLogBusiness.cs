using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness'
    public class SystemLogBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness'
    {
        #region Fields, Properties

        private readonly ISystemLogBO systemLogBO;

        #endregion Fields, Properties

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.SystemLogBusiness(IBOFactory)'
        public SystemLogBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.SystemLogBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            this.systemLogBO = boFactory.GetBO<ISystemLogBO>(printConfig);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.SystemLogBusiness(IBOFactory, bool)'
        public SystemLogBusiness(IBOFactory boFactory, bool byPass)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.SystemLogBusiness(IBOFactory, bool)'
        {
            PrintConfig printConfig = byPass ? GetPrintConfigbyPass() : GetPrintConfig();
            this.systemLogBO = boFactory.GetBO<ISystemLogBO>(printConfig);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.FillterSystemLog(ConditionSearchSystemLog)'
        public List<SystemLogInfo> FillterSystemLog(ConditionSearchSystemLog condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.FillterSystemLog(ConditionSearchSystemLog)'
        {
            condition.TotalRecords = this.systemLogBO.Count(condition);
            return this.systemLogBO.Filter(condition).ToList();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.DownloadDataSystemLogs(ConditionSearchSystemLog)'
        public ExportFileInfo DownloadDataSystemLogs(ConditionSearchSystemLog condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.DownloadDataSystemLogs(ConditionSearchSystemLog)'
        {
            ExportFileInfo fileInfo = this.systemLogBO.DownloadDataSystem(condition);
            return fileInfo;
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }
        private PrintConfig GetPrintConfigbyPass()
        {
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathFileAsset, fullPathFileInvoice);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return config;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.SaveAudit(SaveSystemLog)'
        public string SaveAudit(SaveSystemLog saveSystemLog)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SystemLogBusiness.SaveAudit(SaveSystemLog)'
        {
            //Save Audit Log (Login) 
            String ip = IP.GetIPAddress();
            var systemInfo = new SystemLogInfo
            {
                FunctionCode = saveSystemLog.FunctionCode,
                LogSummary = saveSystemLog.LogSummary,
                LogType = saveSystemLog.LogType,
                LogDetail = saveSystemLog.LogDetail,
                IP = ip,
                UserName = saveSystemLog.FunctionCode.Equals("Portal") ? saveSystemLog.UserId : this.CurrentUser.UserId,
            };
            var insertSystemLog = this.systemLogBO.Create(systemInfo);
            return insertSystemLog.ToString();
        }
        #endregion
    }
}