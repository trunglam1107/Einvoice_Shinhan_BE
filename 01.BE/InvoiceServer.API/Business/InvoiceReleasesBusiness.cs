using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using InvoiceServer.Common.Enums;
using System;
using System.Collections.Generic;
using System.Web.Configuration;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness'
    public class InvoiceReleasesBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness'
    {
        #region Fields, Properties

        private readonly IInvoiceReleasesBO invoiceReleaseBO;
        private readonly IRegisterTemplatesBO templateCompanyUseReleaseBO;
        private readonly Func<string, string> GetConfig = key => WebConfigurationManager.AppSettings[key];

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.InvoiceReleasesBusiness(IBOFactory)'
        public InvoiceReleasesBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.InvoiceReleasesBusiness(IBOFactory)'
        {
            var printConfig = GetPrintConfig();
            this.invoiceReleaseBO = boFactory.GetBO<IInvoiceReleasesBO>(this.CurrentUser);
            this.templateCompanyUseReleaseBO = boFactory.GetBO<IRegisterTemplatesBO>(printConfig);
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.Fillter(ConditionSearchInvoiceReleases)'
        public IEnumerable<InvoiceReleasesMaster> Fillter(ConditionSearchInvoiceReleases condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.Fillter(ConditionSearchInvoiceReleases)'
        {
            condition.TotalRecords = this.invoiceReleaseBO.Count(condition);
            return this.invoiceReleaseBO.Filter(condition);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.GetInvoiceSample()'
        public IEnumerable<RegisterTemplateMaster> GetInvoiceSample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.GetInvoiceSample()'
        {
            long companyId = GetCompanyId();
            return templateCompanyUseReleaseBO.GetList(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.GetInvoiceReleasesInfo(long)'
        public InvoiceReleasesInfo GetInvoiceReleasesInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.GetInvoiceReleasesInfo(long)'
        {
            long companyId = GetCompanyId();
            return this.invoiceReleaseBO.GetInvoiceReleasesInfo(id, companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.ViewGetInvoiceReleasesInfo(long)'
        public InvoiceReleasesInfo ViewGetInvoiceReleasesInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.ViewGetInvoiceReleasesInfo(long)'
        {
            long companyId = GetCompanyId();
            return this.invoiceReleaseBO.GetInvoiceReleasesInfo(id, companyId, true);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.Create(InvoiceReleasesInfo)'
        public ResultCode Create(InvoiceReleasesInfo invoiceReleasesInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.Create(InvoiceReleasesInfo)'
        {
            if (invoiceReleasesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode result = this.invoiceReleaseBO.Create(invoiceReleasesInfo);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.Update(long, InvoiceReleasesInfo)'
        public ResultCode Update(long id, InvoiceReleasesInfo invoiceReleasesInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.Update(long, InvoiceReleasesInfo)'
        {
            if (invoiceReleasesInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }
            long idCompany = GetCompanyId();
            return this.invoiceReleaseBO.Update(id, idCompany, invoiceReleasesInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.UpdateStatus(long, InvoiceReleasesStatus, bool)'
        public ResultCode UpdateStatus(long id, InvoiceReleasesStatus invoiceStatus, bool isUnApproved = false)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.UpdateStatus(long, InvoiceReleasesStatus, bool)'
        {
            if (invoiceStatus == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            if (!Enum.IsDefined(typeof(RecordStatus), invoiceStatus.Status))
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long idCompany = GetCompanyId();
            invoiceStatus.ActionUserId = this.CurrentUser.Id;
            return this.invoiceReleaseBO.UpdateStatus(id, idCompany, invoiceStatus, isUnApproved);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.Delete(long)'
        {
            long companyId = GetCompanyId();
            return this.invoiceReleaseBO.Delete(id, companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.GetTemplateRelease()'
        public InvoiceReleasesTemplateInfo GetTemplateRelease()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.GetTemplateRelease()'
        {
            return this.invoiceReleaseBO.GetTemplateReleaseTemplate();
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
            string fullPathTemplateFolder = DefaultFields.ASSET_FOLDER;
            PrintConfig config = new PrintConfig(fullPathTemplateFolder);
            config.BuildAssetByCompany(this.CurrentUser.Company);
            return config;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.GetConfigInfo()'
        public WebConfigInfo GetConfigInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'InvoiceReleasesBusiness.GetConfigInfo()'
        {
            WebConfigInfo configInfor = new WebConfigInfo();

            configInfor.CompanyNameConfig = this.GetConfig("CompanyName");
            configInfor.LinkAgencyLogin = this.GetConfig("LinkAgencyLogin");

            return configInfor;
        }
        #endregion
    }
}