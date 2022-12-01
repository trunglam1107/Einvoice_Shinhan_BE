using InvoiceServer.Business.BL;
using InvoiceServer.Business.BL.Interface;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.GatewayLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogBusiness'
    public class GatewaylogBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogBusiness'
    {
        #region Fields, Properties

        private readonly IGatewaylogBO gatewayLogBO;

        #endregion Fields, Properties

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogBusiness.GatewaylogBusiness(IBOFactory)'
        public GatewaylogBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogBusiness.GatewaylogBusiness(IBOFactory)'
        {
            var printConfig = GetPrintConfig();
            this.gatewayLogBO = boFactory.GetBO<IGatewaylogBO>(printConfig);
        }

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogBusiness.FillterClient(ConditionSearchGatewaylog)'
        public List<GatewaylogDetail> FillterClient(ConditionSearchGatewaylog condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogBusiness.FillterClient(ConditionSearchGatewaylog)'
        {
            condition.TotalRecords = this.gatewayLogBO.Count(condition);
            return this.gatewayLogBO.Filter(condition).ToList();
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.FolderAssetOfCompany);
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogBusiness.ReportHistoryTVan(ConditionSearchGatewaylog)'
        public ExportFileInfo ReportHistoryTVan(ConditionSearchGatewaylog condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'GatewaylogBusiness.ReportHistoryTVan(ConditionSearchGatewaylog)'
        {
            ExportFileInfo fileInfo;
            fileInfo = this.gatewayLogBO.ReportHistoryTVan(condition);
            return fileInfo;
        }

        #endregion
    }
}