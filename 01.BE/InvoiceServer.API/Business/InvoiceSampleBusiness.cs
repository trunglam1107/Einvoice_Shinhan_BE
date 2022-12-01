using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Business.Models.InvoiceTemplate;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness'
    public class InvoiceSampleBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceSampleBO invoiceSampleBO;
        private readonly IInvoiceTemplateBO invoiceTemplateBO;

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.InvoiceSampleBusiness(IBOFactory)'
        public InvoiceSampleBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.InvoiceSampleBusiness(IBOFactory)'
        {
            var printConfig = GetPrintConfig();
            this.invoiceSampleBO = boFactory.GetBO<IInvoiceSampleBO>(this.CurrentUser);
            invoiceTemplateBO = boFactory.GetBO<IInvoiceTemplateBO>(printConfig);
        }

        #endregion Contructor

        #region Methods
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.GeInvoiceSampleById(long)'
        public InvoiceSampleInfo GeInvoiceSampleById(long invoiceSampleId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.GeInvoiceSampleById(long)'
        {
            return invoiceSampleBO.GetInvoiceSampleById(invoiceSampleId);
        }
        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.CreateInvoiceSample(InvoiceSampleInfo)'
        public InvoiceSampleInfo CreateInvoiceSample(InvoiceSampleInfo invoiceSampleInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.CreateInvoiceSample(InvoiceSampleInfo)'
        {
            invoiceSampleInfo.CompanyId = GetCompanyIdOfUser();
            return invoiceSampleBO.CreateInvoiceSample(invoiceSampleInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.UpdateInvoiceSample(InvoiceSampleInfo)'
        public InvoiceSampleInfo UpdateInvoiceSample(InvoiceSampleInfo invoiceSampleInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.UpdateInvoiceSample(InvoiceSampleInfo)'
        {
            invoiceSampleInfo.CompanyId = GetCompanyIdOfUser();
            return invoiceSampleBO.UpdateInvoiceSample(invoiceSampleInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.GetByCode(string)'
        public InvoiceSampleInfo GetByCode(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.GetByCode(string)'
        {
            if (code == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var currnetTaxTypeVm = this.invoiceSampleBO.GetByCode(code);
            if (currnetTaxTypeVm != null)
            {
                return currnetTaxTypeVm;
            }
            return new InvoiceSampleInfo();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.GetById(long)'
        public InvoiceSampleInfo GetById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.GetById(long)'
        {
            var invoiceSampleInfo = this.invoiceSampleBO.GetById(id);
            if (invoiceSampleInfo != null)
            {
                return invoiceSampleInfo;
            }
            return new InvoiceSampleInfo();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.Update(long, InvoiceSampleInfo)'
        public ResultCode Update(long id, InvoiceSampleInfo invoiceSampleInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.Update(long, InvoiceSampleInfo)'
        {
            if (invoiceSampleInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            var invoiceTemplateVM = new InvoiceTemplateViewModel()
            {
                //Id = invoiceTemplateBO.GetByInvoiceSampleCode(invoiceSampleViewModel.Code),
                //InvoiceSampleCode = invoiceSampleViewModel.Code,
                //InvoiceTypeId = invoiceSampleInfo.Id,
                Name = invoiceSampleInfo.FileName,
                Deleted = false,
                DetailInvoice = File.ReadAllText(invoiceSampleInfo.TemplateURL),
                UrlFile = invoiceSampleInfo.FileName
            };

            long idCompany = GetCompanyIdOfUser();
            invoiceTemplateBO.CreateInvoiceTemplateByInvoiceSample(invoiceTemplateVM, idCompany, id);
            return this.invoiceSampleBO.Update(id, invoiceSampleInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.CopyInvoiceTemplate(long, InvoiceSampleInfo, InvoiceTemplateViewModel)'
        public ResultCode CopyInvoiceTemplate(long id, InvoiceSampleInfo invoiceSampleInfo, InvoiceTemplateViewModel invoiceTemplateSampleViewModel)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.CopyInvoiceTemplate(long, InvoiceSampleInfo, InvoiceTemplateViewModel)'
        {
            if (invoiceSampleInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long idCompany = GetCompanyIdOfUser();
            invoiceTemplateBO.CreateInvoiceTemplateByInvoiceSample(invoiceTemplateSampleViewModel, idCompany, id);
            return this.invoiceSampleBO.Update(id, invoiceSampleInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.GetByInvoiceTypeId(long)'
        public IEnumerable<InvoiceSampleInfo> GetByInvoiceTypeId(long invoiceTypeId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.GetByInvoiceTypeId(long)'
        {
            var currnetTaxTypeVm = this.invoiceSampleBO.GetByInvoiceTypeId(invoiceTypeId);
            return currnetTaxTypeVm;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.MyInvoiceSampleUsing(long)'
        public MyUsingModel MyInvoiceSampleUsing(long invoiceId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.MyInvoiceSampleUsing(long)'
        {
            var result = new MyUsingModel();
            result.isUsing = invoiceSampleBO.MyInvoiceSampleUsing(invoiceId);
            return result;
        }

        #region Private Method

        private PrintConfig GetPrintConfig()
        {
            string fullPathFolderAsset = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathFolderAsset);
            config.FullPathFolderTemplateInvoice = HttpContext.Current.Server.MapPath(Config.ApplicationSetting.Instance.TemplateInvoiceFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.Delete(string)'
        public ResultCode Delete(string code)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.Delete(string)'
        {
            return this.invoiceSampleBO.Delete(code);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.DeleteById(long)'
        public ResultCode DeleteById(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.DeleteById(long)'
        {
            return this.invoiceSampleBO.DeleteById(id);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.Fillter(ConditionSearchInvoiceSample)'
        public IEnumerable<InvoiceSampleInfo> Fillter(ConditionSearchInvoiceSample condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceSampleBusiness.Fillter(ConditionSearchInvoiceSample)'
        {
            condition.TotalRecords = this.invoiceSampleBO.Count(condition);
            return this.invoiceSampleBO.Filter(condition);
        }

        #endregion
    }
}