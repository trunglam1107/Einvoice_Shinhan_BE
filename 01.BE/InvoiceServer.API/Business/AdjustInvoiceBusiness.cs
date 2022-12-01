using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness'
    public class AdjustInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceBO invoiceBO;
        private readonly IRegisterTemplatesBO templateCompanyUseReleaseBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.AdjustInvoiceBusiness(IBOFactory)'
        public AdjustInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.AdjustInvoiceBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            var emailConfig = new EmailConfig()
            {
                FolderEmailTemplate = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.EmailTemplateFilePath),
            };
            this.invoiceBO = boFactory.GetBO<IInvoiceBO>(printConfig, emailConfig, this.CurrentUser);
            this.templateCompanyUseReleaseBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig);
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.Fillter(SearchReplacedInvoice)'
        public IEnumerable<ReplacedInvoice> Fillter(SearchReplacedInvoice condition)
        {
            condition.TotalRecords = this.invoiceBO.CountReplacedInvoice(condition);
            return this.invoiceBO.FilterReplacedInvoice(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.FillterAdjustInvoice(ConditionSearchInvoice)'
        public InvoiceMaster FillterAdjustInvoice(ConditionSearchInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.FillterAdjustInvoice(ConditionSearchInvoice)'
        {
            return this.invoiceBO.FilterAnnoun(condition).FirstOrDefault();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.GetInvoiceSample()'
        public IEnumerable<RegisterTemplateMaster> GetInvoiceSample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.GetInvoiceSample()'
        {
            long companyId = GetCompanyId();
            return templateCompanyUseReleaseBO.GetList(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.GetInvoicenfo(long)'
        public InvoiceInfo GetInvoicenfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.GetInvoicenfo(long)'
        {
            long companyId = GetCompanyId();
            return this.invoiceBO.GetInvoiceInfo(id, companyId, false);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.UpdateAdjustFileRecipt(InvoiceInfo)'
        public ResultCode UpdateAdjustFileRecipt(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.UpdateAdjustFileRecipt(InvoiceInfo)'
        {
            return this.invoiceBO.UpdateAdjustFileRecipt(invoiceInfo);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.UpdateCancelFileRecipt(InvoiceInfo)'
        public ResultCode UpdateCancelFileRecipt(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.UpdateCancelFileRecipt(InvoiceInfo)'
        {
            return this.invoiceBO.UpdateCancelFileRecipt(invoiceInfo);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.Create(InvoiceInfo)'
        public Result Create(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.Create(InvoiceInfo)'
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            if (!invoiceInfo.CompanyId.HasValue)
            {
                invoiceInfo.CompanyId = this.CurrentUser.Company.Id;
            }
            invoiceInfo.UserAction = this.CurrentUser.Id;
            invoiceInfo.FolderPath = DefaultFields.FILE_RECEPT_FOLDER;
            ReleaseInvoiceMaster Info = this.invoiceBO.Adjust(invoiceInfo);
            Result result = new Result();
            result.Count = Info.ReleaseInvoiceInfos.Count;
            return result;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.RegistInvoiceNo(long, string)'
        public InvoiceNo RegistInvoiceNo(long templateId, string symbol)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.RegistInvoiceNo(long, string)'
        {
            long companyId = GetCompanyId();
            return this.invoiceBO.RegistInvoiceNo(companyId, templateId, symbol);
        }

        private long GetCompanyId()
        {
            long companyId = 0;
            if (this.CurrentUser.Company.Id.HasValue)
            {
                companyId = this.CurrentUser.Company.Id.Value;
            }

            return companyId;
        }

        private PrintConfig GetPrintConfig()
        {
            var fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            string fullPathFileExportData = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.FolderExportDataOfCompany);
            PrintConfig config = new PrintConfig(fullPathTemplateFolder, fullPathFileInvoice, fullPathFileExportData);
            return config;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.ExportExcel(SearchReplacedInvoice)'
        public ExportFileInfo ExportExcel(SearchReplacedInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'AdjustInvoiceBusiness.ExportExcel(SearchReplacedInvoice)'
        {
            ExportFileInfo fileInfo = this.invoiceBO.ExportExcelAdjust(condition, 1);
            return fileInfo;
        }
        #endregion
    }
}