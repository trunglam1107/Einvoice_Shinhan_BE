using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness'
    public class SubstituteInvoiceBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceBO invoiceBO;
        private readonly IRegisterTemplatesBO templateCompanyUseReleaseBO;
        private static readonly Logger logger = new Logger();

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.SubstituteInvoiceBusiness(IBOFactory)'
        public SubstituteInvoiceBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.SubstituteInvoiceBusiness(IBOFactory)'
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.Fillter(ConditionSearchInvoice)'
        public IEnumerable<InvoiceMaster> Fillter(ConditionSearchInvoice condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.Fillter(ConditionSearchInvoice)'
        {
            condition.TotalRecords = this.invoiceBO.CountSubstitute(condition);
            return this.invoiceBO.FilterSubstitute(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.GetInvoiceSample()'
        public IEnumerable<RegisterTemplateMaster> GetInvoiceSample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.GetInvoiceSample()'
        {
            long companyId = GetCompanyId();
            return templateCompanyUseReleaseBO.GetList(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.GetInvoicenfo(long)'
        public InvoiceInfo GetInvoicenfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.GetInvoicenfo(long)'
        {
            long companyId = GetCompanyId();
            return this.invoiceBO.GetInvoiceInfo(id, companyId, false);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.Create(InvoiceInfo)'
        public Result Create(InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.Create(InvoiceInfo)'
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
            invoiceInfo.FolderPath = Config.ApplicationSetting.Instance.FolderInvoiceFile + "\\FileRecipt";
            ReleaseInvoiceMaster Info = this.invoiceBO.Substitute(invoiceInfo);
            Result result = new Result();
            result.Count = Info.ReleaseInvoiceInfos.Count;
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.Update(long, InvoiceInfo)'
        public ResultCode Update(long id, InvoiceInfo invoiceInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.Update(long, InvoiceInfo)'
        {
            if (invoiceInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long idCompany = GetCompanyIdOfUser();
            invoiceInfo.CompanyId = idCompany;
            return this.invoiceBO.Update(id, idCompany, invoiceInfo);
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.RegistInvoiceNo(long, string)'
        public InvoiceNo RegistInvoiceNo(long templateId, string symbol)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SubstituteInvoiceBusiness.RegistInvoiceNo(long, string)'
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
            string fullPathFileAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathFileAsset, fullPathFileInvoice);
            try
            {
                if (this.CurrentUser.Company != null)
                {
                    config.BuildAssetByCompany(this.CurrentUser.Company);
                }
            }
            catch (Exception ex)
            {
                logger.Error(this.CurrentUser.UserId, ex);
            }
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);

            return config;
        }

        #endregion
    }
}