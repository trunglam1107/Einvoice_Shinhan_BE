using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.HistoryReportGeneral;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness'
    public class ReportManagementBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness'
    {
        #region Fields, Properties
        private readonly IReportSummaryBO reportSummaryBO;
        private readonly IHistoryReportGeneralBO historyReportGeneralBO;
        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.ReportManagementBusiness(IBOFactory)'
        public ReportManagementBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.ReportManagementBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            this.reportSummaryBO = boFactory.GetBO<IReportSummaryBO>(printConfig);

            this.historyReportGeneralBO = boFactory.GetBO<IHistoryReportGeneralBO>();
        }

        #endregion Contructor

        #region Methods
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.ReportCustomers(out long, string, string, int?, long?, string)'
        public IEnumerable<ReportCustomer> ReportCustomers(out long totalRecords, string dateFrom = null, string dateTo = null, int? contractType = null, long? customerId = null, string customerName = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.ReportCustomers(out long, string, string, int?, long?, string)'
        {
            var condition = new ConditionReportSummary(this.CurrentUser, dateFrom, dateTo, contractType, customerId, customerName);
            var reportCustomers = this.reportSummaryBO.ViewReportCustomers(condition, this.CurrentUser.Company);
            totalRecords = 100;
            return reportCustomers;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.ReportCustomersDownload(string, string, int?, long?, string)'
        public ExportFileInfo ReportCustomersDownload(string dateFrom = null, string dateTo = null, int? contractType = null, long? customerId = null, string customerName = null)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.ReportCustomersDownload(string, string, int?, long?, string)'
        {
            var condition = new ConditionReportSummary(this.CurrentUser, dateFrom, dateTo, contractType, customerId, customerName);
            ExportFileInfo fileInfo = this.reportSummaryBO.ExportReportCustomers(condition, this.CurrentUser.Company);
            fileInfo.FileName = "Customer report";
            return fileInfo;
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.GetAll(FilterHistoryReportGeneralCondition)'
        public List<InvoiceServer.Business.Models.HistoryReportGeneralInfo> GetAll(FilterHistoryReportGeneralCondition condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.GetAll(FilterHistoryReportGeneralCondition)'
        {
            condition.TotalRecords = this.historyReportGeneralBO.Count(condition);
            return this.historyReportGeneralBO.GetAll(condition).ToList();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.DownloadExcelHRG(FilterHistoryReportGeneralCondition)'
        public ExportFileInfo DownloadExcelHRG(FilterHistoryReportGeneralCondition condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReportManagementBusiness.DownloadExcelHRG(FilterHistoryReportGeneralCondition)'
        {
            ExportFileInfo fileInfo;
            fileInfo = this.historyReportGeneralBO.DownloadExcelHRG(condition);
            return fileInfo;
        }
        #endregion
    }
}