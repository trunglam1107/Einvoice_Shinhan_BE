using InvoiceServer.Business.BL;
using InvoiceServer.Business.Models;
using InvoiceServer.Business.Models.Company;
using InvoiceServer.Common;
using InvoiceServer.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceServer.API.Business
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness'
    public class CompanyBusiness : BaseBusiness
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness'
    {
        #region Fields, Properties

        private readonly IMyCompanyBO companyBo;
        private readonly ISignatureBO signatureBO;
        private static readonly Logger logger = new Logger();

        #endregion Fields, Properties

        #region Contructor

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.CompanyBusiness(IBOFactory)'
        public CompanyBusiness(IBOFactory boFactory)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.CompanyBusiness(IBOFactory)'
        {
            var uploadImageConfig = new UpdateloadImageConfig
            {
                MaxSizeImage = Config.ApplicationSetting.Instance.MaxSizeImage,
                RootFolderUpload = DefaultFields.ASSET_FOLDER
            };
            this.companyBo = boFactory.GetBO<IMyCompanyBO>(uploadImageConfig);
            this.signatureBO = boFactory.GetBO<ISignatureBO>();
        }

        #endregion Contructor

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.FillterCompanies(ConditionSearchCompany)'
        public IEnumerable<MasterCompanyInfo> FillterCompanies(ConditionSearchCompany condition)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.FillterCompanies(ConditionSearchCompany)'
        {
            condition.TotalRecords = this.companyBo.CountFillterCompany(condition);
            var companies = this.companyBo.FilterCompany(condition);
            return companies;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetCompanyInfo(long)'
        public CompanyInfo GetCompanyInfo(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetCompanyInfo(long)'
        {
            return this.companyBo.GetCompanyInfo(companyId, RoleInfo.BRANCH);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.CreateCompanyInfo(CompanyInfo)'
        public ResultCode CreateCompanyInfo(CompanyInfo companyInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.CreateCompanyInfo(CompanyInfo)'
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            ResultCode result = this.companyBo.Create(companyInfo, this.CurrentUser);
            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.UpdateCompanyInfo(long, CompanyInfo)'
        public ResultCode UpdateCompanyInfo(long companyId, CompanyInfo companyInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.UpdateCompanyInfo(long, CompanyInfo)'
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            return this.companyBo.Update(companyId, companyInfo);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.Delete(long)'
        public ResultCode Delete(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.Delete(long)'
        {
            string roleOfCurrentUser = this.CurrentUser.RoleUser.Level;
            return this.companyBo.Delete(id, roleOfCurrentUser);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetMyCompanyInfo(long)'
        public MyCompanyInfo GetMyCompanyInfo(long id)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetMyCompanyInfo(long)'
        {
            long? companyid = this.CurrentUser.Company.Id;
            var company = this.companyBo.GetCompanyOfUser(id, id);// cho check trung nhau
            company.Signatures = this.signatureBO.GetList((long)companyid).ToList();
            return company;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetCompanyLogin()'
        public CompanyInfo GetCompanyLogin()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetCompanyLogin()'
        {
            var companyid = this.CurrentUser.Company.Id;
            var company = this.companyBo.GetCompanyInfo((long)companyid);
            return company;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetListMyCompanyInfo()'
        public IEnumerable<MyCompanyInfo> GetListMyCompanyInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetListMyCompanyInfo()'
        {
            var companyInfoEnity = this.companyBo.GetList();
            return companyInfoEnity;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.UpdateMyCompany(long, MyCompanyInfo)'
        public ResultCode UpdateMyCompany(long id, MyCompanyInfo companyInfo)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.UpdateMyCompany(long, MyCompanyInfo)'
        {
            if (companyInfo == null)
            {
                throw new BusinessLogicException(ResultCode.DataInvalid, MsgApiResponse.DataInvalid);
            }

            long? companyid = this.CurrentUser.Company.Id;
            string roleLevel = this.CurrentUser.RoleUser.Level;
            return this.companyBo.UpdateMyCompany(id, companyid, companyInfo, roleLevel);

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetCompanyChill(long)'
        public IEnumerable<CompanyInfo> GetCompanyChill(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetCompanyChill(long)'
        {
            return companyBo.GetCompanyChill(companyId);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetFooterInfo()'
        public FooterInvoiceInfo GetFooterInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetFooterInfo()'
        {
            var footerInvoiceInfo = new FooterInvoiceInfo()
            {
                FooterCompanyName = Config.ApplicationSetting.Instance.FooterCompanyName,
                FooterTaxCode = Config.ApplicationSetting.Instance.FooterTaxCode,
                FooterPhone = Config.ApplicationSetting.Instance.FooterPhone,
                FooterLink = Config.ApplicationSetting.Instance.FooterLink,
                FooterLinkText = Config.ApplicationSetting.Instance.FooterLinkText,
                FooterPortalLink = Config.ApplicationSetting.Instance.FooterPortalLink,
                FooterPortalLinkText = Config.ApplicationSetting.Instance.FooterPortalLinkText,
            };

            return footerInvoiceInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetHeaderInfo()'
        public dynamic GetHeaderInfo()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.GetHeaderInfo()'
        {
            dynamic headerInvoiceInfo = null;
            if (CustomerIndex.IsHuaNan)
            {
                headerInvoiceInfo = new HuaNanHeaderInvoiceInfo()
                {
                    CompanyNameCn = Config.ApplicationSetting.Instance.CompanyNameCn,
                    CompanyBranchCn = Config.ApplicationSetting.Instance.CompanyBranchCn,
                    CompanyNameVn = Config.ApplicationSetting.Instance.CompanyNameVn,
                    CompanyBranchVn = Config.ApplicationSetting.Instance.CompanyBranchVn,
                };
            }
            if (CustomerIndex.IsMega)
            {
                headerInvoiceInfo = new MegaHeaderInvoiceInfo()
                {
                    CompanyNameVn = Config.ApplicationSetting.Instance.CompanyNameVn,
                    CompanyBranchVn = Config.ApplicationSetting.Instance.CompanyBranchVn,
                    CompanyAddressRow1 = Config.ApplicationSetting.Instance.CompanyAddressRow1,
                    CompanyAddressRow2 = Config.ApplicationSetting.Instance.CompanyAddressRow2,
                };
            }
            return headerInvoiceInfo;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.MyCompanyUsing(long)'
        public MyUsingModel MyCompanyUsing(long companyId)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'CompanyBusiness.MyCompanyUsing(long)'
        {
            var result = new MyUsingModel();
            var isUsing = companyBo.MyCompanyUsing(companyId);
            if (isUsing > 0)
            {
                logger.Error(this.CurrentUser.Id.ToString(), new Exception($"Store procedure SP_COMPANY_USING return code: {isUsing}"));
            }
            result.isUsing = isUsing > 0;
            return result;
        }
        #endregion
    }
}