using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.DAO;
using InvoiceServer.Business.DAO.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.GatewayLog;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceServer.Business.BL
{
    public class GatewaylogBO : IGatewaylogBO
    {
        #region Fields, Properties
        private readonly IGatewaylogRepository gatewaylogRepository;
        private readonly IDbTransactionManager transaction;
        private readonly IInvoiceRepository invoiceRepository;
        private readonly PrintConfig config;
        private readonly ISystemSettingRepository systemSettingRepository;
        #endregion

        #region Contructor

        public GatewaylogBO(IRepositoryFactory repoFactory, PrintConfig config)
        {
            Ensure.Argument.NotNullArg(repoFactory, "repoFactory");

            this.gatewaylogRepository = repoFactory.GetRepository<IGatewaylogRepository>();
            this.transaction = repoFactory.GetRepository<IDbTransactionManager>();
            this.invoiceRepository = repoFactory.GetRepository<IInvoiceRepository>();
            this.config = config;
            this.systemSettingRepository = repoFactory.GetRepository<ISystemSettingRepository>();
        }
        //public ClientBO(IRepositoryFactory repoFactory, PrintConfig config, EmailConfig emailConfig) : this(repoFactory, config)
        //{
        //    this.email = new Email(repoFactory);
        //    this.emailConfig = emailConfig;
        //}

        #endregion

        #region Methods

        public long Count(ConditionSearchGatewaylog condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            return this.gatewaylogRepository.Count(condition);
        }

        public IEnumerable<GatewaylogDetail> Filter(ConditionSearchGatewaylog condition)
        {
            if (condition == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            var projects = this.gatewaylogRepository.Filter(condition);
            return projects;

        }
        public ExportFileInfo ReportHistoryTVan(ConditionSearchGatewaylog condition)
        {
            var dataRport = new ReportCompineHistoryTVanExport(condition.CurrentUser?.Company);
            var systemSettingInfo = this.systemSettingRepository.FilterSystemSetting(condition.CompanyId ?? 0);
            dataRport.Items = this.gatewaylogRepository.Filter(condition).ToList();
            ReportHistoryTVanView export = new ReportHistoryTVanView(dataRport, systemSettingInfo, config, condition);
            return export.ExportFile();
        }
        #endregion
    }
}
