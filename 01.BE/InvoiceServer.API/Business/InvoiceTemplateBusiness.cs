using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness'
    public class InvoiceTemplateBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness'
    {
        #region Fields, Properties

        private readonly IRegisterTemplatesBO registerTemplateBO;
        private readonly IInvoiceTemplateBO invoiceTemplateBO;
        private readonly IMyCompanyBO companyBo;
        private readonly INoticeUseInvoiceBO noticeUseInvoiceBO;
        private readonly IReleaseAnnouncementBO releaseAnnouncementBO;
        private readonly IInvoiceReleasesBO invoiceReleaseBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.InvoiceTemplateBusiness(IBOFactory)'
        public InvoiceTemplateBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.InvoiceTemplateBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            this.registerTemplateBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig, this.CurrentUser);
            this.invoiceTemplateBO = boFactory.GetBO<IInvoiceTemplateBO>(printConfig, this.CurrentUser);
            this.noticeUseInvoiceBO = boFactory.GetBO<INoticeUseInvoiceBO>(printConfig, this.CurrentUser);
            this.releaseAnnouncementBO = boFactory.GetBO<IReleaseAnnouncementBO>(printConfig, this.CurrentUser);
            this.invoiceReleaseBO = boFactory.GetBO<IInvoiceReleasesBO>(this.CurrentUser);

            var uploadImageConfig = new UpdateloadImageConfig
            {
                MaxSizeImage = Config.ApplicationSetting.Instance.MaxSizeImage,
                RootFolderUpload = DefaultFields.ASSET_FOLDER
            };
            this.companyBo = boFactory.GetBO<IMyCompanyBO>(uploadImageConfig);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.InvoiceTemplateBusiness(IBOFactory, bool)'
        public InvoiceTemplateBusiness(IBOFactory boFactory, bool byPass)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.InvoiceTemplateBusiness(IBOFactory, bool)'
        {
            PrintConfig printConfig = GetPrintConfigbyPass();
            this.registerTemplateBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig, this.CurrentUser);
            this.invoiceTemplateBO = boFactory.GetBO<IInvoiceTemplateBO>(printConfig, this.CurrentUser);
            var uploadImageConfig = new UpdateloadImageConfig
            {
                MaxSizeImage = Config.ApplicationSetting.Instance.MaxSizeImage,
                RootFolderUpload = DefaultFields.ASSET_FOLDER
            };
            this.companyBo = boFactory.GetBO<IMyCompanyBO>(uploadImageConfig);
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.GetList()'
        public IEnumerable<RegisterTemplateMaster> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.GetList()'
        {
            var company = this.CurrentUser.Company;
            long companyId = 0;
            // do mau chi duoc tao moi khi la Chinh Nhanh/HO -> PGD get mau theo CN ma no truc thuoc
            if (company.LevelCustomer == LevelCustomerInfo.TransactionOffice)
            {
                companyId = company.ParentCompanyId;
            }
            else
            {
                companyId = (long)company.Id;
            }
            var invoiceTemplate = this.registerTemplateBO.GetList(companyId);
            return invoiceTemplate;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.GetListDenominator(long?)'
        public IEnumerable<RegisterTemplateDenominator> GetListDenominator(long? branch)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.GetListDenominator(long?)'
        {
            long branhId = 0;
            long companyId = 0;
            var company = this.CurrentUser.Company;
            // do mau chi duoc tao moi khi la Chinh Nhanh/HO -> PGD get mau theo CN ma no truc thuoc
            if (company.LevelCustomer == LevelCustomerInfo.TransactionOffice)
            {
                companyId = company.ParentCompanyId;
            }
            else
            {
                companyId = (long)company.Id;
            }
            if (branch != 0)
            {
                company = this.companyBo.GetCompanyInfo((long)branch);
                // do mau chi duoc tao moi khi la Chinh Nhanh/HO -> PGD get mau theo CN ma no truc thuoc
                if (company.LevelCustomer == LevelCustomerInfo.TransactionOffice)
                {
                    branhId = company.ParentCompanyId;
                }
                else
                {
                    branhId = (long)company.Id;
                }
            }
            var invoiceTemplate = this.registerTemplateBO.GetListDenominator(companyId, branhId);
            return invoiceTemplate;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.GetTemplateList(long)'
        public IEnumerable<RegisterTemplateMaster> GetTemplateList(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.GetTemplateList(long)'
        {
            var invoiceTemplate = this.registerTemplateBO.GetList(id);
            return invoiceTemplate;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PTGetList(long)'
        public IEnumerable<RegisterTemplateMaster> PTGetList(long companyID)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PTGetList(long)'
        {
            var company = this.companyBo.GetCompanyInfo(companyID);
            long companyId = 0;
            // do mau chi duoc tao moi khi la Chinh Nhanh/HO -> PGD get mau theo CN ma no truc thuoc
            if (company.LevelCustomer == LevelCustomerInfo.TransactionOffice)
            {
                companyId = company.ParentCompanyId;
            }
            else
            {
                companyId = (long)company.Id;
            }
            var invoiceTemplate = this.registerTemplateBO.GetList(companyId);

            return invoiceTemplate;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.GetListInvoiceTemplateApproved(long)'
        public IEnumerable<TemplateAccepted> GetListInvoiceTemplateApproved(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.GetListInvoiceTemplateApproved(long)'
        {

            var invoiceTemplateApproved = this.registerTemplateBO.GetInvoiceApproved(id);
            return invoiceTemplateApproved;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PrintView(string)'
        public ExportFileInfo PrintView(string parttern)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PrintView(string)'
        {
            long id;
            long companyId = GetCompanyIdOfUser();
            bool isNum = long.TryParse(parttern, out id);
            if (isNum)
            {
                ExportFileInfo fileInfo = this.invoiceTemplateBO.PrintViewTemplate(id, companyId);
                return fileInfo;
            }
            else
            {
                string partternOfTemplate = FilterParameterInString(parttern, string.Empty, true);
                string prefix = FilterParameterInString(parttern, "prefix=");
                string suffix = FilterParameterInString(parttern, "suffix=");
                string symbol = string.Format("{0}/{1}E", prefix, suffix);
                if (prefix.Replace("undefined", "") == "" || suffix.Replace("undefined", "") == "")
                {
                    symbol = "";
                }
                string numberFrom = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
                ExportFileInfo fileInfo = this.registerTemplateBO.PrintView(companyId, partternOfTemplate, symbol, numberFrom);
                return fileInfo;
            }
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PrintView(string, long?)'
        public ExportFileInfo PrintView(string parttern, long? companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PrintView(string, long?)'
        {
            long id;
            long currentCompanyId = GetCompanyIdOfUser();
            if (companyId.HasValue)
            {
                currentCompanyId = (long)companyId;
            }
            bool isNum = long.TryParse(parttern, out id);
            if (isNum)
            {
                ExportFileInfo fileInfo = this.invoiceTemplateBO.PrintViewTemplate(id, currentCompanyId);
                return fileInfo;
            }
            else
            {
                string partternOfTemplate = FilterParameterInString(parttern, string.Empty, true);
                string prefix = FilterParameterInString(parttern, "prefix=");
                string suffix = FilterParameterInString(parttern, "suffix=");
                string symbol = string.Format("{0}/{1}E", prefix, suffix);
                if (prefix.Replace("undefined", "") == "" || suffix.Replace("undefined", "") == "")
                {
                    symbol = "";
                }
                string numberFrom = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
                ExportFileInfo fileInfo = this.registerTemplateBO.PrintView(currentCompanyId, partternOfTemplate, symbol, numberFrom);
                return fileInfo;
            }
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PrintViewPdf(long, long)'
        public ExportFileInfo PrintViewPdf(long id, long parentId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PrintViewPdf(long, long)'
        {
            long currentCompanyId = GetCompanyIdOfUser();
            var notificationUseDetail = this.noticeUseInvoiceBO.GetListNotificationUseDetail(parentId).Where(p => p.ID == id).FirstOrDefault();
            string numberFrom = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            ExportFileInfo fileInfo = this.registerTemplateBO.PrintView(currentCompanyId, notificationUseDetail.REGISTERTEMPLATECODE, notificationUseDetail.CODE, numberFrom);
            return fileInfo;
        }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PrintViewReleasePdf(long, long)'
        public ExportFileInfo PrintViewReleasePdf(long id, long parentId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateBusiness.PrintViewReleasePdf(long, long)'
        {
            long currentCompanyId = GetCompanyIdOfUser();
            var invoiceReleaseInfo = this.invoiceReleaseBO.GetInvoiceReleasesInfo(parentId, currentCompanyId);
            var invoiceReleaseDetailInfo = invoiceReleaseInfo.InvoiceReleasesDetailInfo.Where(p => p.Id == id).FirstOrDefault();
            string numberFrom = DefaultFields.INVOICE_NO_DEFAULT_VALUE;
            ExportFileInfo fileInfo = this.registerTemplateBO.PrintView(currentCompanyId, invoiceReleaseDetailInfo.Code, invoiceReleaseDetailInfo.Symbol, numberFrom);
            return fileInfo;
        }
        private string FilterParameterInString(string parameter, string key, bool pattern = false)
        {
            string vaues = string.Empty;
            string[] parameters = parameter.Replace("undefined", "").Split('&');
            if (parameters.Length == 0)
            {
                return vaues;
            }
            if (pattern)
            {
                return parameters[0].Trim();
            }

            foreach (var item in parameters)
            {
                if (item.IndexOf(key) > -1)
                {
                    vaues = item.Replace(key, "").Trim();
                }
            }

            return vaues;
        }
        private PrintConfig GetPrintConfig()
        {
            string fullPathFolderAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathFolderAsset, fullPathFileInvoice);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

        private PrintConfig GetPrintConfigbyPass()
        {
            string fullPathFolderAsset = DefaultFields.ASSET_FOLDER;
            string fullPathFileInvoice = Config.ApplicationSetting.Instance.FolderInvoiceFile;
            PrintConfig config = new PrintConfig(fullPathFolderAsset, fullPathFileInvoice);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            return config;
        }

        #endregion
    }
}