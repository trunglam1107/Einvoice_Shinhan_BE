using InvoiceServer.Business.DAO;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Data.DBAccessor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace InvoiceServer.Business.BL
{
    public class SystemLogBO : ISystemLogBO
    {
        #region Fields, Properties
        private readonly ISystemLogRepository systemLogRepository;
        private readonly PrintConfig config;

        #endregion

        #region Contructor

        public SystemLogBO(IRepositoryFactory repoFactory)
        {
            Ensure.Argument.ArgumentNotNull(repoFactory, "repoFactory");
            this.systemLogRepository = repoFactory.GetRepository<ISystemLogRepository>();
        }

        public SystemLogBO(IRepositoryFactory repoFactory, PrintConfig config)
            : this(repoFactory)
        {
            this.config = config;
        }

        #endregion

        #region Methods
        public IEnumerable<SystemLogInfo> Filter(ConditionSearchSystemLog condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.systemLogRepository.FilterSystemLog(condition);
        }

        public int Count(ConditionSearchSystemLog condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.systemLogRepository.Count(condition);
        }

        public ExportFileInfo DownloadDataSystem(ConditionSearchSystemLog condition)
        {
            var dataRport = new SystemLogExport(condition.CurrentUser?.Company);
            var systemLogs = Filter(condition);
            dataRport.Items = systemLogs.Select(p => new SystemLogInfo(p)).ToList();
            ExportSystemLogView export = new ExportSystemLogView(dataRport, config, condition.Language);
            return export.ExportFile();
        }

        public ResultCode Create(SystemLogInfo model)
        {
            var result = new SYSTEMLOG()
            {
                FUNCTIONCODE = model.FunctionCode,
                LOGSUMMARY = model.LogSummary,
                LOGTYPE = model.LogType,
                LOGDATE = DateTime.Now,
                LOGDETAIL = model.LogDetail,
                IP = model.IP,
                USERNAME = model.UserName,
            };
            var isUpdateSuccess = systemLogRepository.Insert(result);
            if (isUpdateSuccess)
            {
                return ResultCode.NoError;
            }
            return ResultCode.UnitIssuedNotUpdate;
        }

        #endregion Methods
    }
}