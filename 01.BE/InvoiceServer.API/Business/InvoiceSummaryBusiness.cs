using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryBusiness'
    public class InvoiceSummaryBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceBO invoiceBO;
        private readonly INoticeUseInvoiceDetailBO noticeUseInvoiceDetailBO;
        private readonly IReportCancellingBO reportCancellingBO;


        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryBusiness.InvoiceSummaryBusiness(IBOFactory)'
        public InvoiceSummaryBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryBusiness.InvoiceSummaryBusiness(IBOFactory)'
        {
            var printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig, emailConfig);
            this.noticeUseInvoiceDetailBO = boFactory.GetBO<INoticeUseInvoiceDetailBO>();
            this.reportCancellingBO = boFactory.GetBO<IReportCancellingBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryBusiness.GetSummaryInvoice()'
        public SummaryInvoice GetSummaryInvoice()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSummaryBusiness.GetSummaryInvoice()'
        {
            long companyId = GetCompanyIdOfUser();
            decimal numberInvoiceNotRelease = this.invoiceBO.CountInvoiceNotRelease(companyId);
            decimal numberInvoiceAdjustment = this.invoiceBO.CountInvoiceAdjustment(companyId);
            decimal numberInvoiceCancel = this.invoiceBO.CountInvoiceCancel(companyId);
            decimal numberInvoiceSubstitute = this.invoiceBO.CountInvoiceSubstitute(companyId);
            decimal numberInvoiceRemoved = this.invoiceBO.CountInvoiceDeleted(companyId);
            decimal numberInvoiceRelease = this.invoiceBO.CountInvoiceRelease(companyId);
            return new SummaryInvoice
            {
                NumberNotRelease = numberInvoiceNotRelease,
                NumberAdjustment = numberInvoiceAdjustment,
                NumberCancel = numberInvoiceCancel,
                NumberSubstitute = numberInvoiceSubstitute,
                NumberDeleted = numberInvoiceRemoved,
                NumberRelease = numberInvoiceRelease,
            };
        }

        private PrintConfig GetPrintConfig()
        {
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathFileAsset);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return config;
        }

        public string GetExpireDateToken()
        {
            long companyId = GetCompanyIdOfUser();
            var result = this.invoiceBO.GetExpirceDateToken(companyId);

            return result.ToString();
        }
        #endregion
    }
}