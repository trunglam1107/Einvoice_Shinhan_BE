using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceBusiness'
    public class ReplacedInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceBO invoiceBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceBusiness.ReplacedInvoiceBusiness(IBOFactory)'
        public ReplacedInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceBusiness.ReplacedInvoiceBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };

            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig, emailConfig);
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceBusiness.Fillter(SearchReplacedInvoice)'
        public IEnumerable<ReplacedInvoice> Fillter(SearchReplacedInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceBusiness.Fillter(SearchReplacedInvoice)'
        {
            condition.TotalRecords = this.invoiceBO.CountReplacedInvoice(condition);
            return this.invoiceBO.FilterReplacedInvoice(condition);
        }

        private PrintConfig GetPrintConfig()
        {
            var fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            string fullPathFileExportData = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.FolderExportDataOfCompany);
            PrintConfig config = new PrintConfig(fullPathTemplateFolder, fullPathFileInvoice, fullPathFileExportData);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceBusiness.ExportExcel(SearchReplacedInvoice)'
        public ExportFileInfo ExportExcel(SearchReplacedInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'ReplacedInvoiceBusiness.ExportExcel(SearchReplacedInvoice)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.ExportExcelAdjust(condition, 2);
            return fileInfo;
        }
        #endregion
    }
}