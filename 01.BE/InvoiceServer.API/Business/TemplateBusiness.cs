using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness'
    public class TemplateBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceTemplateBO _iInvoiceTemplateBO;
        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness.TemplateBusiness(IBOFactory)'
        public TemplateBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness.TemplateBusiness(IBOFactory)'
        {
            PrintConfig printConfig = GetPrintConfig();
            _iInvoiceTemplateBO = boFactory.GetBO<IInvoiceTemplateBO>(printConfig);
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness.GetList()'
        public IEnumerable<InvoiceTemplateInfo> GetList()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness.GetList()'
        {
            var idCompany = GetCompanyIdOfUser();
            return this._iInvoiceTemplateBO.GetList(idCompany);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness.FilterByInvoiceSample(long)'
        public IEnumerable<InvoiceTemplateInfo> FilterByInvoiceSample(long invoiceSampleId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness.FilterByInvoiceSample(long)'
        {
            var idCompany = this.GetCompanyIdOfUser();
            return this._iInvoiceTemplateBO.Filter(invoiceSampleId, idCompany);
        }


        private PrintConfig GetPrintConfig()
        {
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness.GetByInvoiceSampleCode(string)'
        public long GetByInvoiceSampleCode(string invoiceSampleCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'TemplateBusiness.GetByInvoiceSampleCode(string)'
        {

            return _iInvoiceTemplateBO.GetByInvoiceSampleCode(invoiceSampleCode, this.CurrentUser.Company.Id ?? 0);
        }

        #endregion
    }
}