using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.InvoiceTemplate;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness'
    public class InvoiceTemplateSampleBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness'
    {
        #region Fields, Properties
        private readonly IInvoiceTemplateSampleBO invoiceTemplateSampleBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.InvoiceTemplateSampleBusiness(IBOFactory)'
        public InvoiceTemplateSampleBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.InvoiceTemplateSampleBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            this.invoiceTemplateSampleBO = boFactory.GetBO<IInvoiceTemplateSampleBO>(printConfig);
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetList()'
        public IEnumerable<InvoiceTemplateInfo> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetList()'
        {
            var invoiceTemplateSample = this.invoiceTemplateSampleBO.GetList();

            return invoiceTemplateSample;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetListByInvoiceTypeId(long)'
        public IEnumerable<InvoiceTemplateInfo> GetListByInvoiceTypeId(long invoiceTypeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetListByInvoiceTypeId(long)'
        {
            var invoiceTemplateSample = this.invoiceTemplateSampleBO.GetListByInvoiceTypeId(invoiceTypeId);

            return invoiceTemplateSample;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetById(long)'
        public InvoiceTemplateInfo GetById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetById(long)'
        {
            return invoiceTemplateSampleBO.GetById(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetViewModelById(long)'
        public InvoiceTemplateViewModel GetViewModelById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetViewModelById(long)'
        {
            return invoiceTemplateSampleBO.GetViewModelById(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetTemplateSampleNameById(long)'
        public InvoiceTemplateSampleName GetTemplateSampleNameById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceTemplateSampleBusiness.GetTemplateSampleNameById(long)'
        {
            return invoiceTemplateSampleBO.GetTemplateSampleNameById(id);
        }


        private PrintConfig GetPrintConfig()
        {
            string fullPathFolderAsset = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathFolderAsset);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

        #endregion
    }
}